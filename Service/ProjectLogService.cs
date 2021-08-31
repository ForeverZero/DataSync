using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using DataSynchronizor.Model;
using DataSynchronizor.Repository.Base;

namespace DataSynchronizor.Service
{
    public class ProjectLogService : BaseService
    {
        private readonly Lazy<DbSet<ProjectLog>> _dbCtx =
            new Lazy<DbSet<ProjectLog>>(() => BaseDao.GetDbCtx().ProjectLogs);

        private static readonly Lazy<ProjectLogService> Instance =
            new Lazy<ProjectLogService>(() => new ProjectLogService());

        public static ProjectLogService GetInstance => Instance.Value;

        public void Log(Project project, ProjectOpType opType, string remark)
        {
            var ctx = _dbCtx.Value;
            var log = new ProjectLog
            {
                Remark = remark,
                OpType = opType,
                ProjectId = project.Id,
                Time = DateTime.Now
            };
            ctx.Add(log);
            BaseDao.SaveChanges();
        }

        public List<ProjectLog> List(Project project)
        {
            return _dbCtx.Value
                .Where(log => log.ProjectId == project.Id)
                .OrderByDescending(log => log.Time)
                .Take(50).ToList();
        }
    }
}