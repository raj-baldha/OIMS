using System;
using System.Collections.Generic;
using System.Text;
using OIMS.Application.DTOs.Common;
using OIMS.Application.DTOs.Request;
using OIMS.Application.DTOs.Response;

namespace OIMS.Application.Interfaces.Services
{
    public interface IInventoryService
    {
        Task<List<LowStockProductResponseDto>> GetLowStockProductsAsync();
        Task<PagedResponseDto<InventoryHistoryResponseDto>> GetInventoryHistoryAsync(
            int productId,
            GetInventoryHistoryRequestDto request
        );
    }
}
