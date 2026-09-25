using Microsoft.EntityFrameworkCore;
using OIMS.Application.DTOs.Request;
using OIMS.Application.DTOs.Response;
using OIMS.Application.Interfaces.Repositories;
using OIMS.Data.DbContext;
using OIMS.Domain.Entities;

namespace OIMS.Infrastructure.Repositories
{
    public class ProductRepository : IProductRepository
    {
        private readonly AppDbContext _context;

        public ProductRepository(AppDbContext context)
        {
            _context = context;
        }

        public async Task<bool> IsSkuExistsAsync(string sku)
        {
            return await _context.Products
                .AnyAsync(x =>
                    x.Sku == sku &&
                    !x.IsDeleted);
        }

        public async Task<bool> IsCategoryExistsAsync(int categoryId)
        {
            return await _context.Categories
                .AnyAsync(x =>
                    x.Id == categoryId &&
                    !x.IsDeleted);
        }

        public async Task AddAsync(Product product)
        {
            await _context.Products.AddAsync(product);
        }

        public async Task SaveChangesAsync()
        {
            await _context.SaveChangesAsync();
        }

        public async Task<Product?> GetByIdAsync(int productId)
        {
            return await _context.Products
                .FirstOrDefaultAsync(x =>
                    x.Id == productId &&
                    !x.IsDeleted);
        }

        public async Task<bool> IsSkuExistsForOtherProductAsync(string sku,int productId)
        {
            return await _context.Products
                .AnyAsync(x =>
                    x.Sku == sku &&
                    x.Id != productId &&
                    !x.IsDeleted);
        }

        public async Task<Product?> GetByIdForInventoryAsync(int productId)
        {
            return await _context.Products
                .FirstOrDefaultAsync(x =>
                    x.Id == productId &&
                    !x.IsDeleted);
        }

        public async Task AddInventoryTransactionAsync(InventoryTransaction transaction)
        {
            await _context.InventoryTransactions.AddAsync(transaction);
        }

        public async Task<(List<ProductListResponseDto> Data, int TotalRecords)>GetProductsAsync(GetProductsRequestDto request,int pageSize)
        {
            var query = _context.Products
                .AsNoTracking()
                .Where(x => !x.IsDeleted)
                .AsQueryable();

            if (!string.IsNullOrWhiteSpace(request.Search))
            {
                string search = request.Search.Trim();

                query = query.Where(x =>
                    x.Name.Contains(search) ||
                    x.Sku.Contains(search));
            }

            if (request.CategoryId.HasValue)
            {
                query = query.Where(x =>
                    x.CategoryId == request.CategoryId.Value);
            }

            if (request.IsActive.HasValue)
            {
                query = query.Where(x =>
                    x.IsActive == request.IsActive.Value);
            }

            if (request.MinPrice.HasValue)
            {
                query = query.Where(x =>
                    x.SellingPrice >= request.MinPrice.Value);
            }

            if (request.MaxPrice.HasValue)
            {
                query = query.Where(x =>
                    x.SellingPrice <= request.MaxPrice.Value);
            }

            int totalRecords = await query.CountAsync();

            string sortBy =
                request.SortBy?.Trim().ToLower()
                ?? "name";

            string sortOrder =
                request.SortOrder?.Trim().ToLower()
                ?? "asc";

            bool descending = sortOrder == "desc";

            query = sortBy switch
            {
                "price" => descending
                    ? query.OrderByDescending(x => x.SellingPrice)
                    : query.OrderBy(x => x.SellingPrice),

                "quantity" => descending
                    ? query.OrderByDescending(x => x.QuantityOnHand)
                    : query.OrderBy(x => x.QuantityOnHand),

                "sku" => descending
                    ? query.OrderByDescending(x => x.Sku)
                    : query.OrderBy(x => x.Sku),

                _ => descending
                    ? query.OrderByDescending(x => x.Name)
                    : query.OrderBy(x => x.Name)
            };

            int page = request.Page < 1 ? 1 : request.Page;

            int skip = (page - 1) * pageSize;

            var data = await query
                .Skip(skip)
                .Take(pageSize)
                .Select(x => new ProductListResponseDto
                {
                    Id = x.Id,
                    Name = x.Name,
                    Sku = x.Sku,
                    CategoryId = x.CategoryId,
                    CategoryName = x.Category != null
                        ? x.Category.Name
                        : string.Empty,
                    SellingPrice = x.SellingPrice,
                    QuantityOnHand = x.QuantityOnHand,
                    MinStockLevel = x.MinStockLevel,
                    IsActive = x.IsActive
                })
                .ToListAsync();

            return (data, totalRecords);
        }
    }
}