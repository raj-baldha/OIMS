using System;
using System.Collections.Generic;
using System.Text;

namespace OIMS.Application.DTOs.Request
{
    public class GetCustomersRequestDto
    {
        public string? Search { get; set; }
        public string? SortBy { get; set; }
        public string? SortOrder { get; set; }
        public int Page { get; set; } = 1;
    }
}
