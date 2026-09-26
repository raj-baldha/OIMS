namespace OIMS.Application.DTOs.Orders
{
    public class GetOrdersRequestDto
    {
        public string? Search { get; set; }
        public string? Status { get; set; }
        public string? SortBy { get; set; }
        public string? SortOrder { get; set; }
        public int Page { get; set; } = 1;
    }
}
