using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace OIMS.Application.DTOs.Request
{
    public class CreateCategoryRequestDto
    {
        [Required]
        [StringLength(150)]
        public string Name { get; set; } = string.Empty;
    }
}
