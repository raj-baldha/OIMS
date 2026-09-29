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
    public class CustomersController : ControllerBase
    {
        private readonly ICustomerService _customerService;

        /// <summary>Initializes the controller with its customer service.</summary>
        public CustomersController(ICustomerService customerService)
        {
            _customerService = customerService;
        }

        /// <summary>Creates a customer account from the supplied request.</summary>
        [Authorize(Roles = "Administrator,Manager,Employee")]
        [HttpPost]
        public async Task<IActionResult> CreateCustomer(CreateCustomerRequestDto request)
        {
            int userId = ClaimHelper.GetUserId(User);

            var customer = await _customerService.CreateCustomerAsync(request, userId);

            var response = ApiResponse<CreatedCustomerResponseDto>.SuccessResponse(
                customer,
                "Customer created successfully."
            );

            return StatusCode(StatusCodes.Status201Created, response);
        }

        /// <summary>Returns a filtered, paginated list of customers.</summary>
        [Authorize(Roles = "Administrator,Manager,Employee,Customer")]
        [HttpGet]
        public async Task<IActionResult> GetCustomers([FromQuery] GetCustomersRequestDto request)
        {
            string role = ClaimHelper.GetRole(User);

            int userId = ClaimHelper.GetUserId(User);

            var customers = await _customerService.GetCustomersAsync(request, role, userId);

            var response = ApiResponse<PagedResponseDto<CustomerListResponseDto>>.SuccessResponse(
                customers,
                "Customers retrieved successfully."
            );

            return Ok(response);
        }

        /// <summary>Updates the customer identified by the supplied ID.</summary>
        [Authorize(Roles = "Administrator,Manager,Employee")]
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateCustomer(int id, UpdateCustomerRequestDto request)
        {
            int userId = ClaimHelper.GetUserId(User);

            var customer = await _customerService.UpdateCustomerAsync(id, request, userId);

            var response = ApiResponse<CreatedCustomerResponseDto>.SuccessResponse(
                customer,
                "Customer updated successfully."
            );

            return Ok(response);
        }

        /// <summary>Deactivates the customer identified by the supplied ID.</summary>
        [Authorize(Roles = "Administrator,Manager,Employee")]
        [HttpPost("{id}/deactivate")]
        public async Task<IActionResult> DeactivateCustomer(int id)
        {
            int userId = ClaimHelper.GetUserId(User);

            await _customerService.DeactiveCustomersAsync(id, userId);

            var response = ApiResponse<bool>.SuccessResponse(
                true,
                "Customer deactivated successfully."
            );

            return Ok(response);
        }
    }
}
