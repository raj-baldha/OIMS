using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using OIMS.API.Helpers;
using OIMS.Application.DTOs.Common;
using OIMS.Application.DTOs.Orders;
using OIMS.Application.DTOs.Request;
using OIMS.Application.Interfaces.Services;

namespace OIMS.API.Controllers;

[Route("api/[controller]")]
[ApiController]
[Authorize]
public class OrdersController : ControllerBase
{
    private readonly IOrderService _orderService;

    /// <summary>Initializes the controller with its order service.</summary>
    public OrdersController(IOrderService orderService)
    {
        _orderService = orderService;
    }

    /// <summary>Creates an order for the authenticated customer.</summary>
    [Authorize(Roles = "Customer")]
    [HttpPost]
    public async Task<IActionResult> CreateOrder(CreateOrderRequestDto request)
    {
        int customerId = ClaimHelper.GetUserId(User);

        var order = await _orderService.CreateOrderAsync(request, customerId);

        var response = ApiResponse<CreatedOrderResponseDto>.SuccessResponse(
            order,
            "Order placed successfully."
        );

        return StatusCode(StatusCodes.Status201Created, response);
    }

    /// <summary>Updates an order's status using the supplied request.</summary>
    [HttpPatch("{orderId:int}/status")]
    [Authorize(Roles = "Administrator,Manager,Employee")]
    public async Task<IActionResult> UpdateOrderStatus(
        int orderId,
        UpdateOrderStatusRequestDto request
    )
    {
        int userId = ClaimHelper.GetUserId(User);

        await _orderService.UpdateOrderStatusAsync(orderId, request, userId);

        var response = ApiResponse<object>.SuccessResponse(
            null!,
            "Order status updated successfully."
        );

        return Ok(response);
    }

    /// <summary>Cancels the specified order when permitted for the current user.</summary>
    [HttpPatch("{orderId:int}/cancel")]
    [Authorize(Roles = "Customer,Administrator,Manager,Employee")]
    public async Task<IActionResult> CancelOrder(int orderId)
    {
        int userId = ClaimHelper.GetUserId(User);
        string role = ClaimHelper.GetRole(User);

        await _orderService.CancelOrderAsync(orderId, userId, role);

        var response = ApiResponse<object>.SuccessResponse(null!, "Order cancelled successfully.");

        return Ok(response);
    }

    /// <summary>Returns orders matching the query and current user's access scope.</summary>
    [HttpGet]
    [Authorize(Roles = "Customer,Administrator,Manager,Employee")]
    public async Task<IActionResult> GetOrders([FromQuery] GetOrdersRequestDto request)
    {
        int userId = ClaimHelper.GetUserId(User);
        string role = ClaimHelper.GetRole(User);

        var orders = await _orderService.GetOrdersAsync(request, userId, role);

        var response = ApiResponse<PagedResponseDto<OrderListResponseDto>>.SuccessResponse(
            orders,
            "Orders retrieved successfully."
        );

        return Ok(response);
    }
}
