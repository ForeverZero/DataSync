using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Windows.Media;
using DataSynchronizor.ViewModel;

namespace DataSynchronizor.Model
{
    [Table("t_project")]
    public class Project : BaseView
    {
        public Project(string name)
        {
            Name = name;
            CreateTime = DateTime.Now;
        }

        public Project()
        {
        }

        public const int MaxLenName = 50;

        [Key, Column("id")] public int Id { set; get; }

        [Column("name"), MaxLength(MaxLenName)]
        public string Name { set; get; }

        [Column("folder"), MaxLength(500)] public string Folder { set; get; }
        [Column("last_sync_time")] public DateTime LastSyncTime { get; set; }

        [Column("monitor_process"), MaxLength(100)]
        public string MonitorProcess { get; set; }

        [Column("create_time")] public DateTime CreateTime { get; set; }

        [NotMapped]
        private bool _isConflict;
        [NotMapped]
        public bool IsConflict
        {
            get => _isConflict;
            set
            {
                _isConflict = value;
                RaisePropertiyChanged("ShowColor");
            }
        }

        [NotMapped] public Brush ShowColor => IsConflict ? Brushes.Red : Brushes.Black;

        public override string ToString()
        {
            return Name;
        }
    }
}