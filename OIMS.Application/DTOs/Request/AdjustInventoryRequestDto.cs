using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace OIMS.Application.DTOs.Request
{
    public class AdjustInventoryRequestDto
    {
        [Required]
        public int QuantityChange { get; set; }

        [StringLength(500)]
        public string? Notes { get; set; }
    }
}
