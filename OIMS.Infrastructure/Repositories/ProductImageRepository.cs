using Microsoft.EntityFrameworkCore;
using OIMS.Application.Interfaces.Repositories;
using OIMS.Data.DbContext;
using OIMS.Domain.Entities;

namespace OIMS.Infrastructure.Repositories
{
    public class ProductImageRepository : IProductImageRepository
    {
        private readonly AppDbContext _context;

        public ProductImageRepository(AppDbContext context)
        {
            _context = context;
        }

        public async Task<Product?> GetProductAsync(int productId)
        {
            return await _context.Products
                .FirstOrDefaultAsync(x =>
                    x.Id == productId &&
                    !x.IsDeleted);
        }

        public async Task<int> GetImageCountAsync(int productId)
        {
            return await _context.ProductImages
                .CountAsync(x =>
                    x.ProductId == productId &&
                    !x.IsDeleted);
        }

        public async Task AddAsync(ProductImage productImage)
        {
            await _context.ProductImages.AddAsync(productImage);
        }

        public async Task SaveChangesAsync()
        {
            await _context.SaveChangesAsync();
        }
    }
}