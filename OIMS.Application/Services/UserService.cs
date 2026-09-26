using OIMS.Application.DTOs.Request;
using OIMS.Application.DTOs.Response;
using OIMS.Application.Interfaces.Helpers;
using OIMS.Application.Interfaces.Repositories;
using OIMS.Application.Interfaces.Services;
using OIMS.Domain.Entities;
using OIMS.Domain.Exceptions;

namespace OIMS.Application.Services
{
    public class UserService : IUserService
    {
        private readonly IUserRepository _userRepository;
        private readonly IPasswordHelper _passwordHelper;
        private readonly IEmailService _emailService;

        public UserService(
            IUserRepository userRepository,
            IPasswordHelper passwordHelper,
            IEmailService emailService
        )
        {
            _userRepository = userRepository;
            _passwordHelper = passwordHelper;
            _emailService = emailService;
        }

        public async Task<CreatedUserResponseDto> CreateUserAsync(CreateUserRequestDto request)
        {
            bool emailExists = await _userRepository.IsEmailExistsAsync(request.Email);

            if (emailExists)
            {
                throw new ConflictException("Email already exists.");
            }

            var allowedRoles = new[] { "Administrator", "Manager", "Employee" };

            var role = allowedRoles.FirstOrDefault(x =>
                x.Equals(request.Role, StringComparison.OrdinalIgnoreCase)
            );

            if (role == null)
            {
                throw new BadRequestException(
                    "Invalid role. Allowed roles are Administrator, Manager and Employee."
                );
            }

            string passwordHash = _passwordHelper.HashPassword(request.Password);

            var user = new User
            {
                Username = request.Username,
                Email = request.Email,
                PasswordHash = passwordHash,
                Role = role,
                IsActive = true,
                IsDeleted = false,
                CreatedAt = DateTime.UtcNow,
            };

            await _userRepository.AddAsync(user);
            await _userRepository.SaveChangesAsync();

            var templatePath = Path.Combine(
                AppContext.BaseDirectory,
                "EmailTemplate",
                "WelcomeEmail.html"
            );

            var body = await File.ReadAllTextAsync(templatePath);

            body = body.Replace("{{Username}}", user.Username)
                .Replace("{{Email}}", user.Email)
                .Replace("{{Role}}", user.Role)
                .Replace("{{Password}}", request.Password);

            await _emailService.SendEmailAsync(user.Email, "Welcome to OIMS", body);

            return new CreatedUserResponseDto
            {
                Id = user.Id,
                Username = user.Username,
                Email = user.Email,
                Role = user.Role,
                IsActive = user.IsActive,
                CreatedAt = user.CreatedAt,
            };
        }
    }
}
