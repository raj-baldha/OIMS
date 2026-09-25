using System;
using System.Collections.Generic;
using System.Text;

namespace OIMS.Application.DTOs.Request
{
    public class GetProductsRequestDto
    {
        public string? Search { get; set; }
        public int? CategoryId { get; set; }
        public bool? IsActive { get; set; }
        public decimal? MinPrice { get; set; }
        public decimal? MaxPrice { get; set; }
        public string? SortBy { get; set; }
        public string? SortOrder { get; set; }
        public int Page { get; set; } = 1;
    }
}
