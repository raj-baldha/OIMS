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
    public class AuthController : ControllerBase
    {
        private readonly IAuthService _authService;
        private readonly IConfiguration _configuration;

        /// <summary>Initializes the controller with its authentication service and configuration.</summary>
        public AuthController(IAuthService authService, IConfiguration configuration)
        {
            _authService = authService;
            _configuration = configuration;
        }

        /// <summary>Authenticates staff credentials and returns the login result.</summary>
        [HttpPost("login")]
        public async Task<IActionResult> Login(LoginRequestDto request)
        {
            var result = await _authService.LoginAsync(request);

            CookieHelper.SetAccessTokenCookie(Response, _configuration, result.Token);

            var response = ApiResponse<LoginResponseDto>.SuccessResponse(
                result.User,
                "Login successful."
            );

            return Ok(response);
        }

        /// <summary>Ends the staff session by clearing its authentication cookie.</summary>
        [HttpPost("logout")]
        public IActionResult Logout()
        {
            CookieHelper.ClearAccessTokenCookie(Response);

            var response = ApiResponse<object>.SuccessResponse(null!, "Logout successful.");

            return Ok(response);
        }
    }
}
