using OIMS.Application.DTOs.Common;
using OIMS.Application.DTOs.Request;
using OIMS.Application.DTOs.Response;
using System;
using System.Collections.Generic;
using System.Text;

namespace OIMS.Application.Interfaces.Services
{
    public interface IProductService
    {
        Task<CreatedProductResponseDto> CreateProductAsync(CreateProductRequestDto request,int userId);
        Task<CreatedProductResponseDto> UpdateProductAsync(int productId,UpdateProductRequestDto request,int userId);
        Task<CreatedProductResponseDto> AdjustInventoryAsync(int productId,AdjustInventoryRequestDto request,int userId);
        Task<DeactivatedProductResponseDto> DeactivateProductAsync(int productId,int userId);
        Task<PagedResponseDto<ProductListResponseDto>> GetProductsAsync(GetProductsRequestDto request);
    }
}
