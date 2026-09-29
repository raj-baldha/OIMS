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
    public class UserRepositoryTests
    {
        private AppDbContext CreateDbContext()
        {
            var options = new DbContextOptionsBuilder<AppDbContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;

            return new AppDbContext(options);
        }

        private static User CreateSampleUser(
            string email = "test@example.com",
            bool isDeleted = false
        )
        {
            return new User
            {
                Username = "testuser",
                Email = email,
                PasswordHash = "hashed-password-123",
                Role = "Employee",
                IsActive = true,
                IsDeleted = isDeleted,
                CreatedAt = DateTime.UtcNow,
            };
        }

        [Fact]
        public async Task IsEmailExistsAsync_ShouldReturnTrue_WhenEmailExistsAndNotDeleted()
        {
            using var context = CreateDbContext();
            var user = CreateSampleUser("raj@gmail.com", isDeleted: false);
            await context.Users.AddAsync(user);
            await context.SaveChangesAsync();

            var repository = new UserRepository(context);

            var result = await repository.IsEmailExistsAsync("raj@gmail.com");

            result.Should().BeTrue();
        }

        [Fact]
        public async Task IsEmailExistsAsync_ShouldReturnFalse_WhenUserIsDeleted()
        {
            using var context = CreateDbContext();
            var user = CreateSampleUser("raj@gmail.com", isDeleted: true);
            await context.Users.AddAsync(user);
            await context.SaveChangesAsync();

            var repository = new UserRepository(context);

            var result = await repository.IsEmailExistsAsync("raj@gmail.com");

            result.Should().BeFalse();
        }

        [Fact]
        public async Task IsEmailExistsAsync_ShouldReturnFalse_WhenEmailDoesNotExist()
        {
            using var context = CreateDbContext();
            var repository = new UserRepository(context);

            var result = await repository.IsEmailExistsAsync("notfound@gmail.com");

            result.Should().BeFalse();
        }

        [Fact]
        public async Task GetByEmailAsync_ShouldReturnUser_WhenUserExistsAndNotDeleted()
        {
            using var context = CreateDbContext();
            var user = CreateSampleUser("john@example.com", isDeleted: false);
            await context.Users.AddAsync(user);
            await context.SaveChangesAsync();

            var repository = new UserRepository(context);

            var result = await repository.GetByEmailAsync("john@example.com");

            result.Should().NotBeNull();
            result!.Email.Should().Be("john@example.com");
            result.Username.Should().Be(user.Username);
            result.IsDeleted.Should().BeFalse();
        }

        [Fact]
        public async Task GetByEmailAsync_ShouldReturnNull_WhenUserExistsButIsDeleted()
        {
            using var context = CreateDbContext();
            var user = CreateSampleUser("deleted@example.com", isDeleted: true);
            await context.Users.AddAsync(user);
            await context.SaveChangesAsync();

            var repository = new UserRepository(context);

            var result = await repository.GetByEmailAsync("deleted@example.com");

            result.Should().BeNull();
        }

        [Fact]
        public async Task GetByEmailAsync_ShouldReturnNull_WhenEmailDoesNotExist()
        {
            using var context = CreateDbContext();
            var repository = new UserRepository(context);

            var result = await repository.GetByEmailAsync("nonexistent@example.com");

            result.Should().BeNull();
        }

        [Fact]
        public async Task AddAsync_ShouldAddUserToContext_WithAddedEntityState()
        {
            using var context = CreateDbContext();
            var repository = new UserRepository(context);
            var newUser = CreateSampleUser("newuser@example.com");

            await repository.AddAsync(newUser);

            var entry = context.Entry(newUser);
            entry.State.Should().Be(EntityState.Added);
        }

        [Fact]
        public async Task SaveChangesAsync_ShouldPersistAddedUserToDatabase()
        {
            using var context = CreateDbContext();
            var repository = new UserRepository(context);
            var newUser = CreateSampleUser("persisted@example.com");

            await repository.AddAsync(newUser);
            await repository.SaveChangesAsync();

            var savedUser = await context.Users.FirstOrDefaultAsync(u =>
                u.Email == "persisted@example.com"
            );
            savedUser.Should().NotBeNull();
            savedUser!.Id.Should().BeGreaterThan(0);
            savedUser.Email.Should().Be("persisted@example.com");
        }
    }
}
