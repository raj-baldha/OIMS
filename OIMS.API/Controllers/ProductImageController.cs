using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using OIMS.API.Helpers;
using OIMS.Application.DTOs.Common;
using OIMS.Application.DTOs.Products;
using OIMS.Application.Interfaces.Services;

namespace OIMS.API.Controllers
{
    [ApiController]
    [Route("api/products/{productId}/images")]
    [Authorize(Roles = "Administrator,Manager,Employee")]
    public class ProductImageController : ControllerBase
    {
        private readonly IProductImageService _productImageService;

        /// <summary>Initializes the controller with its product image service.</summary>
        public ProductImageController(IProductImageService productImageService)
        {
            _productImageService = productImageService;
        }

        /// <summary>Uploads one or more images for the specified product.</summary>
        [HttpPost]
        public async Task<IActionResult> UploadImages(
            int productId,
            [FromForm] List<IFormFile> files
        )
        {
            int userId = ClaimHelper.GetUserId(User);

            var result = await _productImageService.UploadImagesAsync(productId, files, userId);

            var response = ApiResponse<List<ProductImageResponseDto>>.SuccessResponse(
                result,
                "Product images uploaded successfully."
            );

            return Ok(response);
        }
    }
}
