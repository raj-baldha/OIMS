using System;
using System.Collections.Generic;
using System.Text;

namespace OIMS.Application.DTOs.Response
{
    public class CreatedCategoryResponseDto
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; }
        public int? CreatedBy { get; set; }
    }
}
