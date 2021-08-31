using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using DataSynchronizor.Model;
using DataSynchronizor.Repository.Base;

namespace DataSynchronizor.Repository
{
    public class ProjectDao : BaseDao
    {
        private ProjectDao()
        {
        }

        private static readonly DbSet<Project> Ctx = DbCtx.Projects;
        private static readonly Lazy<ProjectDao> Instance = new Lazy<ProjectDao>(() => new ProjectDao());

        public static ProjectDao GetInstance => Instance.Value;

        public List<Project> List()
        {
            return Ctx.ToList();
        }

        public Project Add(string name)
        {
            var prj = new Project(name);
            Ctx.Add(prj);
            return prj;
        }

        public bool NameExist(string name)
        {
            return null != Ctx.FirstOrDefault(prj => prj.Name == name);
        }

        public void Delete(Project project)
        {
            Ctx.Remove(project);
        }

        public Project SelectByFolder(string folder)
        {
            return Ctx.SingleOrDefault(p => p.Folder == folder);
        }
    }
}