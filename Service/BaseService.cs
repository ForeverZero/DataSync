using DataSynchronizor.Repository.Base;
using NLog;

namespace DataSynchronizor.Service
{
    public class BaseService
    {
        protected static readonly Logger Logger = LogManager.GetCurrentClassLogger();

        public void SaveDbChanges()
        {
            BaseDao.SaveChanges();
        }
    }
}