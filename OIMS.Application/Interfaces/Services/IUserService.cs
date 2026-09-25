using OIMS.Application.DTOs.Request;
using OIMS.Application.DTOs.Response;

namespace OIMS.Application.Interfaces.Services
{
    public interface IUserService
    {
        Task<CreatedUserResponseDto> CreateUserAsync(CreateUserRequestDto request);
    }
}