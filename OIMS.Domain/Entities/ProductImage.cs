using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace OIMS.Domain.Entities
{
    [Table("product_images")]
    public class ProductImage
    {
        [Key]
        [Column("id")]
        public int Id { get; set; }

        [Required]
        [Column("product_id")]
        public int ProductId { get; set; }

        [Required]
        [StringLength(1000)]
        [Column("file_url")]
        public string FileUrl { get; set; } = string.Empty;

        [StringLength(255)]
        [Column("original_file_name")]
        public string? OriginalFileName { get; set; }

        [StringLength(255)]
        [Column("stored_file_name")]
        public string? StoredFileName { get; set; }

        [StringLength(100)]
        [Column("content_type")]
        public string? ContentType { get; set; }

        [Column("file_size")]
        public long? FileSize { get; set; }

        [Column("created_at")]
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        [Column("created_by")]
        public int? CreatedBy { get; set; }

        [Column("is_deleted")]
        public bool IsDeleted { get; set; } = false;

        [Column("deleted_at")]
        public DateTime? DeletedAt { get; set; }

        [Column("deleted_by")]
        public int? DeletedBy { get; set; }

        [ForeignKey("ProductId")]
        public virtual Product? Product { get; set; }
    }
}