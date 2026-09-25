using System;
using System.Collections.Generic;
using System.Text;

namespace OIMS.Application.DTOs.Response
{
    public class LowStockProductResponseDto
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Sku { get; set; } = string.Empty;
        public int CategoryId { get; set; }
        public string CategoryName { get; set; } = string.Empty;
        public int QuantityOnHand { get; set; }
        public int MinStockLevel { get; set; }
        public int Shortage { get; set; }
        public bool IsActive { get; set; }
    }
}
