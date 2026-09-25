using System;
using System.Collections.Generic;
using System.Text;

namespace OIMS.Application.DTOs.Response
{
    public class InventoryHistoryResponseDto
    {
        public int Id { get; set; }
        public int ProductId { get; set; }
        public string ChangeType { get; set; } = string.Empty;
        public int QuantityBefore { get; set; }
        public int QuantityChange { get; set; }
        public int QuantityAfter { get; set; }
        public string? Notes { get; set; }
        public int? CreatedBy { get; set; }
        public string? CreatedByName { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}
