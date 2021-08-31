using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Windows;
using System.Windows.Threading;
using DataSynchronizor.Model;

namespace DataSynchronizor.Service
{
    public class SyncService : BaseService
    {
        private readonly FileService _fileService = FileService.GetInstance;
        private readonly ProjectService _projectService = ProjectService.GetInstance;
        private readonly DsService _dsService = DsService.GetInstance;
        private HashSet<string> _processes;
        private SysParam _sysParam;
        private static DispatcherTimer _timer;
        private readonly ProjectLogService _logService = ProjectLogService.GetInstance;
        private readonly TrayService _trayService = TrayService.GetInstance;
        private static readonly Lazy<SyncService> Instance = new Lazy<SyncService>(() => new SyncService());

        public static SyncService GetInstance => Instance.Value;

        public void StartSyncTask()
        {
            if (_timer != null)
            {
                return;
            }

            // 系统参数
            _sysParam = _dsService.GetSysParam();

            _timer = new DispatcherTimer
            {
                IsEnabled = true,
                Interval = TimeSpan.FromSeconds(_sysParam.SyncTimeSeconds),
                Tag = null
            };
            _timer.Tick += (o, e) => { Sync(); };
            Logger.Info("定时器启动");
        }

        public void ChangeSyncTime(int syncTime)
        {
            _timer.Interval = TimeSpan.FromSeconds(syncTime);
        }

        /**
         * 同步所有项目
         */
        public void Sync()
        {
            // 同步目录存在
            if (string.IsNullOrEmpty(_sysParam.SyncFolder))
            {
                Logger.Info("未设置同步目录, 不执行同步任务");
                return;
            }

            var syncFolder = new DirectoryInfo(_sysParam.SyncFolder);
            if (!syncFolder.Exists)
            {
                Logger.Warn($"同步目录{syncFolder.FullName}不存在");
                return;
            }

            // 初始化进程列表
            var processes = Process.GetProcesses();
            _processes = new HashSet<string>(processes.Length);
            foreach (var process in processes)
            {
                _processes.Add(process.ProcessName);
            }

            // 逐个同步项目
            var projects = _projectService.List();
            projects.ForEach(Sync);
        }

        private void Sync(Project project)
        {
            if (string.IsNullOrEmpty(project.Folder))
            {
                Logger.Info($"项目[{project.Name}]未设置目录, 跳过");
                return;
            }

            // 监控进程存在， 跳过执行
            if (_processes.Contains(project.MonitorProcess))
            {
                Logger.Info($"项目: {project.Name}, 进程: {project.MonitorProcess}正在运行, 跳过本次执行");
                return;
            }

            // 项目目录不存在， 跳过
            var prjFdr = new DirectoryInfo(project.Folder);
            if (!prjFdr.Exists)
            {
                Logger.Warn($"项目: {project.Name}, 目录{project.Folder}不存在, 跳过");
                return;
            }

            // 同步目录的控制信息
            var pathSyncFolderProjectCtlFile =
                new FileInfo($"{_sysParam.SyncFolder}\\{project.Name}\\{FileService.DsCtlFileName}");

            // 项目目录的控制信息
            var projectCtlFile = new FileInfo($"{project.Folder}\\{FileService.DsCtlFileName}");
            var projectCtl = _fileService.GetLatestCtl(projectCtlFile);

            // 如果控制文件不存在 或者 文件夹变更日期晚于控制文件的最新日期, 写控制文件
            var projectChangeTime = _fileService.GetLatestChangeTime(project.Folder);
            if ((projectCtl == null && projectChangeTime != null) ||
                (projectCtl != null && projectChangeTime?.CompareTo(projectCtl.Time) > 0))
            {
                Logger.Info($"项目[{project.Name}]发现变更, 时间: {projectChangeTime}");
                _fileService.WriteCtlFile(project, DateTime.Now);
            }

            // 同步目录中, 本项目的目录
            var syncFolderProjectFolder = $"{_sysParam.SyncFolder}\\{project.Name}";

            // 控制文件的比较 拷贝文件逻辑
            var ctlCompareResult = _fileService.CompareCtlFile(projectCtlFile, pathSyncFolderProjectCtlFile);
            switch (ctlCompareResult)
            {
                case DsCtlCompareResult.Equal:
                    // 相等, 直接退出
                    return;
                case DsCtlCompareResult.Earlier:
                    // 更小, 从同步目录拷贝到项目目录
                    _fileService.Copy(syncFolderProjectFolder, project.Folder);
                    _logService.Log(project, ProjectOpType.Download, _fileService.GetLatestCtl(pathSyncFolderProjectCtlFile).Hash);
                    return;
                case DsCtlCompareResult.Later:
                    // 更大, 从项目目录拷贝到同步目录
                    _fileService.Copy(project.Folder, syncFolderProjectFolder);
                    _logService.Log(project, ProjectOpType.Upload, _fileService.GetLatestCtl(projectCtlFile).Hash);
                    return;
                case DsCtlCompareResult.Conflict:
                    // 标记为冲突
                    if (project.IsConflict)
                    {
                        return;
                    }
                    
                    // 冲突提示
                    if (_sysParam.ConflictMessage)
                    {
                        _trayService.Notify(null, $"项目[{project.Name}]发现冲突, 请处理");
                    }
                    _logService.Log(project, ProjectOpType.Conflict, string.Empty);
                    project.IsConflict = true;
                    return;
                default:
                    // 理论上不会走到这里
                    return;
            }
        }
    }
}