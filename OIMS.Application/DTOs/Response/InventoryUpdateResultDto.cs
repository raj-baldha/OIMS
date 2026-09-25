using System;
using System.Collections.Generic;
using System.Text;

namespace OIMS.Application.DTOs.Response
{
    public class InventoryUpdateResultDto
    {
        public int QuantityBefore { get; set; }
        public int QuantityAfter { get; set; }
    }
}
