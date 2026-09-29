using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using OIMS.Application.DTOs.Request;
using OIMS.Data.DbContext;
using OIMS.Domain.Entities;
using OIMS.Domain.Enums;
using OIMS.Infrastructure.Repositories;
using Xunit;

namespace OIMS.Tests.Repository
{
    public class InventoryRepositoryTests
    {
        private AppDbContext CreateDbContext()
        {
            var options = new DbContextOptionsBuilder<AppDbContext>()
                .UseInMemoryDatabase(Guid.NewGuid().ToString())
                .Options;

            return new AppDbContext(options);
        }

        private Category CreateCategory(int id = 1, string name = "Electronics")
        {
            return new Category
            {
                Id = id,
                Name = name,
                IsDeleted = false,
            };
        }

        private Product CreateProduct(
            int id,
            Category category,
            int qty,
            int minStock,
            bool isActive = true,
            bool isDeleted = false
        )
        {
            return new Product
            {
                Id = id,
                Name = "Product " + id,
                Sku = "SKU-" + id,
                CategoryId = category.Id,
                Category = category,
                QuantityOnHand = qty,
                MinStockLevel = minStock,
                SellingPrice = 50,
                CostPrice = 30,
                IsActive = isActive,
                IsDeleted = isDeleted,
            };
        }

        [Fact]
        public async Task GetLowStockProductsAsync_ShouldReturnProducts_WhenQuantityIsLessThanOrEqualToMinStockLevel()
        {
            using var context = CreateDbContext();

            var category = CreateCategory(1, "Electronics");
            context.Categories.Add(category);

            var lowProduct1 = CreateProduct(1, category, qty: 2, minStock: 10);
            var lowProduct2 = CreateProduct(2, category, qty: 5, minStock: 5);
            var normalProduct = CreateProduct(3, category, qty: 20, minStock: 10);

            context.Products.AddRange(lowProduct1, lowProduct2, normalProduct);
            await context.SaveChangesAsync();

            var repository = new InventoryRepository(context);

            var result = await repository.GetLowStockProductsAsync();

            result.Should().HaveCount(2);
            result[0].Id.Should().Be(1);
            result[0].CategoryName.Should().Be("Electronics");
            result[0].Shortage.Should().Be(8);

            result[1].Id.Should().Be(2);
            result[1].Shortage.Should().Be(0);
        }

        [Fact]
        public async Task GetLowStockProductsAsync_ShouldExcludeDeletedAndInactiveProducts()
        {
            using var context = CreateDbContext();

            var category = CreateCategory(1, "Hardware");
            context.Categories.Add(category);

            var deletedProduct = CreateProduct(
                1,
                category,
                qty: 1,
                minStock: 10,
                isActive: true,
                isDeleted: true
            );
            var inactiveProduct = CreateProduct(
                2,
                category,
                qty: 1,
                minStock: 10,
                isActive: false,
                isDeleted: false
            );

            context.Products.AddRange(deletedProduct, inactiveProduct);
            await context.SaveChangesAsync();

            var repository = new InventoryRepository(context);

            var result = await repository.GetLowStockProductsAsync();

            result.Should().BeEmpty();
        }

        [Fact]
        public async Task ProductExistsAsync_ShouldReturnTrue_WhenProductExistsAndNotDeleted()
        {
            using var context = CreateDbContext();

            var category = CreateCategory();
            context.Categories.Add(category);

            var product = CreateProduct(1, category, qty: 10, minStock: 5, isDeleted: false);
            context.Products.Add(product);
            await context.SaveChangesAsync();

            var repository = new InventoryRepository(context);

            var result = await repository.ProductExistsAsync(1);

            result.Should().BeTrue();
        }

        [Fact]
        public async Task ProductExistsAsync_ShouldReturnFalse_WhenProductIsDeleted()
        {
            using var context = CreateDbContext();

            var category = CreateCategory();
            context.Categories.Add(category);

            var product = CreateProduct(1, category, qty: 10, minStock: 5, isDeleted: true);
            context.Products.Add(product);
            await context.SaveChangesAsync();

            var repository = new InventoryRepository(context);

            var result = await repository.ProductExistsAsync(1);

            result.Should().BeFalse();
        }

        [Fact]
        public async Task ProductExistsAsync_ShouldReturnFalse_WhenProductDoesNotExist()
        {
            using var context = CreateDbContext();
            var repository = new InventoryRepository(context);

            var result = await repository.ProductExistsAsync(999);

            result.Should().BeFalse();
        }

