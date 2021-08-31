using System;
using System.Collections.ObjectModel;
using DataSynchronizor.Model;

namespace DataSynchronizor.ViewModel
{
    public class MainView : BaseView
    {
        private string _syncFolder;
        private string _projectFolder;
        private string _projectProcess;
        private ObservableCollection<Project> _projects = new ObservableCollection<Project>();
        private Project _selectedProject;

        public string ProjectProcess
        {
            get => _projectProcess?.Trim();
            set
            {
                _projectProcess = value;
                RaisePropertiyChanged("ProjectProcess");
            }
        }
        
        public string ProjectFolder
        {
            get => _projectFolder?.Trim();
            set
            {
                _projectFolder = value;
                RaisePropertiyChanged("ProjectFolder");
            }
        }
        
        public Project SelectedProject
        {
            get => _selectedProject;
            set
            {
                _selectedProject = value;
                RaisePropertiyChanged("SelectedProject");
            }
        }
        
        public ObservableCollection<Project> Projects
        {
            get => _projects;
            set
            {
                _projects = value;
                RaisePropertiyChanged("Projects");
            }
        }

        public string SyncFolder
        {
            get => _syncFolder;
            set
            {
                _syncFolder = value;
                RaisePropertiyChanged("SyncFolder");
            }
        }
    }
}