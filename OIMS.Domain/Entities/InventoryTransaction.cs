using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using OIMS.Domain.Enums;

namespace OIMS.Domain.Entities
{
    [Table("inventory_transactions")]
    public class InventoryTransaction
    {
        [Key]
        [Column("id")]
        public int Id { get; set; }

        [Required]
        [Column("product_id")]
        public int ProductId { get; set; }

        [Required]
        [Column("change_type")]
        public InventoryChangeType ChangeType { get; set; }

        [Required]
        [Column("quantity_before")]
        public int QuantityBefore { get; set; }

        [Required]
        [Column("quantity_change")]
        public int QuantityChange { get; set; }

        [Required]
        [Column("quantity_after")]
        public int QuantityAfter { get; set; }

        [StringLength(500)]
        [Column("notes")]
        public string? Notes { get; set; }

        [Column("created_by")]
        public int? CreatedBy { get; set; }

        [Column("created_at")]
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        [ForeignKey("ProductId")]
        public virtual Product? Product { get; set; }

        [ForeignKey("CreatedBy")]
        public virtual User? Creator { get; set; }
    }
}
