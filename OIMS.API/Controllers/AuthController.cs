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

        public AuthController(IAuthService authService, IConfiguration configuration)
        {
            _authService = authService;
            _configuration = configuration;
        }

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

        [HttpPost("logout")]
        public IActionResult Logout()
        {
            CookieHelper.ClearAccessTokenCookie(Response);

            var response = ApiResponse<object>.SuccessResponse(null!, "Logout successful.");

            return Ok(response);
        }
    }
}
