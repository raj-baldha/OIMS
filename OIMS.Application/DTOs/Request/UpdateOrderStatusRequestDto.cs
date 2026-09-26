using System.ComponentModel.DataAnnotations;

namespace OIMS.Application.DTOs.Request
{
    public class UpdateOrderStatusRequestDto
    {
        [Required]
        public string Status { get; set; } = string.Empty;
    }
}
