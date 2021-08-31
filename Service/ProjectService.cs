using System;
using System.Collections.Generic;
using DataSynchronizor.Model;
using DataSynchronizor.Repository;

namespace DataSynchronizor.Service
{
    public class ProjectService : BaseService
    {
        private readonly ProjectDao _projectDao = ProjectDao.GetInstance;
        private static readonly Lazy<ProjectService> Instance = new Lazy<ProjectService>(() => new ProjectService());
        private readonly ProjectLogService _logService = ProjectLogService.GetInstance;

        public static ProjectService GetInstance => Instance.Value;

        public Project Add(string name)
        {
            Logger.Info($"新增项目: {name}");
            // 项目名称长度
            if (name.Length > Project.MaxLenName)
            {
                throw new Exception($"项目名称长度不能超过{Project.MaxLenName}");
            }
            // 项目名称防重
            if (_projectDao.NameExist(name))
            {
                throw new Exception($"项目{name}已存在");
            }

            var project = _projectDao.Add(name);
            SaveDbChanges();
            _logService.Log(project, ProjectOpType.CreateProject, string.Empty);
            return project;
        }

        public List<Project> List()
        {
            return _projectDao.List();
        }

        public void Delete(Project project)
        {
            _projectDao.Delete(project);
        }

        public Project SelectByFolder(string folder)
        {
            return _projectDao.SelectByFolder(folder);
        }
    }
}