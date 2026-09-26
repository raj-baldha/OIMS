namespace OIMS.Application.DTOs.Orders
{
    public class PaymentOrderResponseDto
    {
        public int Id { get; set; }

        public decimal Amount { get; set; }

        public string PaymentMethod { get; set; } = string.Empty;

        public string PaymentStatus { get; set; } = string.Empty;

        public DateTime? PaidAt { get; set; }
    }
}
