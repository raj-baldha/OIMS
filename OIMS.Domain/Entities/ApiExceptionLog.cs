using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace OIMS.Domain.Entities
{
    [Table("api_exception_logs")]
    public class ApiExceptionLog
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

        [StringLength(200)]
        [Column("action_name")]
        public string? ActionName { get; set; }

        [Required]
        [StringLength(200)]
        [Column("exception_type")]
        public string ExceptionType { get; set; } = string.Empty;

        [Required]
        [Column("message")]
        public string Message { get; set; } = string.Empty;

        [Column("stack_trace", TypeName = "nvarchar(max)")]
        public string? StackTrace { get; set; }

        [Column("status_code")]
        public int StatusCode { get; set; }

        [StringLength(100)]
        [Column("error_code")]
        public string? ErrorCode { get; set; }

        [Column("created_at")]
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        [ForeignKey("UserId")]
        public virtual User? User { get; set; }
    }
}
