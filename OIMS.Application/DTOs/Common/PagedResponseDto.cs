using System;
using System.Collections.Generic;
using System.Text;

namespace OIMS.Application.DTOs.Common
{
    public class PagedResponseDto<T>
    {
        public List<T> Data { get; set; } = new();
        public int Page { get; set; }
        public int PageSize { get; set; }
        public int TotalRecords { get; set; }
        public int TotalPages { get; set; }
    }
}
