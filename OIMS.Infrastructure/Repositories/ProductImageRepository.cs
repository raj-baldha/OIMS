using Microsoft.EntityFrameworkCore;
using OIMS.Application.Interfaces.Repositories;
using OIMS.Data.DbContext;
using OIMS.Domain.Entities;

namespace OIMS.Infrastructure.Repositories
{
    public class ProductImageRepository : IProductImageRepository
    {
        private readonly AppDbContext _context;

        /// <summary>Initializes the repository with the application database context.</summary>
        public ProductImageRepository(AppDbContext context)
        {
            _context = context;
        }

        /// <summary>Finds a product by identifier for image operations.</summary>
        public async Task<Product?> GetProductAsync(int productId)
        {
            return await _context.Products.FirstOrDefaultAsync(x =>
                x.Id == productId && !x.IsDeleted
            );
        }

        /// <summary>Retrieves the image records associated with a product.</summary>
        public async Task<List<ProductImage>> GetImagesByProductIdAsync(int productId)
        {
            return await _context
                .ProductImages.Where(x => x.ProductId == productId && !x.IsDeleted)
                .ToListAsync();
        }

        /// <summary>Counts the image records associated with a product.</summary>
        public async Task<int> GetImageCountAsync(int productId)
        {
            return await _context.ProductImages.CountAsync(x =>
                x.ProductId == productId && !x.IsDeleted
            );
        }

        /// <summary>Adds a product image record to the database context.</summary>
        public async Task AddAsync(ProductImage productImage)
        {
            await _context.ProductImages.AddAsync(productImage);
        }

        /// <summary>Persists pending database changes.</summary>
        public async Task SaveChangesAsync()
        {
            await _context.SaveChangesAsync();
        }
    }
}
