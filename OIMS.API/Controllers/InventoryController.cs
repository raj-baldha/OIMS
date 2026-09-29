using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using OIMS.Application.DTOs.Common;
using OIMS.Application.DTOs.Request;
using OIMS.Application.DTOs.Response;
using OIMS.Application.Interfaces.Services;

namespace OIMS.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize(Roles = "Administrator,Manager,Employee")]
    public class InventoryController : ControllerBase
    {
        private readonly IInventoryService _inventoryService;

        /// <summary>Initializes the controller with its inventory service.</summary>
        public InventoryController(IInventoryService inventoryService)
        {
            _inventoryService = inventoryService;
        }

        /// <summary>Returns the products currently below their low-stock threshold.</summary>
        [HttpGet("low-stock")]
        public async Task<IActionResult> GetLowStockProducts()
        {
            var products = await _inventoryService.GetLowStockProductsAsync();

            var response = ApiResponse<List<LowStockProductResponseDto>>.SuccessResponse(
                products,
                "Low-stock products retrieved successfully."
            );

            return Ok(response);
        }

        /// <summary>Returns paginated inventory transaction history matching the query.</summary>
        [HttpGet("{productId:int}/inventory-history")]
        public async Task<IActionResult> GetInventoryHistory(
            int productId,
            [FromQuery] GetInventoryHistoryRequestDto request
        )
        {
            var history = await _inventoryService.GetInventoryHistoryAsync(productId, request);

            var response = ApiResponse<
                PagedResponseDto<InventoryHistoryResponseDto>
            >.SuccessResponse(history, "Inventory history retrieved successfully.");

            return Ok(response);
        }
    }
}
