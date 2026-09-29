using OIMS.Domain.Entities;

namespace OIMS.Application.Interfaces.Repositories
{
    public interface IProductImageRepository
    {
        Task<Product?> GetProductAsync(int productId);
        Task<int> GetImageCountAsync(int productId);
        Task<List<ProductImage>> GetImagesByProductIdAsync(int productId);
        Task AddAsync(ProductImage productImage);
        Task SaveChangesAsync();
    }
}
