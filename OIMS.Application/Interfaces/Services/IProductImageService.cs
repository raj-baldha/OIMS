using Microsoft.AspNetCore.Http;
using OIMS.Application.DTOs.Products;

namespace OIMS.Application.Interfaces.Services
{
    public interface IProductImageService
    {
        Task<List<ProductImageResponseDto>> UploadImagesAsync(
            int productId,
            List<IFormFile> files,
            int userId
        );
    }
}
