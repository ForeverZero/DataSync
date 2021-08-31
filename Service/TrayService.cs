using System;
using System.Drawing;
using System.Reflection;
using System.Windows.Forms;

namespace DataSynchronizor.Service
{
    public class TrayService : BaseService
    {
        #region 单例
        private static readonly Lazy<TrayService> Instance = new Lazy<TrayService>(() => new TrayService());
        public static TrayService GetInstance => Instance.Value;
        #endregion

        private NotifyIcon _notifyIcon;

        #region 初始化托盘图标
        public NotifyIcon GetIcon()
        {
            // 已经初始化 直接返回
            if (_notifyIcon != null)
            {
                return _notifyIcon;
            }
            
            // 还没初始化,初始化后返回
            var assembly = Assembly.GetExecutingAssembly();
            var stream = assembly.GetManifestResourceStream("DataSynchronizor.Resources.Ico.ds.ico");
            
            //设置托盘的各个属性
            _notifyIcon = new NotifyIcon();
            _notifyIcon.Text = "Data Sync";
            _notifyIcon.Icon = new Icon(stream);
            _notifyIcon.Visible = true;

            return _notifyIcon;
        }
        #endregion

        #region 气泡通知

        public void Notify(string title, string text)
        {
            // 未初始化 直接返回
            if (_notifyIcon == null)
            {
                return;
            }

            _notifyIcon.BalloonTipText = text;
            _notifyIcon.BalloonTipTitle = title;
            _notifyIcon.ShowBalloonTip(2000);
        }
        #endregion
    }
}