namespace DataSynchronizor.ViewModel
{
    public class SysConfigView : BaseView
    {
        private int _syncTime;
        private bool _isAutoStart;
        private bool _startHide;
        private bool _startMessage;
        private bool _conflictMessage;

        public int SyncTime
        {
            get => _syncTime;
            set
            {
                _syncTime = value;
                RaisePropertiyChanged("SyncTime");
            }
        }

        public bool IsAutoStart
        {
            get => _isAutoStart;
            set
            {
                _isAutoStart = value;
                RaisePropertiyChanged("IsAutoStart");
            }
        }
        
        public bool StartHide
        {
            get => _startHide;
            set
            {
                _startHide = value;
                RaisePropertiyChanged("StartHide");
            }
        }
        public bool StartMessage
        {
            get => _startMessage;
            set
            {
                _startMessage = value;
                RaisePropertiyChanged("StartMessage");
            }
        }
        public bool ConflictMessage
        {
            get => _conflictMessage;
            set
            {
                _conflictMessage = value;
                RaisePropertiyChanged("ConflictMessage");
            }
        }
    }
}