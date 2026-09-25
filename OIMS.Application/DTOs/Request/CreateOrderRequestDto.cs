using OIMS.Application.DTOs.Orders;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace OIMS.Application.DTOs.Request
{
    public class CreateOrderRequestDto
    {
        [Required]
        [StringLength(500)]
        public string ShippingAddress { get; set; } = string.Empty;

        [Required]
        public string PaymentMethod { get; set; } = string.Empty;

        [Required]
        [MinLength(1)]
        public List<CreateOrderItemRequestDto> Items { get; set; } = new();
    }
}
