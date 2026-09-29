using AutoMapper;
using OIMS.Application.DTOs.Request;
using OIMS.Application.DTOs.Response;
using OIMS.Application.Interfaces.Helpers;
using OIMS.Application.Interfaces.Repositories;
using OIMS.Application.Interfaces.Services;
using OIMS.Domain.Exceptions;

namespace OIMS.Application.Services
{
    public class AuthService : IAuthService
    {
        private readonly IUserRepository _userRepository;
        private readonly IPasswordHelper _passwordHelper;
        private readonly IJwtHelper _jwtHelper;
        private readonly IMapper _mapper;

        /// <summary>Initializes the service with user, password, and token dependencies.</summary>
        public AuthService(
            IUserRepository userRepository,
            IPasswordHelper passwordHelper,
            IJwtHelper jwtHelper,
            IMapper mapper
        )
        {
            _userRepository = userRepository;
            _passwordHelper = passwordHelper;
            _jwtHelper = jwtHelper;
            _mapper = mapper;
        }

        /// <summary>Validates staff credentials and creates the corresponding login result and token.</summary>
        public async Task<(LoginResponseDto User, string Token)> LoginAsync(LoginRequestDto request)
        {
            request.Email = request.Email.Trim();

            var user = await _userRepository.GetByEmailAsync(request.Email);

            if (user == null)
            {
                throw new UnauthorizedException("Invalid email or password.");
            }

            if (!user.IsActive)
            {
                throw new UnauthorizedException("Your account is inactive.");
            }

            bool isPasswordValid = _passwordHelper.VerifyPassword(
                request.Password,
                user.PasswordHash
            );

            if (!isPasswordValid)
            {
                throw new UnauthorizedException("Invalid email or password.");
            }

            string token = _jwtHelper.GenerateToken(user.Id, user.Email, user.Role);

            var response = _mapper.Map<LoginResponseDto>(user);

            return (response, token);
        }
    }
}
