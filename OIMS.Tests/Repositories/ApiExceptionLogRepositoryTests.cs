using System;
using System.Threading.Tasks;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using OIMS.Data.DbContext;
using OIMS.Domain.Entities;
using OIMS.Infrastructure.Repositories;
using Xunit;

namespace OIMS.Tests.Repository
{
    public class ApiExceptionLogRepositoryTests
    {
        private AppDbContext CreateDbContext()
        {
            var options = new DbContextOptionsBuilder<AppDbContext>()
                .UseInMemoryDatabase(Guid.NewGuid().ToString())
                .Options;

            return new AppDbContext(options);
        }

        [Fact]
        public async Task AddAsync_ShouldAddApiExceptionLogToDatabase()
        {
            using var context = CreateDbContext();
            var repository = new ApiExceptionLogRepository(context);

            var log = new ApiExceptionLog
            {
                HttpMethod = "GET",
                RequestPath = "/api/products/999",
                ActionName = "GetProductById",
                ExceptionType = "NotFoundException",
                Message = "Product not found",
                StackTrace = "at OIMS.Application.Services...",
                StatusCode = 404,
                ErrorCode = "ERR_NOT_FOUND",
                CreatedAt = DateTime.UtcNow,
            };

            await repository.AddAsync(log);
            await repository.SaveChangesAsync();

            var savedLog = await context.ApiExceptionLogs.FirstOrDefaultAsync(x =>
                x.ErrorCode == "ERR_NOT_FOUND"
            );

            savedLog.Should().NotBeNull();
            savedLog!.HttpMethod.Should().Be("GET");
            savedLog.StatusCode.Should().Be(404);
            savedLog.Message.Should().Be("Product not found");
        }

        [Fact]
        public async Task AddAsync_ShouldAddLogWithUserId_WhenUserIsProvided()
        {
            using var context = CreateDbContext();

            var user = new User
            {
                Id = 1,
                Username = "testuser",
                Email = "test@example.com",
                PasswordHash = "hash",
                Role = "User",
            };
            context.Users.Add(user);
            await context.SaveChangesAsync();

            var repository = new ApiExceptionLogRepository(context);

            var log = new ApiExceptionLog
            {
                UserId = 1,
                User = user,
                HttpMethod = "POST",
                RequestPath = "/api/orders",
                ActionName = "CreateOrder",
                ExceptionType = "ValidationException",
                Message = "Invalid data",
                StatusCode = 400,
                CreatedAt = DateTime.UtcNow,
            };

            await repository.AddAsync(log);
            await repository.SaveChangesAsync();

            var savedLog = await context.ApiExceptionLogs.FirstOrDefaultAsync(x => x.UserId == 1);

            savedLog.Should().NotBeNull();
            savedLog!.ExceptionType.Should().Be("ValidationException");
            savedLog.UserId.Should().Be(1);
        }

        [Fact]
        public async Task SaveChangesAsync_ShouldPersistMultipleLogs()
        {
            using var context = CreateDbContext();
            var repository = new ApiExceptionLogRepository(context);

            var log1 = new ApiExceptionLog
            {
                HttpMethod = "GET",
                RequestPath = "/api/users",
                ExceptionType = "UnauthorizedAccessException",
                Message = "Access denied",
                StatusCode = 401,
            };

            var log2 = new ApiExceptionLog
            {
                HttpMethod = "PUT",
                RequestPath = "/api/settings",
                ExceptionType = "TimeoutException",
                Message = "Request timed out",
                StatusCode = 408,
            };

            await repository.AddAsync(log1);
            await repository.AddAsync(log2);
            await repository.SaveChangesAsync();

            var count = await context.ApiExceptionLogs.CountAsync();

            count.Should().Be(2);
        }
    }
}
