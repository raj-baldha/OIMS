using System;
using System.IO;
using System.Threading.Tasks;
using FluentAssertions;
using Moq;
using OIMS.Application.DTOs.Request;
using OIMS.Application.DTOs.Response;
using OIMS.Application.Interfaces.Helpers;
using OIMS.Application.Interfaces.Repositories;
using OIMS.Application.Interfaces.Services;
using OIMS.Application.Services;
using OIMS.Domain.Entities;
using OIMS.Domain.Exceptions;
using Xunit;

namespace OIMS.Tests.Services
{
    public class UserServiceTests
    {
        private static readonly object FileLock = new object();
        private readonly Mock<IUserRepository> _userRepositoryMock;
        private readonly Mock<IPasswordHelper> _passwordHelperMock;
        private readonly Mock<IEmailService> _emailServiceMock;
        private readonly UserService _userService;

        public UserServiceTests()
        {
            _userRepositoryMock = new Mock<IUserRepository>();
            _passwordHelperMock = new Mock<IPasswordHelper>();
            _emailServiceMock = new Mock<IEmailService>();

            _userService = new UserService(
                _userRepositoryMock.Object,
                _passwordHelperMock.Object,
                _emailServiceMock.Object
            );

            EnsureEmailTemplateExists();
        }

        private static void EnsureEmailTemplateExists()
        {
            lock (FileLock)
            {
                var templateDir = Path.Combine(AppContext.BaseDirectory, "EmailTemplate");
                if (!Directory.Exists(templateDir))
                {
                    Directory.CreateDirectory(templateDir);
                }

                var templatePath = Path.Combine(templateDir, "WelcomeEmail.html");
                if (!File.Exists(templatePath))
                {
                    File.WriteAllText(
                        templatePath,
                        "Hello {{Username}}, email: {{Email}}, pass: {{Password}}, role: {{Role}}"
                    );
                }
            }
        }

        [Fact]
        public async Task CreateUserAsync_ShouldThrowConflictException_WhenEmailAlreadyExists()
        {
            var request = new CreateUserRequestDto
            {
                Username = "testuser",
                Email = "duplicate@example.com",
                Password = "Password123",
                Role = "Employee",
            };

            _userRepositoryMock
                .Setup(repo => repo.IsEmailExistsAsync(request.Email))
                .ReturnsAsync(true);

            var action = async () => await _userService.CreateUserAsync(request);

            await action
                .Should()
                .ThrowAsync<ConflictException>()
                .WithMessage("Email already exists.");

            _userRepositoryMock.Verify(repo => repo.AddAsync(It.IsAny<User>()), Times.Never);
            _userRepositoryMock.Verify(repo => repo.SaveChangesAsync(), Times.Never);
            _emailServiceMock.Verify(
                s => s.SendEmailAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()),
                Times.Never
            );
        }