        [Fact]
        public async Task GetInventoryHistoryAsync_ShouldReturnMappedHistoryForGivenProduct()
        {
            using var context = CreateDbContext();

            var user = new User
            {
                Id = 1,
                Username = "admin_user",
                Email = "admin@example.com",
                PasswordHash = "hash",
                Role = "Admin",
            };
            context.Users.Add(user);

            var t1 = new InventoryTransaction
            {
                Id = 1,
                ProductId = 10,
                ChangeType = InventoryChangeType.ManualAdjustment,
                QuantityBefore = 10,
                QuantityChange = 5,
                QuantityAfter = 15,
                Notes = "Stock added",
                CreatedBy = 1,
                Creator = user,
                CreatedAt = DateTime.UtcNow.AddMinutes(-10),
            };

            var otherProductTransaction = new InventoryTransaction
            {
                Id = 2,
                ProductId = 20,
                ChangeType = InventoryChangeType.Damage,
                QuantityBefore = 0,
                QuantityChange = 5,
                QuantityAfter = 5,
                CreatedAt = DateTime.UtcNow,
            };

            context.InventoryTransactions.AddRange(t1, otherProductTransaction);
            await context.SaveChangesAsync();

            var repository = new InventoryRepository(context);

            var request = new GetInventoryHistoryRequestDto { Page = 1 };

            var (data, totalRecords) = await repository.GetInventoryHistoryAsync(
                10,
                request,
                pageSize: 10
            );

            totalRecords.Should().Be(1);
            data.Should().HaveCount(1);
            data[0].ProductId.Should().Be(10);
            data[0].CreatedByName.Should().Be("admin_user");
            data[0].QuantityAfter.Should().Be(15);
            data[0].Notes.Should().Be("Stock added");
        }

        [Fact]
        public async Task GetInventoryHistoryAsync_ShouldPaginateProperly()
        {
            using var context = CreateDbContext();

            var date = DateTime.UtcNow;

            context.InventoryTransactions.AddRange(
                new InventoryTransaction
                {
                    Id = 1,
                    ProductId = 5,
                    ChangeType = InventoryChangeType.ManualAdjustment,
                    QuantityBefore = 10,
                    QuantityChange = 1,
                    QuantityAfter = 11,
                    CreatedAt = date.AddMinutes(1),
                },
                new InventoryTransaction
                {
                    Id = 2,
                    ProductId = 5,
                    ChangeType = InventoryChangeType.ManualAdjustment,
                    QuantityBefore = 11,
                    QuantityChange = 1,
                    QuantityAfter = 12,
                    CreatedAt = date.AddMinutes(2),
                },
                new InventoryTransaction
                {
                    Id = 3,
                    ProductId = 5,
                    ChangeType = InventoryChangeType.ManualAdjustment,
                    QuantityBefore = 12,
                    QuantityChange = 1,
                    QuantityAfter = 13,
                    CreatedAt = date.AddMinutes(3),
                }
            );
            await context.SaveChangesAsync();

            var repository = new InventoryRepository(context);

            var request = new GetInventoryHistoryRequestDto { Page = 2 };

            var (data, totalRecords) = await repository.GetInventoryHistoryAsync(
                5,
                request,
                pageSize: 2
            );

            totalRecords.Should().Be(3);
            data.Should().HaveCount(1);
            data[0].Id.Should().Be(3);
            data[0].QuantityAfter.Should().Be(13);
        }

        [Fact]
        public async Task GetInventoryHistoryAsync_ShouldDefaultPageToOne_WhenPageIsZeroOrNegative()
        {
            using var context = CreateDbContext();

            context.InventoryTransactions.Add(
                new InventoryTransaction
                {
                    Id = 1,
                    ProductId = 5,
                    ChangeType = InventoryChangeType.ManualAdjustment,
                    QuantityBefore = 5,
                    QuantityChange = 5,
                    QuantityAfter = 10,
                    CreatedAt = DateTime.UtcNow,
                }
            );
            await context.SaveChangesAsync();

            var repository = new InventoryRepository(context);

            var request = new GetInventoryHistoryRequestDto { Page = -1 };

            var (data, totalRecords) = await repository.GetInventoryHistoryAsync(
                5,
                request,
                pageSize: 10
            );

            totalRecords.Should().Be(1);
            data.Should().HaveCount(1);
            data[0].Id.Should().Be(1);
        }

        [Fact]
        public async Task GetInventoryHistoryAsync_ShouldHandleNullCreatorGracefully()
        {
            using var context = CreateDbContext();

            context.InventoryTransactions.Add(
                new InventoryTransaction
                {
                    Id = 1,
                    ProductId = 8,
                    ChangeType = InventoryChangeType.ManualAdjustment,
                    QuantityBefore = 5,
                    QuantityChange = 1,
                    QuantityAfter = 6,
                    CreatedBy = null,
                    Creator = null,
                    CreatedAt = DateTime.UtcNow,
                }
            );
            await context.SaveChangesAsync();

            var repository = new InventoryRepository(context);

            var request = new GetInventoryHistoryRequestDto { Page = 1 };

            var (data, totalRecords) = await repository.GetInventoryHistoryAsync(
                8,
                request,
                pageSize: 10
            );

            totalRecords.Should().Be(1);
            data.Should().HaveCount(1);
            data[0].CreatedByName.Should().BeNull();
            data[0].CreatedBy.Should().BeNull();
        }
    }
}
