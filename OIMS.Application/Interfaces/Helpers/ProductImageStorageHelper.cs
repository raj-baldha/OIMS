using Microsoft.AspNetCore.Http;

namespace OIMS.Application.Helpers
{
    public static class ProductImageStorageHelper
    {
        private const string ImageFolder = "uploads/products";

        public static async Task<string> SaveImageAsync(int productId, IFormFile file)
        {
            string folderPath = Path.Combine(
                Directory.GetCurrentDirectory(),
                "wwwroot",
                "uploads",
                "products",
                productId.ToString()
            );

            if (!Directory.Exists(folderPath))
            {
                Directory.CreateDirectory(folderPath);
            }

            string extension = Path.GetExtension(file.FileName).ToLowerInvariant();

            string storedFileName = $"{Guid.NewGuid()}{extension}";

            string filePath = Path.Combine(folderPath, storedFileName);

            await using var stream = new FileStream(filePath, FileMode.Create);

            await file.CopyToAsync(stream);

            return $"/{ImageFolder}/{productId}/{storedFileName}";
        }
    }
}
