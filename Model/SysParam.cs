using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace DataSynchronizor.Model
{
    [Table("t_sys_param")]
    public class SysParam
    {
        [Key, Column("id")] public int Id { set; get; }

        [Column("sync_folder"), MaxLength(500)]
        public string SyncFolder { set; get; }
        
        [Column("sync_time_seconds")]
        public int SyncTimeSeconds { get; set; }
        
        [Column("is_auto_start")]
        public bool IsAutoStart { get; set; }
        
        [Column("start_hide")]
        public bool StartHide { get; set; }
        
        [Column("start_message")]
        public bool StartMessage { get; set; }
        
        [Column("conflict_message")]
        public bool ConflictMessage { get; set; }
    }
}