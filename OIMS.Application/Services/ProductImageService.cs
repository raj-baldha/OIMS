using Microsoft.AspNetCore.Http;
using OIMS.Application.DTOs.Products;
using OIMS.Application.Helpers;
using OIMS.Application.Interfaces.Repositories;
using OIMS.Application.Interfaces.Services;
using OIMS.Domain.Entities;
using OIMS.Domain.Exceptions;

namespace OIMS.Application.Services;

public class ProductImageService : IProductImageService
{
    private const int MaxImagesPerProduct = 3;
    private const long MaxFileSize = 5 * 1024 * 1024;

    private static readonly string[] AllowedExtensions = [".jpg", ".jpeg", ".png", ".webp"];

    private static readonly string[] AllowedContentTypes =
    [
        "image/jpeg",
        "image/png",
        "image/webp",
    ];

    private readonly IProductImageRepository _productImageRepository;
    private readonly IHttpContextAccessor _httpContextAccessor;

    /// <summary>Initializes the service with its product image storage and persistence dependencies.</summary>
    public ProductImageService(
        IProductImageRepository productImageRepository,
        IHttpContextAccessor httpContextAccessor
    )
    {
        _productImageRepository = productImageRepository;
        _httpContextAccessor = httpContextAccessor;
    }

    /// <summary>Validates and stores images for a product, then returns their response details.</summary>
    public async Task<List<ProductImageResponseDto>> UploadImagesAsync(
        int productId,
        List<IFormFile> files,
        int userId
    )
    {
        var product = await _productImageRepository.GetProductAsync(productId);
        if (product is null)
        {
            throw new NotFoundException("Product not found.");
        }

        if (files is null || files.Count == 0)
        {
            throw new BadRequestException("At least one image is required.");
        }

        int existingImageCount = await _productImageRepository.GetImageCountAsync(productId);
        if (existingImageCount + files.Count > MaxImagesPerProduct)
        {
            throw new BadRequestException(
                $"A product can have a maximum of {MaxImagesPerProduct} images."
            );
        }

        var uploadedImages = new List<ProductImage>();
        var now = DateTime.UtcNow;

        foreach (var file in files)
        {
            ValidateFile(file);

            string fileUrl = await ProductImageStorageHelper.SaveImageAsync(productId, file);

            var productImage = new ProductImage
            {
                ProductId = productId,
                FileUrl = fileUrl,
                OriginalFileName = file.FileName,
                StoredFileName = Path.GetFileName(fileUrl),
                ContentType = file.ContentType,
                FileSize = file.Length,
                CreatedAt = now,
                CreatedBy = userId,
                IsDeleted = false,
            };

            uploadedImages.Add(productImage);
        }

        foreach (var image in uploadedImages)
        {
            await _productImageRepository.AddAsync(image);
        }

        await _productImageRepository.SaveChangesAsync();

        var request = _httpContextAccessor.HttpContext!.Request;

        return uploadedImages
            .Select(image => new ProductImageResponseDto
            {
                Id = image.Id,
                ProductId = image.ProductId,
                FileUrl = request.Scheme + "://" + request.Host + image.FileUrl,
                OriginalFileName = image.OriginalFileName,
                ContentType = image.ContentType,
                FileSize = image.FileSize,
                CreatedAt = image.CreatedAt,
            })
            .ToList();
    }

    /// <summary>Validates an uploaded image file against the supported file requirements.</summary>
    private static void ValidateFile(IFormFile file)
    {
        if (file is null || file.Length == 0)
        {
            throw new BadRequestException("Image file cannot be empty.");
        }

        if (file.Length > MaxFileSize)
        {
            throw new BadRequestException("Each image must be less than or equal to 5 MB.");
        }

        string extension = Path.GetExtension(file.FileName).ToLowerInvariant();
        if (!AllowedExtensions.Contains(extension))
        {
            throw new BadRequestException("Only JPG, JPEG, PNG and WEBP images are allowed.");
        }

        if (!AllowedContentTypes.Contains(file.ContentType.ToLowerInvariant()))
        {
            throw new BadRequestException("Invalid image content type.");
        }
    }
}
