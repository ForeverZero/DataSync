using System;
using System.Linq;
using DataSynchronizor.Model;
using DataSynchronizor.Repository.Base;

namespace DataSynchronizor.Repository
{
    public class SysParamDao : BaseDao
    {
        private SysParamDao()
        {
        }

        private static readonly Lazy<SysParamDao> Instance = new Lazy<SysParamDao>(() => new SysParamDao());
        private readonly Lazy<SysParam> _sysParamInstance = new Lazy<SysParam>(() => DbCtx.SysParams.SingleOrDefault());

        public static SysParamDao GetInstance => Instance.Value;

        public SysParam Get()
        {
            return _sysParamInstance.Value;
        }
    }
}