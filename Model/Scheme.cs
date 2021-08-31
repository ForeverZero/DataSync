using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace DataSynchronizor.Model
{
    [Table("t_scheme")]
    public class Scheme
    {
        [Key, Column("id")] public int Id { set; get; }

        [Column("database_version")] public int DatabaseVersion { set; get; }
    }
}