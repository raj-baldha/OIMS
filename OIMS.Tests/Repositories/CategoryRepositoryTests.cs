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
    public class CategoryRepositoryTests
    {
        private AppDbContext CreateDbContext()
        {
            var options = new DbContextOptionsBuilder<AppDbContext>()
                .UseInMemoryDatabase(Guid.NewGuid().ToString())
                .Options;

            return new AppDbContext(options);
        }

        [Fact]
        public async Task IsNameExistsAsync_ShouldReturnTrue_WhenCategoryNameExistsAndNotDeleted()
        {
            using var context = CreateDbContext();

            var category = new Category { Name = "Electronics", IsDeleted = false };

            context.Categories.Add(category);
            await context.SaveChangesAsync();

            var repository = new CategoryRepository(context);

            var result = await repository.IsNameExistsAsync("Electronics");

            result.Should().BeTrue();
        }

        [Fact]
        public async Task IsNameExistsAsync_ShouldReturnFalse_WhenCategoryNameIsDeleted()
        {
            using var context = CreateDbContext();

            var category = new Category { Name = "Electronics", IsDeleted = true };

            context.Categories.Add(category);
            await context.SaveChangesAsync();

            var repository = new CategoryRepository(context);

            var result = await repository.IsNameExistsAsync("Electronics");

            result.Should().BeFalse();
        }

        [Fact]
        public async Task IsNameExistsAsync_ShouldReturnFalse_WhenCategoryNameDoesNotExist()
        {
            using var context = CreateDbContext();
            var repository = new CategoryRepository(context);

            var result = await repository.IsNameExistsAsync("Sports");

            result.Should().BeFalse();
        }

        [Fact]
        public async Task AddAsync_ShouldAddCategoryToDatabase()
        {
            using var context = CreateDbContext();
            var repository = new CategoryRepository(context);

            var category = new Category { Name = "Books", IsDeleted = false };

            await repository.AddAsync(category);
            await repository.SaveChangesAsync();

            var savedCategory = await context.Categories.FirstOrDefaultAsync(x =>
                x.Name == "Books"
            );

            savedCategory.Should().NotBeNull();
            savedCategory!.Name.Should().Be("Books");
        }

        [Fact]
        public async Task SaveChangesAsync_ShouldPersistMultipleOperations()
        {
            using var context = CreateDbContext();
            var repository = new CategoryRepository(context);

            var cat1 = new Category { Name = "Toys", IsDeleted = false };
            var cat2 = new Category { Name = "Home", IsDeleted = false };

            await repository.AddAsync(cat1);
            await repository.AddAsync(cat2);
            await repository.SaveChangesAsync();

            var count = await context.Categories.CountAsync();

            count.Should().Be(2);
        }
    }
}
