using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace OIMS.Domain.Entities
{
    [Table("api_execution_logs")]
    public class ApiExecutionLog
    {
        [Key]
        [Column("id")]
        public int Id { get; set; }

        [Column("user_id")]
        public int? UserId { get; set; }

        [Required]
        [StringLength(10)]
        [Column("http_method")]
        public string HttpMethod { get; set; } = string.Empty;

        [Required]
        [StringLength(500)]
        [Column("request_path")]
        public string RequestPath { get; set; } = string.Empty;

        [Required]
        [StringLength(200)]
        [Column("action_name")]
        public string ActionName { get; set; } = string.Empty;

        [Column("status_code")]
        public int StatusCode { get; set; }

        [Column("execution_time_ms")]
        public long ExecutionTimeMs { get; set; }

        [Column("created_at")]
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        [ForeignKey("UserId")]
        public virtual User? User { get; set; }
    }
}
