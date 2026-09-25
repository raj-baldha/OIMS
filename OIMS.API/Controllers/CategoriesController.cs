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
    public class CategoriesController : ControllerBase
    {
        private readonly ICategoryService _categoryService;

        public CategoriesController(
            ICategoryService categoryService)
        {
            _categoryService = categoryService;
        }

        [HttpPost]
        public async Task<IActionResult> CreateCategory(
            CreateCategoryRequestDto request)
        {
            int userId = ClaimHelper.GetUserId(User);

            var category = await _categoryService.CreateCategoryAsync(request,userId);

            var response = ApiResponse<CreatedCategoryResponseDto>.SuccessResponse(category,"Category created successfully.");

            return StatusCode(StatusCodes.Status201Created,response);
        }
    }
}