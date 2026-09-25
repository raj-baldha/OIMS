using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace OIMS.Domain.Entities
{
    [Table("refresh_tokens")]
    public class RefreshToken
    {
        [Key]
        [Column("id")]
        public int Id { get; set; }

        [Required]
        [Column("user_id")]
        public int UserId { get; set; }

        [Required]
        [StringLength(500)]
        [Column("token")]
        public string Token { get; set; } = string.Empty;

        [Required]
        [Column("expires_at")]
        public DateTime ExpiresAt { get; set; }

        [Column("created_at")]
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        [Column("is_revoked")]
        public bool IsRevoked { get; set; } = false;

        [Column("revoked_at")]
        public DateTime? RevokedAt { get; set; }

        [StringLength(500)]
        [Column("replaced_by_token")]
        public string? ReplacedByToken { get; set; }

        [ForeignKey("UserId")]
        public virtual User? User { get; set; }
    }
}