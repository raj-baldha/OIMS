using System;
using System.Collections.Generic;
using System.Text;
using OIMS.Application.DTOs.Request;
using OIMS.Application.DTOs.Response;
using OIMS.Domain.Entities;

namespace OIMS.Application.Interfaces.Repositories
{
    public interface IProductRepository
    {
        Task<bool> IsSkuExistsAsync(string sku);
        Task<bool> IsCategoryExistsAsync(int categoryId);
        Task AddAsync(Product product);
        Task<Product?> GetByIdAsync(int productId);
        Task<bool> IsSkuExistsForOtherProductAsync(string sku, int productId);
        Task<Product?> GetByIdForInventoryAsync(int productId);
        Task AddInventoryTransactionAsync(InventoryTransaction transaction);
        Task<(List<ProductListResponseDto> Data, int TotalRecords)> GetProductsAsync(
            GetProductsRequestDto request,
            int pageSize
        );
        Task SaveChangesAsync();
    }
}
