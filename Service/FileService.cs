using System;
using System.Collections.Generic;
using System.IO;
using System.Windows.Forms;
using DataSynchronizor.Model;
using DataSynchronizor.Util;

namespace DataSynchronizor.Service
{
    class FileService : BaseService
    {
        private FileService()
        {
        }

        private static readonly Lazy<FileService> Instance = new Lazy<FileService>(() => new FileService());
        public const string DsCtlFileName = "DsCtl";
        private readonly DsService _dsService = DsService.GetInstance;
        private readonly ProjectLogService _logService = ProjectLogService.GetInstance;
        
        public static FileService GetInstance => Instance.Value;

        public DateTime? GetLatestChangeTime(string folder)
        {
            DateTime? changeTime = null;
            var baseDir = new DirectoryInfo(folder);

            // 找出文件的最后写入日期
            var files = baseDir.GetFiles();
            foreach (var file in files)
            {
                // 跳过控制文件
                if (file.Name == DsCtlFileName)
                {
                    continue;
                }

                if (file.LastWriteTime.CompareTo(changeTime) > 0)
                {
                    changeTime = file.LastWriteTime;
                }
            }

            // 递归找子目录
            var subFolders = baseDir.GetDirectories();
            foreach (var subDir in subFolders)
            {
                var folderTime = GetLatestChangeTime(subDir.FullName);
                if (folderTime?.CompareTo(changeTime) > 0)
                {
                    changeTime = folderTime;
                }
            }

            return changeTime;
        }

        public void Copy(string sourceDir, string targetDir)
        {
            // 源目录不存在 退出
            var sourceDirInfo = new DirectoryInfo(sourceDir);
            if (!sourceDirInfo.Exists)
            {
                Logger.Warn($"目录{sourceDir}不存在");
                return;
            }

            // 目标存在, 删除
            var targetDirInfo = new DirectoryInfo(targetDir);
            DeleteFolder(targetDirInfo);
            Directory.CreateDirectory(targetDir);

            // 拷贝文件
            var files = sourceDirInfo.GetFiles();
            foreach (var file in files)
            {
                file.CopyTo($"{targetDir}\\{file.Name}");
            }

            // 拷贝子文件夹
            var subDirs = sourceDirInfo.GetDirectories();
            foreach (var subDir in subDirs)
            {
                var targetSubDir = $"{targetDir}\\{subDir.Name}";
                var sourceSubDir = subDir.FullName;
                // 递归执行
                Copy(sourceSubDir, targetSubDir);
            }
        }

        public void DeleteFolder(DirectoryInfo dir)
        {
            if (!dir.Exists)
            {
                return;
            }

            var files = dir.GetFiles();
            foreach (var file in files)
            {
                file.Delete();
            }

            var subDirs = dir.GetDirectories();
            foreach (var subDir in subDirs)
            {
                DeleteFolder(subDir);
            }
        }

        public void WriteCtlFile(Project project, DateTime time)
        {
            var fdr = project.Folder;
            var ctlFilePath = $"{fdr}\\{DsCtlFileName}";
            var ctlFile = new FileInfo(ctlFilePath);
            if (!ctlFile.Exists)
            {
                // 创建文件并隐藏
                ctlFile.Create().Close();
            }

            // 生成控制信息
            var baseInfo = $"{SystemInformation.ComputerName};{time:yyyy/MM/dd HH:mm:ss}";
            var hash = HashUtil.Sha1Signature(baseInfo);
            var ctnInfo = $"{baseInfo};{hash}";

            // 在文件头部写信息, 先读出原来的信息
            var fsr = ctlFile.OpenRead();
            var sr = new StreamReader(fsr);
            var content = sr.ReadToEnd();
            sr.Close();
            fsr.Close();

            // 写入新的内容
            var fsw = ctlFile.OpenWrite();
            content = $"{ctnInfo}\n{content}";
            var sw = new StreamWriter(fsw);
            sw.Write(content);
            sw.Close();
            fsw.Close();
            
            // 写日志
            _logService.Log(project, ProjectOpType.LocalChange, hash);
        }

        public DsCtl GetLatestCtl(FileInfo fileInfo)
        {
            if (!fileInfo.Exists)
            {
                return null;
            }

            using (var fs = fileInfo.OpenRead())
            {
                using (var fsr = new StreamReader(fs))
                {
                    var ctlLine = fsr.ReadLine();
                    return GetCtl(ctlLine);
                }
            }
        }

        public DsCtl GetCtl(string ctlLine)
        {
            if (string.IsNullOrEmpty(ctlLine))
            {
                return null;
            }

            var part = ctlLine.Split(';');
            if (part.Length < 3)
            {
                return null;
            }

            return new DsCtl
            {
                ComputerName = part[0],
                Time = DateUtil.ToDateTime(part[1]),
                Hash = part[2]
            };
        }

        public DsCtlCompareResult CompareCtlFile(FileInfo ctl1, FileInfo ctl2)
        {
            var ctl1Hash = GetLatestCtl(ctl1)?.Hash;
            var ctl2Hash = GetLatestCtl(ctl2)?.Hash;

            // 如果有一方为null, 非null方更大
            if (ctl1Hash != null && ctl2Hash == null)
            {
                return DsCtlCompareResult.Later;
            }

            if (ctl1Hash == null && ctl2Hash != null)
            {
                return DsCtlCompareResult.Earlier;
            }

            // 相等
            if (ctl1Hash == ctl2Hash || ctl1Hash.Equals(ctl2Hash))
            {
                return DsCtlCompareResult.Equal;
            }

            // 更小, ctl1 hash存在与 ctl2历史hash中
            using (var fs = ctl2.OpenRead())
            {
                using (var fsr = new StreamReader(fs))
                {
                    string line;
                    while ((line = fsr.ReadLine()) != null)
                    {
                        var dsCtl = GetCtl(line);
                        if (ctl1Hash.Equals(dsCtl.Hash))
                        {
                            return DsCtlCompareResult.Earlier;
                        }
                    }
                }
            }

            // 更大, ctl2 hash存在于ctl1历史hash中
            using (var fs = ctl1.OpenRead())
            {
                using (var fsr = new StreamReader(fs))
                {
                    string line;
                    while ((line = fsr.ReadLine()) != null)
                    {
                        var dsCtl = GetCtl(line);
                        if (ctl2Hash.Equals(dsCtl.Hash))
                        {
                            return DsCtlCompareResult.Later;
                        }
                    }
                }
            }

            // 其余情况 冲突
            return DsCtlCompareResult.Conflict;
        }

        public List<DsCtl> ListCtl(FileInfo ctlFile)
        {
            var list = new List<DsCtl>();
            using (var fs = ctlFile.OpenText())
            {
                string line;
                while ((line = fs.ReadLine()) != null)
                {
                    list.Add(GetCtl(line));
                }
            }

            return list;
        }
        
        public List<DsCtl> ListSyncCtl(Project project)
        {
            var sysParam = _dsService.GetSysParam();
            var syncCtlPath = $"{sysParam.SyncFolder}\\{project.Name}\\{DsCtlFileName}";
            return ListCtl(new FileInfo(syncCtlPath));
        }
        
        public List<DsCtl> ListProjectCtl(Project project)
        {
            var syncCtlPath = $"{project.Folder}\\{DsCtlFileName}";
            return ListCtl(new FileInfo(syncCtlPath));
        }
    }
}