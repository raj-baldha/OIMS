using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using OIMS.Domain.Entities;

[Table("order_documents")]
public class OrderDocument
{
    [Key]
    [Column("id")]
    public int Id { get; set; }

    [Required]
    [Column("order_id")]
    public int OrderId { get; set; }

    [Required]
    [StringLength(1000)]
    [Column("file_url")]
    public string FileUrl { get; set; } = string.Empty;

    [Required]
    [StringLength(255)]
    [Column("stored_file_name")]
    public string StoredFileName { get; set; } = string.Empty;

    [Required]
    [StringLength(100)]
    [Column("content_type")]
    public string ContentType { get; set; } = "application/pdf";

    [Required]
    [Column("file_size")]
    public long FileSize { get; set; }

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

    [ForeignKey("OrderId")]
    public virtual Order? Order { get; set; }
}
