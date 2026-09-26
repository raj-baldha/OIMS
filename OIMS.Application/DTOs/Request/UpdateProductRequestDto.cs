using System.ComponentModel.DataAnnotations;

namespace OIMS.Application.DTOs.Request
{
    public class UpdateProductRequestDto
    {
        [Required]
        [StringLength(200)]
        public string Name { get; set; } = string.Empty;

        [Required]
        [StringLength(50)]
        public string Sku { get; set; } = string.Empty;

        [Required]
        public int CategoryId { get; set; }

        [Range(0, double.MaxValue)]
        public decimal SellingPrice { get; set; }

        [Range(0, double.MaxValue)]
        public decimal CostPrice { get; set; }

        [Range(0, int.MaxValue)]
        public int MinStockLevel { get; set; }

        public bool IsActive { get; set; }
    }
}
