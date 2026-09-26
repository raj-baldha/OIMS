using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace OIMS.Domain.Entities
{
    [Table("products")]
    public class Product
    {
        [Key]
        [Column("id")]
        public int Id { get; set; }

        [Required]
        [StringLength(200)]
        [Column("name")]
        public string Name { get; set; } = string.Empty;

        [Required]
        [StringLength(50)]
        [Column("sku")]
        public string Sku { get; set; } = string.Empty;

        [Required]
        [Column("category_id")]
        public int CategoryId { get; set; }

        [Required]
        [Range(0, double.MaxValue)]
        [Column("selling_price", TypeName = "decimal(12,2)")]
        public decimal SellingPrice { get; set; }

        [Required]
        [Range(0, double.MaxValue)]
        [Column("cost_price", TypeName = "decimal(12,2)")]
        public decimal CostPrice { get; set; }

        [Required]
        [Range(0, int.MaxValue)]
        [Column("quantity_on_hand")]
        public int QuantityOnHand { get; set; } = 0;

        [Required]
        [Range(0, int.MaxValue)]
        [Column("min_stock_level")]
        public int MinStockLevel { get; set; } = 0;

        [Required]
        [Column("is_active")]
        public bool IsActive { get; set; } = true;

        [Required]
        [Column("version")]
        [ConcurrencyCheck]
        public int Version { get; set; } = 1;

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

        [ForeignKey("CategoryId")]
        public virtual Category? Category { get; set; }

        public virtual ICollection<ProductImage> ProductImages { get; set; } =
            new List<ProductImage>();

        public virtual ICollection<InventoryTransaction> InventoryTransactions { get; set; } =
            new List<InventoryTransaction>();

        public virtual ICollection<OrderItem> OrderItems { get; set; } = new List<OrderItem>();
    }
}
