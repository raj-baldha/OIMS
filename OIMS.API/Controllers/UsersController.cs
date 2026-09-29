using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using OIMS.Application.DTOs.Common;
using OIMS.Application.DTOs.Request;
using OIMS.Application.DTOs.Response;
using OIMS.Application.Interfaces.Services;

namespace OIMS.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class UsersController : ControllerBase
    {
        private readonly IUserService _userService;

        /// <summary>Initializes the controller with its user service.</summary>
        public UsersController(IUserService userService)
        {
            _userService = userService;
        }

        /// <summary>Creates a user account from the supplied request.</summary>
        [Authorize(Roles = "Administrator")]
        [HttpPost]
        public async Task<IActionResult> CreateUser(CreateUserRequestDto request)
        {
            var user = await _userService.CreateUserAsync(request);

            var response = ApiResponse<CreatedUserResponseDto>.SuccessResponse(
                user,
                "User created successfully."
            );

            return StatusCode(StatusCodes.Status201Created, response);
        }
    }
}
