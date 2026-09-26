using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace OIMS.Domain.Entities
{
    [Table("audit_logs")]
    public class AuditLog
    {
        [Key]
        [Column("id")]
        public int Id { get; set; }

        [Column("user_id")]
        public int? UserId { get; set; }

        [Required]
        [StringLength(50)]
        [Column("action")]
        public string Action { get; set; } = string.Empty;

        [Required]
        [StringLength(100)]
        [Column("entity_name")]
        public string EntityName { get; set; } = string.Empty;

        [Required]
        [Column("entity_id")]
        public int EntityId { get; set; }

        [Column("old_values", TypeName = "nvarchar(max)")]
        public string? OldValues { get; set; }

        [Column("new_values", TypeName = "nvarchar(max)")]
        public string? NewValues { get; set; }

        [Column("created_at")]
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        [ForeignKey("UserId")]
        public virtual User? User { get; set; }
    }
}
