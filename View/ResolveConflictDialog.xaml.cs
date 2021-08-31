using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using DataSynchronizor.Model;
using DataSynchronizor.Service;
using DataSynchronizor.Util;

namespace DataSynchronizor.View
{
    public partial class ResolveConflictDialog : Window
    {
        private readonly FileService _fileService = FileService.GetInstance;
        private readonly DsService _dsService = DsService.GetInstance;
        private readonly ProjectLogService _logService = ProjectLogService.GetInstance;
        private readonly Project _project;
        private readonly SysParam _sysParam;
        private string _hashSync;
        private string _hashProject;

        public ResolveConflictDialog(Window owner, Project project)
        {
            Owner = owner;
            WindowStartupLocation = WindowStartupLocation.CenterOwner;
            _project = project;
            _sysParam = _dsService.GetSysParam();
            InitializeComponent();
            InitWindowData();
        }

        private void InitWindowData()
        {
            // 加载同步目录、项目目录下的控制文件
            var syncCtlList = _fileService.ListSyncCtl(_project);
            var projectCtlList = _fileService.ListProjectCtl(_project);

            // 把Hash存进Set, 一方的Hash在另一方Set中不存在, 说明这条Hash是冲突的
            var syncHashSet = new HashSet<string>(syncCtlList.Count);
            syncCtlList.ForEach(ctl => syncHashSet.Add(ctl.Hash));
            var projectHashSet = new HashSet<string>(projectCtlList.Count);
            projectCtlList.ForEach(ctl => projectHashSet.Add(ctl.Hash));

            // 数据添加进表格, 黄标冲突
            syncCtlList.ForEach(ctl =>
            {
                DgSyncCtl.Items.Add(new
                {
                    ctl.ComputerName, Time = DateUtil.ToStr(ctl.Time), ctl.Hash,
                    IsConflict = !projectHashSet.Contains(ctl.Hash)
                });
            });
            _hashSync = syncCtlList[0].Hash;

            // 数据添加进表格, 黄标冲突
            projectCtlList.ForEach(ctl =>
            {
                DgProjectCtl.Items.Add(new
                {
                    ctl.ComputerName, Time = DateUtil.ToStr(ctl.Time), ctl.Hash,
                    IsConflict = !syncHashSet.Contains(ctl.Hash)
                });
            });
            _hashProject = projectCtlList[0].Hash;
        }

        private void ResolveConflictDialog_OnKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Escape)
            {
                Close();
            }
        }

        private void DgSyncCtl_OnSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            DgSyncCtl.UnselectAllCells();
            DgProjectCtl.UnselectAllCells();
        }

        /**
         * 使用 项目数据
         */
        private void BtnUseProjectData_OnClick(object sender, RoutedEventArgs e)
        {
            var syncProjectFolder = $"{_sysParam.SyncFolder}\\{_project.Name}";
            var projectFolder = _project.Folder;
            _fileService.Copy(projectFolder, syncProjectFolder);
            _project.IsConflict = false;
            _logService.Log(_project, ProjectOpType.ResolveConflictUseProject, _hashProject);
            Close();
        }

        private void BtnUseSyncData_OnClick(object sender, RoutedEventArgs e)
        {
            var syncProjectFolder = $"{_sysParam.SyncFolder}\\{_project.Name}";
            var projectFolder = _project.Folder;
            _fileService.Copy(syncProjectFolder, projectFolder);
            _project.IsConflict = false;
            _logService.Log(_project, ProjectOpType.ResolveConflictUseSync, _hashSync);
            Close();
        }
    }
}