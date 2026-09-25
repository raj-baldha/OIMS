using Microsoft.Extensions.Configuration;
using OIMS.Application.DTOs.Common;
using OIMS.Application.DTOs.Request;
using OIMS.Application.DTOs.Response;
using OIMS.Application.Interfaces.Repositories;
using OIMS.Application.Interfaces.Services;
using OIMS.Domain.Exceptions;

namespace OIMS.Application.Services
{
    public class InventoryService : IInventoryService
    {
        private readonly IInventoryRepository _inventoryRepository;
        private readonly IConfiguration _configuration;

        public InventoryService(
            IInventoryRepository inventoryRepository,
            IConfiguration configuration)
        {
            _inventoryRepository = inventoryRepository;
            _configuration = configuration;
        }

        public async Task<List<LowStockProductResponseDto>>GetLowStockProductsAsync()
        {
            return await _inventoryRepository
                .GetLowStockProductsAsync();
        }

        public async Task<PagedResponseDto<InventoryHistoryResponseDto>> GetInventoryHistoryAsync( int productId, GetInventoryHistoryRequestDto request)
        {
            bool productExists = await _inventoryRepository.ProductExistsAsync(productId);

            if (!productExists)
            {
                throw new NotFoundException("Product not found.");
            }

            int pageSize = int.Parse(_configuration["Pagination:PageSize"]!);

            if (pageSize <= 0)
            {
                pageSize = 10;
            }

            int page = request.Page < 1
                ? 1
                : request.Page;

            request.Page = page;

            var result = await _inventoryRepository.GetInventoryHistoryAsync(productId,request,pageSize);

            int totalPages = (int)Math.Ceiling(result.TotalRecords / (double)pageSize);

            return new PagedResponseDto<InventoryHistoryResponseDto>
            {
                Data = result.Data,
                Page = page,
                PageSize = pageSize,
                TotalRecords = result.TotalRecords,
                TotalPages = totalPages
            };
        }
    }
}