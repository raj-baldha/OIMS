using System;
using System.Collections.Generic;
using System.Text;

namespace OIMS.Application.DTOs.Common
{
    public class InvoiceItemDto
    {
        public string ProductName { get; set; } = string.Empty;
        public int Quantity { get; set; }
        public decimal UnitPrice { get; set; }
        public decimal LineTotal { get; set; }
    }
}
