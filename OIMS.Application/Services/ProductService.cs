using Microsoft.Extensions.Configuration;
using OIMS.Application.DTOs.Common;
using OIMS.Application.DTOs.Request;
using OIMS.Application.DTOs.Response;
using OIMS.Application.Interfaces.Repositories;
using OIMS.Application.Interfaces.Services;
using OIMS.Domain.Entities;
using OIMS.Domain.Enums;
using OIMS.Domain.Exceptions;

namespace OIMS.Application.Services
{
    public class ProductService : IProductService
    {
        private readonly IProductRepository _productRepository;
        private readonly IConfiguration _configuration;

        public ProductService(IProductRepository productRepository, IConfiguration configuration)
        {
            _productRepository = productRepository;
            _configuration = configuration;
        }

        public async Task<CreatedProductResponseDto> CreateProductAsync(
            CreateProductRequestDto request,
            int userId
        )
        {
            string sku = request.Sku.Trim();

            bool skuExists = await _productRepository.IsSkuExistsAsync(sku);

            if (skuExists)
            {
                throw new ConflictException("SKU already exists.");
            }

            bool categoryExists = await _productRepository.IsCategoryExistsAsync(
                request.CategoryId
            );

            if (!categoryExists)
            {
                throw new NotFoundException("Category not found.");
            }

            var product = new Product
            {
                Name = request.Name.Trim(),
                Sku = sku,
                CategoryId = request.CategoryId,
                SellingPrice = request.SellingPrice,
                CostPrice = request.CostPrice,
                QuantityOnHand = request.QuantityOnHand,
                MinStockLevel = request.MinStockLevel,

                IsActive = true,
                Version = 1,

                CreatedAt = DateTime.UtcNow,
                CreatedBy = userId,

                IsDeleted = false,
            };

            await _productRepository.AddAsync(product);
            await _productRepository.SaveChangesAsync();

            return new CreatedProductResponseDto
            {
                Id = product.Id,
                Name = product.Name,
                Sku = product.Sku,
                CategoryId = product.CategoryId,
                SellingPrice = product.SellingPrice,
                CostPrice = product.CostPrice,
                QuantityOnHand = product.QuantityOnHand,
                MinStockLevel = product.MinStockLevel,
                IsActive = product.IsActive,
                CreatedAt = product.CreatedAt,
                CreatedBy = product.CreatedBy,
            };
        }

        public async Task<CreatedProductResponseDto> UpdateProductAsync(
            int productId,
            UpdateProductRequestDto request,
            int userId
        )
        {
            var product = await _productRepository.GetByIdAsync(productId);

            if (product == null)
            {
                throw new NotFoundException("Product not found.");
            }

            string sku = request.Sku.Trim();

            bool skuExists = await _productRepository.IsSkuExistsForOtherProductAsync(
                sku,
                productId
            );

            if (skuExists)
            {
                throw new ConflictException("SKU already exists.");
            }

            bool categoryExists = await _productRepository.IsCategoryExistsAsync(
                request.CategoryId
            );

            if (!categoryExists)
            {
                throw new NotFoundException("Category not found.");
            }

            product.Name = request.Name.Trim();
            product.Sku = sku;
            product.CategoryId = request.CategoryId;
            product.SellingPrice = request.SellingPrice;
            product.CostPrice = request.CostPrice;
            product.MinStockLevel = request.MinStockLevel;
            product.IsActive = request.IsActive;

            product.UpdatedAt = DateTime.UtcNow;
            product.UpdatedBy = userId;

            product.Version++;

            await _productRepository.SaveChangesAsync();

            return new CreatedProductResponseDto
            {
                Id = product.Id,
                Name = product.Name,
                Sku = product.Sku,
                CategoryId = product.CategoryId,
                SellingPrice = product.SellingPrice,
                CostPrice = product.CostPrice,
                QuantityOnHand = product.QuantityOnHand,
                MinStockLevel = product.MinStockLevel,
                IsActive = product.IsActive,
                CreatedAt = product.CreatedAt,
                CreatedBy = product.CreatedBy,
            };
        }

        public async Task<CreatedProductResponseDto> AdjustInventoryAsync(
            int productId,
            AdjustInventoryRequestDto request,
            int userId
        )
        {
            var product = await _productRepository.GetByIdForInventoryAsync(productId);

            if (product == null)
            {
                throw new NotFoundException("Product not found.");
            }

            int quantityBefore = product.QuantityOnHand;

            int quantityChange = request.QuantityChange;

            int quantityAfter = quantityBefore + quantityChange;

            if (quantityAfter < 0)
            {
                throw new BadRequestException("Inventory quantity cannot be negative.");
            }

            if (quantityChange == 0)
            {
                throw new BadRequestException("Quantity change cannot be zero.");
            }

            product.QuantityOnHand = quantityAfter;

            product.UpdatedAt = DateTime.UtcNow;
            product.UpdatedBy = userId;

            product.Version++;

            var transaction = new InventoryTransaction
            {
                ProductId = product.Id,

                ChangeType = InventoryChangeType.ManualAdjustment,

                QuantityBefore = quantityBefore,
                QuantityChange = quantityChange,
                QuantityAfter = quantityAfter,

                Notes = request.Notes?.Trim(),

                CreatedBy = userId,
                CreatedAt = DateTime.UtcNow,
            };

            await _productRepository.AddInventoryTransactionAsync(transaction);

            await _productRepository.SaveChangesAsync();

            return new CreatedProductResponseDto
            {
                Id = product.Id,
                Name = product.Name,
                Sku = product.Sku,
                CategoryId = product.CategoryId,
                SellingPrice = product.SellingPrice,
                CostPrice = product.CostPrice,
                QuantityOnHand = product.QuantityOnHand,
                MinStockLevel = product.MinStockLevel,
                IsActive = product.IsActive,
                CreatedAt = product.CreatedAt,
                CreatedBy = product.CreatedBy,
            };
        }

        public async Task<DeactivatedProductResponseDto> DeactivateProductAsync(
            int productId,
            int userId
        )
        {
            var product = await _productRepository.GetByIdAsync(productId);

            if (product == null)
            {
                throw new NotFoundException("Product not found.");
            }

            if (!product.IsActive)
            {
                throw new BadRequestException("Product is already inactive.");
            }

            product.IsActive = false;

            product.UpdatedAt = DateTime.UtcNow;
            product.UpdatedBy = userId;

            product.Version++;

            await _productRepository.SaveChangesAsync();

            return new DeactivatedProductResponseDto
            {
                Id = product.Id,
                Name = product.Name,
                Sku = product.Sku,
                IsActive = product.IsActive,
                UpdatedAt = product.UpdatedAt,
                UpdatedBy = product.UpdatedBy,
            };
        }

        public async Task<PagedResponseDto<ProductListResponseDto>> GetProductsAsync(
            GetProductsRequestDto request
        )
        {
            int pageSize = int.Parse(_configuration["Pagination:PageSize"]!);

            if (pageSize <= 0)
            {
                pageSize = 10;
            }

            int page = request.Page < 1 ? 1 : request.Page;

            request.Page = page;

            var result = await _productRepository.GetProductsAsync(request, pageSize);

            int totalPages = (int)Math.Ceiling(result.TotalRecords / (double)pageSize);

            return new PagedResponseDto<ProductListResponseDto>
            {
                Data = result.Data,
                Page = page,
                PageSize = pageSize,
                TotalRecords = result.TotalRecords,
                TotalPages = totalPages,
            };
        }
    }
}
