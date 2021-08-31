using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Input;
using DataSynchronizor.ViewModel;

namespace DataSynchronizor.View
{
    public partial class SysConfigDialog : Window
    {
        private readonly SysConfigView _sysConfigView;

        public int SyncTime => _sysConfigView.SyncTime;
        public bool IsAutoStart => _sysConfigView.IsAutoStart;
        public bool StartHide => _sysConfigView.StartHide;
        public bool StartMessage => _sysConfigView.StartMessage;
        public bool ConflictMessage => _sysConfigView.ConflictMessage;
        
        public bool IsOk { get; set; }
        
        public SysConfigDialog(Window owner, int syncTime, bool isAutoStart, bool startHide, bool startMessage, bool conflictMessage)
        {
            
            InitializeComponent();

            _sysConfigView = DataContext as SysConfigView;
            _sysConfigView.SyncTime = syncTime;
            _sysConfigView.IsAutoStart = isAutoStart;
            _sysConfigView.ConflictMessage = conflictMessage;
            _sysConfigView.StartHide = startHide;
            _sysConfigView.StartMessage = startMessage;
            IsOk = false;
            
            Owner = owner;
            WindowStartupLocation = WindowStartupLocation.CenterOwner;
        }

        private void BtnOk_Click(object sender, RoutedEventArgs e)
        {
            IsOk = true;
            Close();
        }

        private void BtnCancel_Click(object sender, RoutedEventArgs e)
        {
            IsOk = false;
            Close();
        }

        private void TbSyncTime_PreviewTextInput(object sender, System.Windows.Input.TextCompositionEventArgs e)
        {
            var re = new Regex("[^0-9.-]+");
            e.Handled = re.IsMatch(e.Text);
        }

        private void UIElement_OnMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            CbAutoStart.IsChecked = !CbAutoStart.IsChecked;
        }
    }
}