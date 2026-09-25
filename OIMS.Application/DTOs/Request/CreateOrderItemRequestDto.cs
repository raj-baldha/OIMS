using System.ComponentModel.DataAnnotations;

namespace OIMS.Application.DTOs.Orders
{
    public class CreateOrderItemRequestDto
    {
        [Range(1, int.MaxValue)]
        public int ProductId { get; set; }

        [Range(1, int.MaxValue)]
        public int Quantity { get; set; }
    }
}