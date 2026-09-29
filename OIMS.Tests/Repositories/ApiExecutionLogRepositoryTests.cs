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
    public class ApiExecutionLogRepositoryTests
    {
        private AppDbContext CreateDbContext()
        {
            var options = new DbContextOptionsBuilder<AppDbContext>()
                .UseInMemoryDatabase(Guid.NewGuid().ToString())
                .Options;

            return new AppDbContext(options);
        }

        [Fact]
        public async Task AddAsync_ShouldAddApiExecutionLogToDatabase()
        {
            using var context = CreateDbContext();
            var repository = new ApiExecutionLogRepository(context);

            var log = new ApiExecutionLog
            {
                HttpMethod = "GET",
                RequestPath = "/api/products",
                ActionName = "GetProducts",
                StatusCode = 200,
                ExecutionTimeMs = 45,
                CreatedAt = DateTime.UtcNow,
            };

            await repository.AddAsync(log);
            await repository.SaveChangesAsync();

            var savedLog = await context.ApiExecutionLogs.FirstOrDefaultAsync(x =>
                x.RequestPath == "/api/products"
            );

            savedLog.Should().NotBeNull();
            savedLog!.HttpMethod.Should().Be("GET");
            savedLog.StatusCode.Should().Be(200);
            savedLog.ExecutionTimeMs.Should().Be(45);
        }

        [Fact]
        public async Task AddAsync_ShouldAddLogWithUserId_WhenUserIsProvided()
        {
            using var context = CreateDbContext();

            var user = new User
            {
                Id = 1,
                Username = "johndoe",
                Email = "john@example.com",
                PasswordHash = "hashedpass",
                Role = "Admin",
            };
            context.Users.Add(user);
            await context.SaveChangesAsync();

            var repository = new ApiExecutionLogRepository(context);

            var log = new ApiExecutionLog
            {
                UserId = 1,
                User = user,
                HttpMethod = "POST",
                RequestPath = "/api/orders",
                ActionName = "CreateOrder",
                StatusCode = 201,
                ExecutionTimeMs = 120,
                CreatedAt = DateTime.UtcNow,
            };

            await repository.AddAsync(log);
            await repository.SaveChangesAsync();

            var savedLog = await context.ApiExecutionLogs.FirstOrDefaultAsync(x => x.UserId == 1);

            savedLog.Should().NotBeNull();
            savedLog!.ActionName.Should().Be("CreateOrder");
            savedLog.UserId.Should().Be(1);
        }

        [Fact]
        public async Task SaveChangesAsync_ShouldPersistMultipleLogs()
        {
            using var context = CreateDbContext();
            var repository = new ApiExecutionLogRepository(context);

            var log1 = new ApiExecutionLog
            {
                HttpMethod = "GET",
                RequestPath = "/api/categories",
                ActionName = "GetCategories",
                StatusCode = 200,
                ExecutionTimeMs = 25,
            };

            var log2 = new ApiExecutionLog
            {
                HttpMethod = "DELETE",
                RequestPath = "/api/products/5",
                ActionName = "DeleteProduct",
                StatusCode = 204,
                ExecutionTimeMs = 60,
            };

            await repository.AddAsync(log1);
            await repository.AddAsync(log2);
            await repository.SaveChangesAsync();

            var count = await context.ApiExecutionLogs.CountAsync();

            count.Should().Be(2);
        }
    }
}
