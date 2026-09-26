using System;
using System.Collections.Generic;
using System.Text;
using OIMS.Application.DTOs.Request;
using OIMS.Application.DTOs.Response;
using OIMS.Domain.Entities;

namespace OIMS.Application.Interfaces.Repositories
{
    public interface IInventoryRepository
    {
        Task<List<LowStockProductResponseDto>> GetLowStockProductsAsync();
        Task<(List<InventoryHistoryResponseDto> Data, int TotalRecords)> GetInventoryHistoryAsync(
            int productId,
            GetInventoryHistoryRequestDto request,
            int pageSize
        );
        Task<bool> ProductExistsAsync(int productId);
    }
}
