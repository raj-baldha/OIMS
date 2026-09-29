using System.Threading.Tasks;
using AutoMapper;
using FluentAssertions;
using Moq;
using OIMS.Application.DTOs.Request;
using OIMS.Application.DTOs.Response;
using OIMS.Application.Interfaces.Helpers;
using OIMS.Application.Interfaces.Repositories;
using OIMS.Application.Services;
using OIMS.Domain.Entities;
using OIMS.Domain.Exceptions;
using Xunit;

namespace OIMS.Tests.Services
{
    public class AuthServiceTests
    {
        private readonly Mock<IUserRepository> _userRepositoryMock;
        private readonly Mock<IPasswordHelper> _passwordHelperMock;
        private readonly Mock<IJwtHelper> _jwtHelperMock;
        private readonly Mock<IMapper> _mapperMock;
        private readonly AuthService _authService;

        public AuthServiceTests()
        {
            _userRepositoryMock = new Mock<IUserRepository>();
            _passwordHelperMock = new Mock<IPasswordHelper>();
            _jwtHelperMock = new Mock<IJwtHelper>();
            _mapperMock = new Mock<IMapper>();

            _authService = new AuthService(
                _userRepositoryMock.Object,
                _passwordHelperMock.Object,
                _jwtHelperMock.Object,
                _mapperMock.Object
            );
        }

        [Fact]
        public async Task LoginAsync_ShouldThrowUnauthorizedException_WhenUserDoesNotExist()
        {
            var request = new LoginRequestDto
            {
                Email = "notfound@example.com",
                Password = "Password123",
            };

            _userRepositoryMock
                .Setup(repo => repo.GetByEmailAsync(request.Email))
                .ReturnsAsync((User?)null);

            var action = async () => await _authService.LoginAsync(request);

            await action
                .Should()
                .ThrowAsync<UnauthorizedException>()
                .WithMessage("Invalid email or password.");
        }

        [Fact]
        public async Task LoginAsync_ShouldThrowUnauthorizedException_WhenUserIsInactive()
        {
            var request = new LoginRequestDto
            {
                Email = "inactive@example.com",
                Password = "Password123",
            };

            var user = new User
            {
                Id = 1,
                Email = request.Email,
                PasswordHash = "hashedPassword",
                IsActive = false,
            };

            _userRepositoryMock
                .Setup(repo => repo.GetByEmailAsync(request.Email))
                .ReturnsAsync(user);

            var action = async () => await _authService.LoginAsync(request);

            await action
                .Should()
                .ThrowAsync<UnauthorizedException>()
                .WithMessage("Your account is inactive.");
        }

        [Fact]
        public async Task LoginAsync_ShouldThrowUnauthorizedException_WhenPasswordIsInvalid()
        {
            var request = new LoginRequestDto
            {
                Email = "test@example.com",
                Password = "WrongPassword",
            };

            var user = new User
            {
                Id = 1,
                Email = request.Email,
                PasswordHash = "hashedPassword",
                IsActive = true,
            };

            _userRepositoryMock
                .Setup(repo => repo.GetByEmailAsync(request.Email))
                .ReturnsAsync(user);

            _passwordHelperMock
                .Setup(helper => helper.VerifyPassword(request.Password, user.PasswordHash))
                .Returns(false);

            var action = async () => await _authService.LoginAsync(request);

            await action
                .Should()
                .ThrowAsync<UnauthorizedException>()
                .WithMessage("Invalid email or password.");
        }

        [Fact]
        public async Task LoginAsync_ShouldReturnUserAndToken_WhenCredentialsAreValid()
        {
            var request = new LoginRequestDto
            {
                Email = "valid@example.com",
                Password = "CorrectPassword",
            };

            var user = new User
            {
                Id = 1,
                Username = "testuser",
                Email = request.Email,
                PasswordHash = "hashedPassword",
                Role = "Admin",
                IsActive = true,
            };

            var expectedResponse = new LoginResponseDto
            {
                Id = 1,
                Username = "testuser",
                Email = request.Email,
                Role = "Admin",
            };

            _userRepositoryMock
                .Setup(repo => repo.GetByEmailAsync(request.Email))
                .ReturnsAsync(user);

            _passwordHelperMock
                .Setup(helper => helper.VerifyPassword(request.Password, user.PasswordHash))
                .Returns(true);

            _jwtHelperMock
                .Setup(jwt => jwt.GenerateToken(user.Id, user.Email, user.Role))
                .Returns("jwt-token-xyz");

            _mapperMock
                .Setup(mapper => mapper.Map<LoginResponseDto>(user))
                .Returns(expectedResponse);

            var result = await _authService.LoginAsync(request);

            result.User.Should().NotBeNull();
            result.User.Id.Should().Be(1);
            result.User.Email.Should().Be("valid@example.com");
            result.User.Role.Should().Be("Admin");
            result.Token.Should().Be("jwt-token-xyz");
        }
    }
}
