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
    public class CustomerAuthController : ControllerBase
    {
        private readonly ICustomerAuthService _customerAuthService;
        private readonly IConfiguration _configuration;

        /// <summary>Initializes the controller with its customer authentication dependencies.</summary>
        public CustomerAuthController(
            ICustomerAuthService customerAuthService,
            IConfiguration configuration
        )
        {
            _customerAuthService = customerAuthService;
            _configuration = configuration;
        }

        /// <summary>Authenticates customer credentials and returns the login result.</summary>
        [HttpPost("login")]
        public async Task<IActionResult> Login(CustomerLoginRequestDto request)
        {
            var result = await _customerAuthService.LoginAsync(request);

            CookieHelper.SetAccessTokenCookie(Response, _configuration, result.Token);

            var response = ApiResponse<CustomerLoginResponseDto>.SuccessResponse(
                result.Customer,
                "Customer login successful."
            );

            return Ok(response);
        }

        /// <summary>Ends the customer session by clearing its authentication cookie.</summary>
        [HttpPost("logout")]
        public IActionResult Logout()
        {
            CookieHelper.ClearAccessTokenCookie(Response);

            var response = ApiResponse<object>.SuccessResponse(
                null!,
                "Customer logout successful."
            );

            return Ok(response);
        }
    }
}
