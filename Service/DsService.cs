using System;
using System.Windows.Forms;
using DataSynchronizor.Common;
using DataSynchronizor.Model;
using DataSynchronizor.Repository;
using Microsoft.Win32;

namespace DataSynchronizor.Service
{
    public class DsService : BaseService
    {
        private DsService()
        {
        }

        private static readonly Lazy<DsService> Instance = new Lazy<DsService>(() => new DsService());
        private readonly SysDao _sysDao = SysDao.GetInstance;
        private readonly SysParamDao _sysParamDao = SysParamDao.GetInstance;

        private readonly RegistryKey _rkApp =
            Registry.CurrentUser.OpenSubKey("SOFTWARE\\Microsoft\\Windows\\CurrentVersion\\Run", true);

        public static DsService GetInstance => Instance.Value;

        public void Init()
        {
            _sysDao.InitDb();
        }

        public SysParam GetSysParam()
        {
            return _sysParamDao.Get();
        }

        public bool IsAutoStart()
        {
            return _rkApp.GetValue(CommonConstant.KeyAppName) != null;
        }

        public void SetAutoStart(bool isAutoStart)
        {
            if (isAutoStart)
            {
                _rkApp.SetValue(CommonConstant.KeyAppName, CommonConstant.AppExePath);
            }
            else
            {
                _rkApp.DeleteValue(CommonConstant.KeyAppName, false);
            }
        }
    }
}