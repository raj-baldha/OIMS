using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using OIMS.API.Helpers;
using OIMS.Application.DTOs.Common;
using OIMS.Application.DTOs.Request;
using OIMS.Application.DTOs.Response;
using OIMS.Application.Interfaces.Services;

namespace OIMS.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize(Roles = "Administrator,Manager,Employee")]
    public class ProductsController : ControllerBase
    {
        private readonly IProductService _productService;

        public ProductsController(
            IProductService productService)
        {
            _productService = productService;
        }

        [HttpPost]
        public async Task<IActionResult> CreateProduct(CreateProductRequestDto request)
        {
            int userId = ClaimHelper.GetUserId(User);

            var product = await _productService.CreateProductAsync(request,userId);

            var response = ApiResponse<CreatedProductResponseDto>.SuccessResponse(product,"Product created successfully.");

            return StatusCode(StatusCodes.Status201Created,response);
        }

        [HttpPut("{productId:int}")]
        public async Task<IActionResult> UpdateProduct(int productId,UpdateProductRequestDto request)
        {
            int userId = ClaimHelper.GetUserId(User);

            var product = await _productService.UpdateProductAsync(productId,request,userId);

            var response = ApiResponse<CreatedProductResponseDto>.SuccessResponse(product,"Product updated successfully.");

            return Ok(response);
        }

        [HttpPost("{productId:int}/inventory-adjustment")]
        public async Task<IActionResult> AdjustInventory(int productId,AdjustInventoryRequestDto request)
        {
            int userId = ClaimHelper.GetUserId(User);

            var product = await _productService.AdjustInventoryAsync(productId,request,userId);

            var response = ApiResponse<CreatedProductResponseDto>.SuccessResponse(product,"Inventory adjusted successfully.");

            return Ok(response);
        }

        [HttpPatch("{productId:int}/deactivate")]
        public async Task<IActionResult> DeactivateProduct(int productId)
        {
            int userId = ClaimHelper.GetUserId(User);

            var product = await _productService.DeactivateProductAsync(productId,userId);

            var response = ApiResponse<DeactivatedProductResponseDto>.SuccessResponse(product,"Product deactivated successfully.");

            return Ok(response);
        }

        [HttpGet]
        public async Task<IActionResult> GetProducts([FromQuery] GetProductsRequestDto request)
        {
            var products = await _productService.GetProductsAsync(request);

            var response = ApiResponse<PagedResponseDto<ProductListResponseDto>>.SuccessResponse(products,"Products retrieved successfully.");

            return Ok(response);
        }
    }
}