using System;
using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Forms;
using System.Windows.Input;
using DataSynchronizor.Model;
using DataSynchronizor.Service;
using DataSynchronizor.Util;
using DataSynchronizor.View;
using DataSynchronizor.ViewModel;
using Application = System.Windows.Application;
using ContextMenu = System.Windows.Forms.ContextMenu;
using KeyEventArgs = System.Windows.Input.KeyEventArgs;
using MenuItem = System.Windows.Forms.MenuItem;
using MessageBox = System.Windows.MessageBox;
using MouseEventArgs = System.Windows.Forms.MouseEventArgs;

namespace DataSynchronizor
{
    /// <summary>
    /// MainWindow.xaml 的交互逻辑
    /// </summary>
    public partial class MainWindow : Window
    {
        private readonly MainView _mainView;
        private readonly DsService _dsService = DsService.GetInstance;
        private readonly SysParam _sysParam;
        private readonly ProjectService _projectService = ProjectService.GetInstance;
        private readonly SyncService _syncService = SyncService.GetInstance;
        private readonly TrayService _trayService = TrayService.GetInstance;
        private readonly ProjectLogService _logService = ProjectLogService.GetInstance;

        public MainWindow()
        {
            WindowStartupLocation = WindowStartupLocation.CenterScreen;
            InitializeComponent();
            // 初始化系统
            _dsService.Init();
            // 加载系统参数
            _sysParam = _dsService.GetSysParam();
            // 加载托盘图标
            InitialTray();
            // VM
            _mainView = DataContext as MainView;
            // 初始化窗口数据
            InitWindowData();
            // 启动定时任务
            _syncService.StartSyncTask();
            
            //隐藏
            if (_sysParam.StartHide)
            {
                Visibility = Visibility.Hidden;
            }
            // 提示信息
            if (_sysParam.StartMessage)
            {
                _trayService.Notify(null, "Data Sync 已启动");
            }                        
        }

         private NotifyIcon _notifyIcon;
        private void InitialTray()
        {

            _notifyIcon = _trayService.GetIcon();

            // 鼠标事件
            _notifyIcon.MouseClick += NotifyIconMouseClick;
            //设置菜单项
            var mOption = new MenuItem("选项");
            mOption.Click += Config;

            //退出菜单项
            var mExit = new MenuItem("退出");
            mExit.Click += ExitClick;

            //关联托盘控件
            var children = new[] { mOption , mExit };
            _notifyIcon.ContextMenu = new ContextMenu(children);

            //窗体状态改变时候触发
            StateChanged += SysTrayStateChanged;
            Closing += (sender, args) =>
            {
                args.Cancel = true;
                ToggleHiddenWindow();
            };
        }

        /**
         * 窗体状态改变时候触发
         */
        private void SysTrayStateChanged(object sender, EventArgs e)
        {
            if (WindowState == WindowState.Minimized)
            {
                Visibility = Visibility.Hidden;
            }
        }

        /**
         * 保存配置
         */
        private void Config(object sender, EventArgs e)
        {
            var scd = new SysConfigDialog(this, _sysParam.SyncTimeSeconds, _dsService.IsAutoStart(), _sysParam.StartHide, _sysParam.StartMessage, _sysParam.ConflictMessage);
            scd.ShowDialog();

            if (!scd.IsOk)
            {
                return;
            }

            _sysParam.SyncTimeSeconds = scd.SyncTime;
            _sysParam.ConflictMessage = scd.ConflictMessage;
            _sysParam.StartHide = scd.StartHide;
            _sysParam.StartMessage = scd.StartMessage;
            _syncService.ChangeSyncTime(scd.SyncTime);
            _syncService.SaveDbChanges();
            
            _dsService.SetAutoStart(scd.IsAutoStart);
        }
        
        /**
         * 退出选项
         */
        private void ExitClick(object sender, EventArgs e)
        {
            if (MessageBox.Show("确定要关闭吗?",
                "退出",
                MessageBoxButton.YesNo,
                MessageBoxImage.Question,
                MessageBoxResult.No) != MessageBoxResult.Yes) return;
            _notifyIcon.Dispose();
            Application.Current.Shutdown();
        }

        /**
         * 鼠标单击
         */
        private void NotifyIconMouseClick(object sender, MouseEventArgs e)
        {
            if (e.Button != MouseButtons.Left) return;
            ToggleHiddenWindow();
        }

        private void ToggleHiddenWindow()
        {
            if (Visibility == Visibility.Visible)
            {
                Visibility = Visibility.Hidden;
            }
            else
            {
                Visibility = Visibility.Visible;
                Activate();
            }
        }

        private void InitWindowData()
        {

            Title = $"{Title} - {SystemInformation.ComputerName}";
            // 同步目录
            _mainView.SyncFolder = _sysParam.SyncFolder;
            // 项目列表
            var projects = _projectService.List();
            var obPrj = new ObservableCollection<Project>();
            foreach (var project in projects)
            {
                obPrj.Add(project);
            }

            _mainView.Projects = obPrj;
        }

