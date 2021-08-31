using System;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace DataSynchronizor.Model
{
    [Table("t_project_log")]
    public class ProjectLog
    {
        [Key, Column("id")] public int Id { set; get; }
        [Column("project_id")] public int ProjectId { get; set; }
        [Column("time")] public DateTime Time { get; set; }
        [Column("op_type")] public ProjectOpType OpType { get; set; }
        [Column("remark")] public string Remark { get; set; }
    }

    public enum ProjectOpType
    {
        [Description("创建项目")]
        CreateProject = 0,
        [Description("本地变更")]
        LocalChange = 1,
        [Description("同步: 项目 => 同步目录")]
        Upload = 2,
        [Description("同步: 项目 <= 同步目录")]
        Download = 3,
        [Description("发现冲突")]
        Conflict = 4,
        [Description("解决冲突: 项目 => 同步目录")]
        ResolveConflictUseProject = 5,
        [Description("解决冲突: 项目 <= 同步目录")]
        ResolveConflictUseSync = 6,
    }
}