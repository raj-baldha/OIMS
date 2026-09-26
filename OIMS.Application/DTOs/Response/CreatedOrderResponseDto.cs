namespace OIMS.Application.DTOs.Orders
{
    public class CreatedOrderResponseDto
    {
        public int Id { get; set; }
        public int CustomerId { get; set; }
        public DateTime OrderDate { get; set; }
        public string Status { get; set; } = string.Empty;
        public string ShippingAddress { get; set; } = string.Empty;
        public decimal Subtotal { get; set; }
        public decimal DiscountAmount { get; set; }
        public decimal TotalAmount { get; set; }
        public string PaymentMethod { get; set; } = string.Empty;
        public string PaymentStatus { get; set; } = string.Empty;
        public string? InvoiceUrl { get; set; }
        public List<CreatedOrderItemResponseDto> Items { get; set; } = new();
    }
}
