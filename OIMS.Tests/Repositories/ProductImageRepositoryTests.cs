using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using OIMS.Data.DbContext;
using OIMS.Domain.Entities;
using OIMS.Infrastructure.Repositories;
using Xunit;

namespace OIMS.Tests.Repository
{
    public class ProductImageRepositoryTests
    {
        private AppDbContext CreateDbContext()
        {
            var options = new DbContextOptionsBuilder<AppDbContext>()
                .UseInMemoryDatabase(Guid.NewGuid().ToString())
                .Options;

            return new AppDbContext(options);
        }

        [Fact]
        public async Task GetProductAsync_ShouldReturnProduct_WhenProductExistsAndNotDeleted()
        {
            using var context = CreateDbContext();

            var product = new Product
            {
                Id = 1,
                Name = "Laptop",
                Sku = "SKU-LAP",
                CategoryId = 1,
                SellingPrice = 500,
                CostPrice = 400,
                IsDeleted = false,
            };

            context.Products.Add(product);
            await context.SaveChangesAsync();

            var repository = new ProductImageRepository(context);

            var result = await repository.GetProductAsync(1);

            result.Should().NotBeNull();
            result!.Name.Should().Be("Laptop");
        }

        [Fact]
        public async Task GetProductAsync_ShouldReturnNull_WhenProductIsDeleted()
        {
            using var context = CreateDbContext();

            var product = new Product
            {
                Id = 1,
                Name = "Laptop",
                Sku = "SKU-LAP",
                CategoryId = 1,
                SellingPrice = 500,
                CostPrice = 400,
                IsDeleted = true,
            };

            context.Products.Add(product);
            await context.SaveChangesAsync();

            var repository = new ProductImageRepository(context);

            var result = await repository.GetProductAsync(1);

            result.Should().BeNull();
        }

        [Fact]
        public async Task GetProductAsync_ShouldReturnNull_WhenProductDoesNotExist()
        {
            using var context = CreateDbContext();
            var repository = new ProductImageRepository(context);

            var result = await repository.GetProductAsync(999);

            result.Should().BeNull();
        }

        [Fact]
        public async Task GetImagesByProductIdAsync_ShouldReturnOnlyActiveImagesForGivenProduct()
        {
            using var context = CreateDbContext();

            var image1 = new ProductImage
            {
                Id = 1,
                ProductId = 10,
                FileUrl = "/images/1.jpg",
                IsDeleted = false,
            };

            var image2 = new ProductImage
            {
                Id = 2,
                ProductId = 10,
                FileUrl = "/images/2.jpg",
                IsDeleted = true,
            };

            var otherProductImage = new ProductImage
            {
                Id = 3,
                ProductId = 20,
                FileUrl = "/images/3.jpg",
                IsDeleted = false,
            };

            context.ProductImages.AddRange(image1, image2, otherProductImage);
            await context.SaveChangesAsync();

            var repository = new ProductImageRepository(context);

            var result = await repository.GetImagesByProductIdAsync(10);

            result.Should().HaveCount(1);
            result[0].Id.Should().Be(1);
            result[0].FileUrl.Should().Be("/images/1.jpg");
        }

        [Fact]
        public async Task GetImagesByProductIdAsync_ShouldReturnEmptyList_WhenNoImagesFound()
        {
            using var context = CreateDbContext();
            var repository = new ProductImageRepository(context);

            var result = await repository.GetImagesByProductIdAsync(5);

            result.Should().BeEmpty();
        }

        [Fact]
        public async Task GetImageCountAsync_ShouldReturnCorrectCount_ExcludingDeletedImages()
        {
            using var context = CreateDbContext();

            context.ProductImages.AddRange(
                new ProductImage
                {
                    Id = 1,
                    ProductId = 7,
                    FileUrl = "/img1.png",
                    IsDeleted = false,
                },
                new ProductImage
                {
                    Id = 2,
                    ProductId = 7,
                    FileUrl = "/img2.png",
                    IsDeleted = false,
                },
                new ProductImage
                {
                    Id = 3,
                    ProductId = 7,
                    FileUrl = "/img3.png",
                    IsDeleted = true,
                },
                new ProductImage
                {
                    Id = 4,
                    ProductId = 8,
                    FileUrl = "/img4.png",
                    IsDeleted = false,
                }
            );
            await context.SaveChangesAsync();

            var repository = new ProductImageRepository(context);

            var count = await repository.GetImageCountAsync(7);

            count.Should().Be(2);
        }

        [Fact]
        public async Task GetImageCountAsync_ShouldReturnZero_WhenProductHasNoImages()
        {
            using var context = CreateDbContext();
            var repository = new ProductImageRepository(context);

            var count = await repository.GetImageCountAsync(99);

            count.Should().Be(0);
        }

        [Fact]
        public async Task AddAsync_ShouldAddProductImageToDatabase()
        {
            using var context = CreateDbContext();
            var repository = new ProductImageRepository(context);

            var image = new ProductImage
            {
                ProductId = 1,
                FileUrl = "/uploads/test.png",
                OriginalFileName = "test.png",
                ContentType = "image/png",
                FileSize = 1024,
                IsDeleted = false,
            };

            await repository.AddAsync(image);
            await repository.SaveChangesAsync();

            var savedImage = await context.ProductImages.FirstOrDefaultAsync(x =>
                x.FileUrl == "/uploads/test.png"
            );

            savedImage.Should().NotBeNull();
            savedImage!.ProductId.Should().Be(1);
            savedImage.OriginalFileName.Should().Be("test.png");
        }
    }
}
