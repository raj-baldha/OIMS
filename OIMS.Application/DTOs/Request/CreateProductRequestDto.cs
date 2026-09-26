using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace OIMS.Application.DTOs.Request
{
    public class CreateProductRequestDto
    {
        public string Name { get; set; } = string.Empty;

        public string Sku { get; set; } = string.Empty;

        public int CategoryId { get; set; }

        public decimal SellingPrice { get; set; }

        public decimal CostPrice { get; set; }

        public int QuantityOnHand { get; set; }

        public int MinStockLevel { get; set; }
    }
}
