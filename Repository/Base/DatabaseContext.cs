using System.Data.Entity;
using DataSynchronizor.Model;

namespace DataSynchronizor.Repository.Base
{
    public class DatabaseContext : DbContext
    {
        public DbSet<Scheme> Schemes { get; set; }
        public DbSet<SysParam> SysParams { get; set; }
        public DbSet<Project> Projects { get; set; }
        public DbSet<ProjectLog> ProjectLogs { get; set; }
    }
}