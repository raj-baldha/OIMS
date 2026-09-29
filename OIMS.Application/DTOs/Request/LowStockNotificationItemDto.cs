namespace OIMS.Application.DTOs.Request
{
    public class LowStockNotificationItemDto
    {
        public int ProductId { get; set; }
        public string ProductName { get; set; } = string.Empty;
        public string Sku { get; set; } = string.Empty;
        public int QuantityOnHand { get; set; }
        public int MinStockLevel { get; set; }
    }
}
