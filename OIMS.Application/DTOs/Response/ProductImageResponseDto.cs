namespace OIMS.Application.DTOs.Products
{
    public class ProductImageResponseDto
    {
        public int Id { get; set; }
        public int ProductId { get; set; }
        public string FileUrl { get; set; } = string.Empty;
        public string? OriginalFileName { get; set; }
        public string? ContentType { get; set; }
        public long? FileSize { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}
