using NLog;

namespace DataSynchronizor.Repository.Base
{
    public abstract class BaseDao
    {
        protected static readonly Logger Logger = LogManager.GetCurrentClassLogger();
        protected static readonly DatabaseContext DbCtx = new DatabaseContext();

        public static void SaveChanges()
        {
            DbCtx.SaveChanges();
        }

        public static DatabaseContext GetDbCtx()
        {
            return DbCtx;
        }
    }
}