        [Theory]
        [InlineData("SuperAdmin")]
        [InlineData("Customer")]
        [InlineData("Guest")]
        public async Task CreateUserAsync_ShouldThrowBadRequestException_WhenRoleIsInvalid(
            string invalidRole
        )
        {
            var request = new CreateUserRequestDto
            {
                Username = "testuser",
                Email = "test@example.com",
                Password = "Password123",
                Role = invalidRole,
            };

            _userRepositoryMock
                .Setup(repo => repo.IsEmailExistsAsync(request.Email))
                .ReturnsAsync(false);

            var action = async () => await _userService.CreateUserAsync(request);

            await action
                .Should()
                .ThrowAsync<BadRequestException>()
                .WithMessage(
                    "Invalid role. Allowed roles are Administrator, Manager and Employee."
                );

            _userRepositoryMock.Verify(repo => repo.AddAsync(It.IsAny<User>()), Times.Never);
            _userRepositoryMock.Verify(repo => repo.SaveChangesAsync(), Times.Never);
            _emailServiceMock.Verify(
                s => s.SendEmailAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()),
                Times.Never
            );
        }

        [Fact]
        public async Task CreateUserAsync_ShouldCreateAdministratorUserAndSendEmail_WhenRequestIsValid()
        {
            var request = new CreateUserRequestDto
            {
                Username = "admin_user",
                Email = "admin@example.com",
                Password = "AdminPassword123!",
                Role = "Administrator",
            };

            _userRepositoryMock
                .Setup(repo => repo.IsEmailExistsAsync(request.Email))
                .ReturnsAsync(false);

            _passwordHelperMock
                .Setup(helper => helper.HashPassword(request.Password))
                .Returns("hashedPassword_admin");

            _userRepositoryMock
                .Setup(repo => repo.AddAsync(It.IsAny<User>()))
                .Returns(Task.CompletedTask);

            _userRepositoryMock.Setup(repo => repo.SaveChangesAsync()).Returns(Task.CompletedTask);

            _emailServiceMock
                .Setup(s => s.SendEmailAsync(request.Email, "Welcome to OIMS", It.IsAny<string>()))
                .Returns(Task.CompletedTask);

            var result = await _userService.CreateUserAsync(request);

            result.Should().NotBeNull();
            result.Username.Should().Be("admin_user");
            result.Email.Should().Be("admin@example.com");
            result.Role.Should().Be("Administrator");
            result.IsActive.Should().BeTrue();

            _userRepositoryMock.Verify(
                repo =>
                    repo.AddAsync(
                        It.Is<User>(u =>
                            u.Username == "admin_user"
                            && u.Email == "admin@example.com"
                            && u.PasswordHash == "hashedPassword_admin"
                            && u.Role == "Administrator"
                            && u.IsActive
                            && !u.IsDeleted
                        )
                    ),
                Times.Once
            );

            _userRepositoryMock.Verify(repo => repo.SaveChangesAsync(), Times.Once);

            _emailServiceMock.Verify(
                s =>
                    s.SendEmailAsync(
                        request.Email,
                        "Welcome to OIMS",
                        It.Is<string>(body =>
                            body.Contains("admin_user")
                            && body.Contains("admin@example.com")
                            && body.Contains("AdminPassword123!")
                            && body.Contains("Administrator")
                        )
                    ),
                Times.Once
            );
        }

        [Fact]
        public async Task CreateUserAsync_ShouldCreateEmployeeUser_WhenRoleIsMixedCase()
        {
            var request = new CreateUserRequestDto
            {
                Username = "emp_user",
                Email = "emp@example.com",
                Password = "EmpPassword123!",
                Role = "employee",
            };

            _userRepositoryMock
                .Setup(repo => repo.IsEmailExistsAsync(request.Email))
                .ReturnsAsync(false);

            _passwordHelperMock
                .Setup(helper => helper.HashPassword(request.Password))
                .Returns("hashedPassword_emp");

            _userRepositoryMock
                .Setup(repo => repo.AddAsync(It.IsAny<User>()))
                .Returns(Task.CompletedTask);

            _userRepositoryMock.Setup(repo => repo.SaveChangesAsync()).Returns(Task.CompletedTask);

            _emailServiceMock
                .Setup(s => s.SendEmailAsync(request.Email, "Welcome to OIMS", It.IsAny<string>()))
                .Returns(Task.CompletedTask);

            var result = await _userService.CreateUserAsync(request);

            result.Should().NotBeNull();
            result.Username.Should().Be("emp_user");
            result.Role.Should().Be("Employee");

            _userRepositoryMock.Verify(
                repo => repo.AddAsync(It.Is<User>(u => u.Role == "Employee")),
                Times.Once
            );
        }

        [Fact]
        public async Task CreateUserAsync_ShouldCreateManagerUser_WhenRoleIsUppercase()
        {
            var request = new CreateUserRequestDto
            {
                Username = "mgr_user",
                Email = "mgr@example.com",
                Password = "MgrPassword123!",
                Role = "MANAGER",
            };

            _userRepositoryMock
                .Setup(repo => repo.IsEmailExistsAsync(request.Email))
                .ReturnsAsync(false);

            _passwordHelperMock
                .Setup(helper => helper.HashPassword(request.Password))
                .Returns("hashedPassword_mgr");

            _userRepositoryMock
                .Setup(repo => repo.AddAsync(It.IsAny<User>()))
                .Returns(Task.CompletedTask);

            _userRepositoryMock.Setup(repo => repo.SaveChangesAsync()).Returns(Task.CompletedTask);

            _emailServiceMock
                .Setup(s => s.SendEmailAsync(request.Email, "Welcome to OIMS", It.IsAny<string>()))
                .Returns(Task.CompletedTask);

            var result = await _userService.CreateUserAsync(request);

            result.Should().NotBeNull();
            result.Username.Should().Be("mgr_user");
            result.Role.Should().Be("Manager");

            _userRepositoryMock.Verify(
                repo => repo.AddAsync(It.Is<User>(u => u.Role == "Manager")),
                Times.Once
            );
        }
    }
}
