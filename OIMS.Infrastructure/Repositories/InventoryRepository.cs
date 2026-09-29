using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using OIMS.Application.DTOs.Request;
using OIMS.Application.DTOs.Response;
using OIMS.Application.Helpers;
using OIMS.Application.Interfaces.Repositories;
using OIMS.Data.DbContext;
using OIMS.Domain.Entities;

namespace OIMS.Infrastructure.Repositories
{
    public class InventoryRepository : IInventoryRepository
    {
        private readonly AppDbContext _context;

        /// <summary>Initializes the repository with the application database context.</summary>
        public InventoryRepository(AppDbContext context)
        {
            _context = context;
        }

        /// <summary>Retrieves active products at or below their minimum stock levels.</summary>
        public async Task<List<LowStockProductResponseDto>> GetLowStockProductsAsync()
        {
            return await _context
                .Products.AsNoTracking()
                .Where(x => !x.IsDeleted && x.IsActive && x.QuantityOnHand <= x.MinStockLevel)
                .OrderBy(x => x.QuantityOnHand)
                .Select(x => new LowStockProductResponseDto
                {
                    Id = x.Id,
                    Name = x.Name,
                    Sku = x.Sku,
                    CategoryId = x.CategoryId,

                    CategoryName = x.Category != null ? x.Category.Name : string.Empty,

                    QuantityOnHand = x.QuantityOnHand,
                    MinStockLevel = x.MinStockLevel,

                    Shortage = x.MinStockLevel - x.QuantityOnHand,

                    IsActive = x.IsActive,
                })
                .ToListAsync();
        }

        /// <summary>Determines whether a non-deleted product exists for the specified ID.</summary>
        public async Task<bool> ProductExistsAsync(int productId)
        {
            return await _context.Products.AnyAsync(x => x.Id == productId && !x.IsDeleted);
        }

        /// <summary>Retrieves paginated inventory transactions for a product.</summary>
        public async Task<(
            List<InventoryHistoryResponseDto> Data,
            int TotalRecords
        )> GetInventoryHistoryAsync(
            int productId,
            GetInventoryHistoryRequestDto request,
            int pageSize
        )
        {
            var query = _context
                .InventoryTransactions.AsNoTracking()
                .Where(x => x.ProductId == productId)
                .AsQueryable();

            int totalRecords = await query.CountAsync();

            int page = request.Page < 1 ? 1 : request.Page;

            int skip = (page - 1) * pageSize;

            var data = await query
                .OrderBy(x => x.CreatedAt)
                .ThenBy(x => x.Id)
                .Skip(skip)
                .Take(pageSize)
                //.Paginate(skip, pageSize)
                .Select(x => new InventoryHistoryResponseDto
                {
                    Id = x.Id,
                    ProductId = x.ProductId,
                    ChangeType = x.ChangeType.ToString(),
                    QuantityBefore = x.QuantityBefore,
                    QuantityChange = x.QuantityChange,
                    QuantityAfter = x.QuantityAfter,
                    Notes = x.Notes,
                    CreatedBy = x.CreatedBy,
                    CreatedByName = x.Creator != null ? x.Creator.Username : null,

                    CreatedAt = x.CreatedAt,
                })
                .ToListAsync();

            return (data, totalRecords);
        }
    }
}
