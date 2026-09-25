using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace OIMS.Domain.Entities
{
    [Table("customers")]
    public class Customer
    {
        [Key]
        [Column("id")]
        public int Id { get; set; }

        [Required]
        [EmailAddress]
        [MaxLength(256)]
        [Column("email")]
        public string Email { get; set; } = string.Empty;

        [Required]
        [MaxLength(500)]
        [Column("password_hash")]
        public string PasswordHash { get; set; } = string.Empty;

        [Required]
        [StringLength(100)]
        [Column("first_name")]
        public string FirstName { get; set; } = string.Empty;

        [Required]
        [StringLength(100)]
        [Column("last_name")]
        public string LastName { get; set; } = string.Empty;

        [StringLength(20)]
        [Column("phone")]
        public string? Phone { get; set; }

        [StringLength(255)]
        [Column("address")]
        public string? Address { get; set; }

        [StringLength(100)]
        [Column("city")]
        public string? City { get; set; }

        [StringLength(100)]
        [Column("state")]
        public string? State { get; set; }

        [StringLength(100)]
        [Column("country")]
        public string? Country { get; set; }

        [Column("is_active")]
        public bool IsActive { get; set; } = true;

        [Column("created_at")]
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        [Column("created_by")]
        public int? CreatedBy { get; set; }

        [Column("updated_at")]
        public DateTime? UpdatedAt { get; set; }

        [Column("updated_by")]
        public int? UpdatedBy { get; set; }

        [Column("is_deleted")]
        public bool IsDeleted { get; set; } = false;

        [Column("deleted_at")]
        public DateTime? DeletedAt { get; set; }

        [Column("deleted_by")]
        public int? DeletedBy { get; set; }

        public virtual ICollection<Order> Orders { get; set; } = new List<Order>();
    }
}