        private void BtnSelectSyncFolder_Click(object sender, RoutedEventArgs e)
        {
            var openFileDialog = new FolderBrowserDialog();
            if (openFileDialog.ShowDialog() != System.Windows.Forms.DialogResult.OK)
            {
                return;
            }
            _mainView.SyncFolder = openFileDialog.SelectedPath;
            _sysParam.SyncFolder = openFileDialog.SelectedPath;
            _dsService.SaveDbChanges();
        }

        private void BtnAdd_OnClick(object sender, RoutedEventArgs e)
        {
            var id = new InputDialog(this, "新建项目");
            id.ShowDialog();
            if (!id.IsOk)
            {
                return;
            }
            
            Project project;
            try
            {
                project = _projectService.Add(id.Text.Trim());
            }
            catch (Exception exception)
            {
                MessageBox.Show(exception.Message, "错误", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }
            _mainView.Projects.Add(project);
        }

        private void BtnDel_OnClick(object sender, RoutedEventArgs e)
        {
            // 没有选中  什么都不执行
            if (_mainView.SelectedProject == null)
            {
                return;
            }
            
            // 弹框确认是否
            var yesNo = MessageBox.Show($"是否要删除项目: {_mainView.SelectedProject}", 
                "删除项目", MessageBoxButton.YesNo, MessageBoxImage.Question);
            if (yesNo != MessageBoxResult.Yes)
            {
                return;
            }
            
            // 从Db删除
            _projectService.Delete(_mainView.SelectedProject);
            _projectService.SaveDbChanges();
            // 从页面移除
            _mainView.Projects.Remove(_mainView.SelectedProject);
        }

        private void BtnSelectProjectFolder_OnClick(object sender, RoutedEventArgs e)
        {
            // 没选中项目 不执行
            var selectedProject = _mainView.SelectedProject;
            if (selectedProject == null)
            {
                return;
            }
            
            var openFileDialog = new FolderBrowserDialog();
            if (openFileDialog.ShowDialog() != System.Windows.Forms.DialogResult.OK)
            {
                return;
            }
            
            // 项目目录不能和同步目录相同
            var path = openFileDialog.SelectedPath;
            if (path == _mainView.SyncFolder)
            {
                MessageBox.Show("项目目录不能和同步目录相同", "错误", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }
            
            // 项目目录必须唯一
            var sameFolderPrj = _projectService.SelectByFolder(path);
            if (sameFolderPrj != null && sameFolderPrj.Id != selectedProject.Id)
            {
                MessageBox.Show($"目录{path}已存在于项目[{selectedProject}]", "错误", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }
            
            // 更新项目目录
            _mainView.ProjectFolder = path;
            selectedProject.Folder = path;
            _projectService.SaveDbChanges();
        }

        private void LbProjects_OnSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var project = _mainView.SelectedProject;
            _mainView.ProjectFolder = project == null ? string.Empty : project.Folder;
            _mainView.ProjectProcess = project == null ? string.Empty : project.MonitorProcess;
            LoadProjectLog();
        }

        private void LoadProjectLog()
        {
            var project = _mainView.SelectedProject;
            DgProjectLog.Items.Clear();

            // 未选中 清空退出
            if (project == null)
            {
                return;
            }
            
            // 选中 加载日志
            var logs = _logService.List(project);
            logs.ForEach(log => DgProjectLog.Items.Add(new
                {
                    Time = DateUtil.ToStr(log.Time), 
                    TypeDisplay = EnumUtil.GetEnumDescription(log.OpType), 
                    Type = log.OpType, 
                    Direction = log.Remark
                }));
        }

        private void TbProjectProcess_OnLostFocus(object sender, RoutedEventArgs e)
        {
            var project = _mainView.SelectedProject;
            
            // 未选中 直接返回
            if (project == null)
            {
                return;
            }
            
            // 更新
            project.MonitorProcess = TbProjectProcess.Text;
            _projectService.SaveDbChanges();
        }

        private void TbProjectProcess_OnKeyDown(object sender, KeyEventArgs e)
        {
            // 回车或Esc
            if (e.Key != Key.Enter && e.Key != Key.Escape)
            {
                return;
            }

            TbProjectProcess_OnLostFocus(sender, e);
        }

        private void BtnSelectProjectProcess_OnClick(object sender, RoutedEventArgs e)
        {
            var selectedPrj = _mainView.SelectedProject;

            if (selectedPrj == null)
            {
                return;
            }
            
            var spd = new SelectProcessDialog(this);
            spd.ShowDialog();

            if (!spd.IsOk)
            {
                return;
            }

            // 更新DB
            selectedPrj.MonitorProcess = spd.ProcessName;
            _projectService.SaveDbChanges();
            
            // 更新界面
            _mainView.ProjectProcess = spd.ProcessName;
        }

        private void BtnSolveConflict_OnClick(object sender, RoutedEventArgs e)
        {
            if (_mainView.SelectedProject == null)
            {
                return;
            }

            if (!_mainView.SelectedProject.IsConflict)
            {
                MessageBox.Show($"项目[{_mainView.SelectedProject.Name}]不存在冲突", "错误", MessageBoxButton.OK,
                    MessageBoxImage.Error
                );
                return;
            }
            // 冲突处理逻辑
            var rcd = new ResolveConflictDialog(this, _mainView.SelectedProject);
            rcd.ShowDialog();
        }

        private void DgProjectLog_OnSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            DgProjectLog.UnselectAllCells();
        }
    }
}