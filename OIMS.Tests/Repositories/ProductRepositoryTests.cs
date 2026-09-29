using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Moq;
using OIMS.Application.DTOs.Request;
using OIMS.Data.DbContext;
using OIMS.Domain.Entities;
using OIMS.Domain.Enums;
using OIMS.Infrastructure.Repositories;
using Xunit;

namespace OIMS.Tests.Repository
{
    public class ProductRepositoryTests
    {
        private AppDbContext CreateDbContext()
        {
            var options = new DbContextOptionsBuilder<AppDbContext>()
                .UseInMemoryDatabase(Guid.NewGuid().ToString())
                .Options;

            return new AppDbContext(options);
        }

        private IHttpContextAccessor CreateMockHttpContextAccessor()
        {
            var httpContext = new DefaultHttpContext();
            httpContext.Request.Scheme = "https";
            httpContext.Request.Host = new HostString("localhost", 5001);

            var mockAccessor = new Mock<IHttpContextAccessor>();
            mockAccessor.Setup(x => x.HttpContext).Returns(httpContext);

            return mockAccessor.Object;
        }

        [Fact]
        public async Task IsSkuExistsAsync_ShouldReturnTrue_WhenSkuExists()
        {
            using var context = CreateDbContext();
            var accessor = CreateMockHttpContextAccessor();

            var product = new Product
            {
                Name = "Mouse",
                Sku = "SKU-001",
                CategoryId = 1,
                SellingPrice = 50,
                CostPrice = 30,
                IsDeleted = false,
            };

            context.Products.Add(product);
            await context.SaveChangesAsync();

            var repository = new ProductRepository(context, accessor);

            var result = await repository.IsSkuExistsAsync("SKU-001");

            result.Should().BeTrue();
        }

        [Fact]
        public async Task IsSkuExistsAsync_ShouldReturnFalse_WhenSkuIsDeleted()
        {
            using var context = CreateDbContext();
            var accessor = CreateMockHttpContextAccessor();

            var product = new Product
            {
                Name = "Mouse",
                Sku = "SKU-001",
                CategoryId = 1,
                SellingPrice = 50,
                CostPrice = 30,
                IsDeleted = true,
            };

            context.Products.Add(product);
            await context.SaveChangesAsync();

            var repository = new ProductRepository(context, accessor);

            var result = await repository.IsSkuExistsAsync("SKU-001");

            result.Should().BeFalse();
        }

        [Fact]
        public async Task IsSkuExistsAsync_ShouldReturnFalse_WhenSkuDoesNotExist()
        {
            using var context = CreateDbContext();
            var accessor = CreateMockHttpContextAccessor();
            var repository = new ProductRepository(context, accessor);

            var result = await repository.IsSkuExistsAsync("NOT-EXIST");

            result.Should().BeFalse();
        }

        [Fact]
        public async Task IsCategoryExistsAsync_ShouldReturnTrue_WhenCategoryExists()
        {
            using var context = CreateDbContext();
            var accessor = CreateMockHttpContextAccessor();

            var category = new Category
            {
                Id = 1,
                Name = "Electronics",
                IsDeleted = false,
            };

            context.Categories.Add(category);
            await context.SaveChangesAsync();

            var repository = new ProductRepository(context, accessor);

            var result = await repository.IsCategoryExistsAsync(1);

            result.Should().BeTrue();
        }

        [Fact]
        public async Task IsCategoryExistsAsync_ShouldReturnFalse_WhenCategoryIsDeleted()
        {
            using var context = CreateDbContext();
            var accessor = CreateMockHttpContextAccessor();

            var category = new Category
            {
                Id = 1,
                Name = "Electronics",
                IsDeleted = true,
            };

            context.Categories.Add(category);
            await context.SaveChangesAsync();

            var repository = new ProductRepository(context, accessor);

            var result = await repository.IsCategoryExistsAsync(1);

            result.Should().BeFalse();
        }

        [Fact]
        public async Task IsCategoryExistsAsync_ShouldReturnFalse_WhenCategoryDoesNotExist()
        {
            using var context = CreateDbContext();
            var accessor = CreateMockHttpContextAccessor();
            var repository = new ProductRepository(context, accessor);

            var result = await repository.IsCategoryExistsAsync(999);

            result.Should().BeFalse();
        }

        [Fact]
        public async Task AddAsync_ShouldAddProductToDatabase()
        {
            using var context = CreateDbContext();
            var accessor = CreateMockHttpContextAccessor();
            var repository = new ProductRepository(context, accessor);

            var product = new Product
            {
                Name = "Keyboard",
                Sku = "SKU-KB",
                CategoryId = 1,
                SellingPrice = 80,
                CostPrice = 50,
            };

            await repository.AddAsync(product);
            await repository.SaveChangesAsync();

            var savedProduct = await context.Products.FirstOrDefaultAsync(p => p.Sku == "SKU-KB");

            savedProduct.Should().NotBeNull();
            savedProduct!.Name.Should().Be("Keyboard");
        }

        [Fact]
        public async Task GetByIdAsync_ShouldReturnProduct_WhenProductExists()
        {
            using var context = CreateDbContext();
            var accessor = CreateMockHttpContextAccessor();

            var product = new Product
            {
                Id = 10,
                Name = "Monitor",
                Sku = "SKU-MON",
                CategoryId = 1,
                SellingPrice = 200,
                CostPrice = 150,
                IsDeleted = false,
            };

            context.Products.Add(product);
            await context.SaveChangesAsync();

            var repository = new ProductRepository(context, accessor);

            var result = await repository.GetByIdAsync(10);

            result.Should().NotBeNull();
            result!.Name.Should().Be("Monitor");
        }

        [Fact]
        public async Task GetByIdAsync_ShouldReturnNull_WhenProductIsDeleted()
        {
            using var context = CreateDbContext();
            var accessor = CreateMockHttpContextAccessor();

            var product = new Product
            {
                Id = 10,
                Name = "Monitor",
                Sku = "SKU-MON",
                CategoryId = 1,
                SellingPrice = 200,
                CostPrice = 150,
                IsDeleted = true,
            };

            context.Products.Add(product);
            await context.SaveChangesAsync();

            var repository = new ProductRepository(context, accessor);

            var result = await repository.GetByIdAsync(10);

            result.Should().BeNull();
        }

        [Fact]
        public async Task GetByIdAsync_ShouldReturnNull_WhenProductDoesNotExist()
        {
            using var context = CreateDbContext();
            var accessor = CreateMockHttpContextAccessor();
            var repository = new ProductRepository(context, accessor);

            var result = await repository.GetByIdAsync(999);

            result.Should().BeNull();
        }

        [Fact]
        public async Task IsSkuExistsForOtherProductAsync_ShouldReturnTrue_WhenAnotherProductUsesSameSku()
        {
            using var context = CreateDbContext();
            var accessor = CreateMockHttpContextAccessor();

            var product = new Product
            {
                Id = 1,
                Name = "Headphones",
                Sku = "SKU-HP",
                CategoryId = 1,
                SellingPrice = 100,
                CostPrice = 60,
                IsDeleted = false,
            };

            context.Products.Add(product);
            await context.SaveChangesAsync();

            var repository = new ProductRepository(context, accessor);

            var result = await repository.IsSkuExistsForOtherProductAsync("SKU-HP", 2);

            result.Should().BeTrue();
        }

        [Fact]
        public async Task IsSkuExistsForOtherProductAsync_ShouldReturnFalse_WhenCheckedAgainstSameProductId()
        {
            using var context = CreateDbContext();
            var accessor = CreateMockHttpContextAccessor();

            var product = new Product
            {
                Id = 1,
                Name = "Headphones",
                Sku = "SKU-HP",
                CategoryId = 1,
                SellingPrice = 100,
                CostPrice = 60,
                IsDeleted = false,
            };

            context.Products.Add(product);
            await context.SaveChangesAsync();

            var repository = new ProductRepository(context, accessor);

            var result = await repository.IsSkuExistsForOtherProductAsync("SKU-HP", 1);

            result.Should().BeFalse();
        }

        [Fact]
        public async Task GetByIdForInventoryAsync_ShouldReturnProduct_WhenExistsAndNotDeleted()
        {
            using var context = CreateDbContext();
            var accessor = CreateMockHttpContextAccessor();

            var product = new Product
            {
                Id = 5,
                Name = "Printer",
                Sku = "SKU-PRN",
                CategoryId = 1,
                SellingPrice = 150,
                CostPrice = 100,
                QuantityOnHand = 20,
                IsDeleted = false,
            };

            context.Products.Add(product);
            await context.SaveChangesAsync();

            var repository = new ProductRepository(context, accessor);

            var result = await repository.GetByIdForInventoryAsync(5);

            result.Should().NotBeNull();
            result!.QuantityOnHand.Should().Be(20);
        }

        [Fact]
        public async Task AddInventoryTransactionAsync_ShouldAddTransactionToDatabase()
        {
            using var context = CreateDbContext();
            var accessor = CreateMockHttpContextAccessor();
            var repository = new ProductRepository(context, accessor);

            var transaction = new InventoryTransaction
            {
                ProductId = 1,
                ChangeType = (InventoryChangeType)1,
                QuantityBefore = 10,
                QuantityChange = 5,
                QuantityAfter = 15,
                Notes = "Stock received",
            };

            await repository.AddInventoryTransactionAsync(transaction);
            await repository.SaveChangesAsync();

            var saved = await context.InventoryTransactions.FirstOrDefaultAsync(t =>
                t.ProductId == 1
            );

            saved.Should().NotBeNull();
            saved!.QuantityAfter.Should().Be(15);
        }

        [Fact]
        public async Task GetProductsAsync_ShouldFilterBySearchText()
        {
            using var context = CreateDbContext();
            var accessor = CreateMockHttpContextAccessor();

            var category = new Category
            {
                Id = 1,
                Name = "Electronics",
                IsDeleted = false,
            };
            context.Categories.Add(category);

            context.Products.AddRange(
                new Product
                {
                    Id = 1,
                    Name = "Dell Laptop",
                    Sku = "SKU-D01",
                    CategoryId = 1,
                    Category = category,
                    SellingPrice = 800,
                    CostPrice = 600,
                    IsDeleted = false,
                },
                new Product
                {
                    Id = 2,
                    Name = "HP Mouse",
                    Sku = "SKU-M02",
                    CategoryId = 1,
                    Category = category,
                    SellingPrice = 20,
                    CostPrice = 10,
                    IsDeleted = false,
                }
            );
            await context.SaveChangesAsync();

            var repository = new ProductRepository(context, accessor);

            var request = new GetProductsRequestDto { Search = "Laptop", Page = 1 };

            var (data, totalRecords) = await repository.GetProductsAsync(request, 10);

            totalRecords.Should().Be(1);
            data.Should().HaveCount(1);
            data[0].Name.Should().Be("Dell Laptop");
        }

        [Fact]
        public async Task GetProductsAsync_ShouldFilterByPriceRange()
        {
            using var context = CreateDbContext();
            var accessor = CreateMockHttpContextAccessor();

            var category = new Category
            {
                Id = 1,
                Name = "General",
                IsDeleted = false,
            };
            context.Categories.Add(category);

            context.Products.AddRange(
                new Product
                {
                    Id = 1,
                    Name = "Item Low",
                    Sku = "SKU-L",
                    CategoryId = 1,
                    Category = category,
                    SellingPrice = 50,
                    CostPrice = 30,
                    IsDeleted = false,
                },
                new Product
                {
                    Id = 2,
                    Name = "Item Mid",
                    Sku = "SKU-M",
                    CategoryId = 1,
                    Category = category,
                    SellingPrice = 150,
                    CostPrice = 100,
                    IsDeleted = false,
                },
                new Product
                {
                    Id = 3,
                    Name = "Item High",
                    Sku = "SKU-H",
                    CategoryId = 1,
                    Category = category,
                    SellingPrice = 300,
                    CostPrice = 200,
                    IsDeleted = false,
                }
            );
            await context.SaveChangesAsync();

            var repository = new ProductRepository(context, accessor);

            var request = new GetProductsRequestDto
            {
                MinPrice = 100,
                MaxPrice = 200,
                Page = 1,
            };

            var (data, totalRecords) = await repository.GetProductsAsync(request, 10);

            totalRecords.Should().Be(1);
            data.Should().HaveCount(1);
            data[0].Name.Should().Be("Item Mid");
        }

        [Fact]
        public async Task GetProductsAsync_ShouldSortByPriceDescending()
        {
            using var context = CreateDbContext();
            var accessor = CreateMockHttpContextAccessor();

            var category = new Category
            {
                Id = 1,
                Name = "General",
                IsDeleted = false,
            };
            context.Categories.Add(category);

            context.Products.AddRange(
                new Product
                {
                    Id = 1,
                    Name = "Item A",
                    Sku = "SKU-A",
                    CategoryId = 1,
                    Category = category,
                    SellingPrice = 100,
                    CostPrice = 50,
                    IsDeleted = false,
                },
                new Product
                {
                    Id = 2,
                    Name = "Item B",
                    Sku = "SKU-B",
                    CategoryId = 1,
                    Category = category,
                    SellingPrice = 300,
                    CostPrice = 150,
                    IsDeleted = false,
                },
                new Product
                {
                    Id = 3,
                    Name = "Item C",
                    Sku = "SKU-C",
                    CategoryId = 1,
                    Category = category,
                    SellingPrice = 200,
                    CostPrice = 100,
                    IsDeleted = false,
                }
            );
            await context.SaveChangesAsync();

            var repository = new ProductRepository(context, accessor);

            var request = new GetProductsRequestDto
            {
                SortBy = "price",
                SortOrder = "desc",
                Page = 1,
            };

            var (data, totalRecords) = await repository.GetProductsAsync(request, 10);

            totalRecords.Should().Be(3);
            data.Should().HaveCount(3);
            data[0].SellingPrice.Should().Be(300);
            data[1].SellingPrice.Should().Be(200);
            data[2].SellingPrice.Should().Be(100);
        }

        [Fact]
        public async Task GetProductsAsync_ShouldPaginateProperly()
        {
            using var context = CreateDbContext();
            var accessor = CreateMockHttpContextAccessor();

            var category = new Category
            {
                Id = 1,
                Name = "General",
                IsDeleted = false,
            };
            context.Categories.Add(category);

            context.Products.AddRange(
                new Product
                {
                    Id = 1,
                    Name = "A",
                    Sku = "SKU-1",
                    CategoryId = 1,
                    Category = category,
                    SellingPrice = 10,
                    CostPrice = 5,
                    IsDeleted = false,
                },
                new Product
                {
                    Id = 2,
                    Name = "B",
                    Sku = "SKU-2",
                    CategoryId = 1,
                    Category = category,
                    SellingPrice = 10,
                    CostPrice = 5,
                    IsDeleted = false,
                },
                new Product
                {
                    Id = 3,
                    Name = "C",
                    Sku = "SKU-3",
                    CategoryId = 1,
                    Category = category,
                    SellingPrice = 10,
                    CostPrice = 5,
                    IsDeleted = false,
                }
            );
            await context.SaveChangesAsync();

            var repository = new ProductRepository(context, accessor);

            var request = new GetProductsRequestDto
            {
                SortBy = "name",
                SortOrder = "asc",
                Page = 2,
            };

            var (data, totalRecords) = await repository.GetProductsAsync(request, 2);

            totalRecords.Should().Be(3);
            data.Should().HaveCount(1);
            data[0].Name.Should().Be("C");
        }

        [Fact]
        public async Task GetProductsAsync_ShouldBuildCorrectFileUrlForImages()
        {
            using var context = CreateDbContext();
            var accessor = CreateMockHttpContextAccessor();

            var category = new Category
            {
                Id = 1,
                Name = "Furniture",
                IsDeleted = false,
            };
            context.Categories.Add(category);

            var product = new Product
            {
                Id = 1,
                Name = "Desk Lamp",
                Sku = "SKU-LAMP",
                CategoryId = 1,
                Category = category,
                SellingPrice = 40,
                CostPrice = 25,
                IsDeleted = false,
                ProductImages = new List<ProductImage>
                {
                    new ProductImage
                    {
                        Id = 1,
                        ProductId = 1,
                        FileUrl = "/uploads/lamp.jpg",
                        IsDeleted = false,
                    },
                    new ProductImage
                    {
                        Id = 2,
                        ProductId = 1,
                        FileUrl = "/uploads/lamp-deleted.jpg",
                        IsDeleted = true,
                    },
                },
            };

            context.Products.Add(product);
            await context.SaveChangesAsync();

            var repository = new ProductRepository(context, accessor);

            var request = new GetProductsRequestDto { Page = 1 };

            var (data, totalRecords) = await repository.GetProductsAsync(request, 10);

            totalRecords.Should().Be(1);
            data.Should().HaveCount(1);
            data[0].Images.Should().HaveCount(1);
            data[0].Images[0].FileUrl.Should().Be("https://localhost:5001/uploads/lamp.jpg");
        }

        [Fact]
        public async Task GetProductsAsync_ShouldFilterByCategoryId()
        {
            using var context = CreateDbContext();
            var accessor = CreateMockHttpContextAccessor();

            var cat1 = new Category
            {
                Id = 1,
                Name = "Cat1",
                IsDeleted = false,
            };
            var cat2 = new Category
            {
                Id = 2,
                Name = "Cat2",
                IsDeleted = false,
            };
            context.Categories.AddRange(cat1, cat2);

            context.Products.AddRange(
                new Product
                {
                    Id = 1,
                    Name = "Product 1",
                    Sku = "SKU-1",
                    CategoryId = 1,
                    Category = cat1,
                    SellingPrice = 50,
                    CostPrice = 30,
                    IsDeleted = false,
                },
                new Product
                {
                    Id = 2,
                    Name = "Product 2",
                    Sku = "SKU-2",
                    CategoryId = 2,
                    Category = cat2,
                    SellingPrice = 60,
                    CostPrice = 40,
                    IsDeleted = false,
                }
            );
            await context.SaveChangesAsync();

            var repository = new ProductRepository(context, accessor);

            var request = new GetProductsRequestDto { CategoryId = 1, Page = 1 };

            var (data, totalRecords) = await repository.GetProductsAsync(request, 10);

            totalRecords.Should().Be(1);
            data.Should().HaveCount(1);
            data[0].CategoryId.Should().Be(1);
        }

        [Fact]
        public async Task GetProductsAsync_ShouldFilterByIsActive()
        {
            using var context = CreateDbContext();
            var accessor = CreateMockHttpContextAccessor();

            var category = new Category
            {
                Id = 1,
                Name = "Cat",
                IsDeleted = false,
            };
            context.Categories.Add(category);

            context.Products.AddRange(
                new Product
                {
                    Id = 1,
                    Name = "Active Item",
                    Sku = "SKU-ACT",
                    CategoryId = 1,
                    Category = category,
                    SellingPrice = 50,
                    CostPrice = 30,
                    IsActive = true,
                    IsDeleted = false,
                },
                new Product
                {
                    Id = 2,
                    Name = "Inactive Item",
                    Sku = "SKU-INACT",
                    CategoryId = 1,
                    Category = category,
                    SellingPrice = 50,
                    CostPrice = 30,
                    IsActive = false,
                    IsDeleted = false,
                }
            );
            await context.SaveChangesAsync();

            var repository = new ProductRepository(context, accessor);

            var request = new GetProductsRequestDto { IsActive = false, Page = 1 };

            var (data, totalRecords) = await repository.GetProductsAsync(request, 10);

            totalRecords.Should().Be(1);
            data.Should().HaveCount(1);
            data[0].Name.Should().Be("Inactive Item");
            data[0].IsActive.Should().BeFalse();
        }

        [Fact]
        public async Task GetProductsAsync_ShouldSortByQuantityAscending()
        {
            using var context = CreateDbContext();
            var accessor = CreateMockHttpContextAccessor();

            var category = new Category
            {
                Id = 1,
                Name = "Cat",
                IsDeleted = false,
            };
            context.Categories.Add(category);

            context.Products.AddRange(
                new Product
                {
                    Id = 1,
                    Name = "Prod A",
                    Sku = "SKU-A",
                    CategoryId = 1,
                    Category = category,
                    SellingPrice = 50,
                    CostPrice = 30,
                    QuantityOnHand = 50,
                    IsDeleted = false,
                },
                new Product
                {
                    Id = 2,
                    Name = "Prod B",
                    Sku = "SKU-B",
                    CategoryId = 1,
                    Category = category,
                    SellingPrice = 50,
                    CostPrice = 30,
                    QuantityOnHand = 10,
                    IsDeleted = false,
                }
            );
            await context.SaveChangesAsync();

            var repository = new ProductRepository(context, accessor);

            var request = new GetProductsRequestDto
            {
                SortBy = "quantity",
                SortOrder = "asc",
                Page = 1,
            };

            var (data, totalRecords) = await repository.GetProductsAsync(request, 10);

            data.Should().HaveCount(2);
            data[0].QuantityOnHand.Should().Be(10);
            data[1].QuantityOnHand.Should().Be(50);
        }

        [Fact]
        public async Task GetProductsAsync_ShouldSortByQuantityDescending()
        {
            using var context = CreateDbContext();
            var accessor = CreateMockHttpContextAccessor();

            var category = new Category
            {
                Id = 1,
                Name = "Cat",
                IsDeleted = false,
            };
            context.Categories.Add(category);

            context.Products.AddRange(
                new Product
                {
                    Id = 1,
                    Name = "Prod A",
                    Sku = "SKU-A",
                    CategoryId = 1,
                    Category = category,
                    SellingPrice = 50,
                    CostPrice = 30,
                    QuantityOnHand = 10,
                    IsDeleted = false,
                },
                new Product
                {
                    Id = 2,
                    Name = "Prod B",
                    Sku = "SKU-B",
                    CategoryId = 1,
                    Category = category,
                    SellingPrice = 50,
                    CostPrice = 30,
                    QuantityOnHand = 50,
                    IsDeleted = false,
                }
            );
            await context.SaveChangesAsync();

            var repository = new ProductRepository(context, accessor);

            var request = new GetProductsRequestDto
            {
                SortBy = "quantity",
                SortOrder = "desc",
                Page = 1,
            };

            var (data, totalRecords) = await repository.GetProductsAsync(request, 10);

            data.Should().HaveCount(2);
            data[0].QuantityOnHand.Should().Be(50);
            data[1].QuantityOnHand.Should().Be(10);
        }

        [Fact]
        public async Task GetProductsAsync_ShouldSortBySkuAscending()
        {
            using var context = CreateDbContext();
            var accessor = CreateMockHttpContextAccessor();

            var category = new Category
            {
                Id = 1,
                Name = "Cat",
                IsDeleted = false,
            };
            context.Categories.Add(category);

            context.Products.AddRange(
                new Product
                {
                    Id = 1,
                    Name = "Prod 1",
                    Sku = "SKU-Z",
                    CategoryId = 1,
                    Category = category,
                    SellingPrice = 50,
                    CostPrice = 30,
                    IsDeleted = false,
                },
                new Product
                {
                    Id = 2,
                    Name = "Prod 2",
                    Sku = "SKU-A",
                    CategoryId = 1,
                    Category = category,
                    SellingPrice = 50,
                    CostPrice = 30,
                    IsDeleted = false,
                }
            );
            await context.SaveChangesAsync();

            var repository = new ProductRepository(context, accessor);

            var request = new GetProductsRequestDto
            {
                SortBy = "sku",
                SortOrder = "asc",
                Page = 1,
            };

            var (data, totalRecords) = await repository.GetProductsAsync(request, 10);

            data.Should().HaveCount(2);
            data[0].Sku.Should().Be("SKU-A");
            data[1].Sku.Should().Be("SKU-Z");
        }

        [Fact]
        public async Task GetProductsAsync_ShouldSortBySkuDescending()
        {
            using var context = CreateDbContext();
            var accessor = CreateMockHttpContextAccessor();

            var category = new Category
            {
                Id = 1,
                Name = "Cat",
                IsDeleted = false,
            };
            context.Categories.Add(category);

            context.Products.AddRange(
                new Product
                {
                    Id = 1,
                    Name = "Prod 1",
                    Sku = "SKU-A",
                    CategoryId = 1,
                    Category = category,
                    SellingPrice = 50,
                    CostPrice = 30,
                    IsDeleted = false,
                },
                new Product
                {
                    Id = 2,
                    Name = "Prod 2",
                    Sku = "SKU-Z",
                    CategoryId = 1,
                    Category = category,
                    SellingPrice = 50,
                    CostPrice = 30,
                    IsDeleted = false,
                }
            );
            await context.SaveChangesAsync();

            var repository = new ProductRepository(context, accessor);

            var request = new GetProductsRequestDto
            {
                SortBy = "sku",
                SortOrder = "desc",
                Page = 1,
            };

            var (data, totalRecords) = await repository.GetProductsAsync(request, 10);

            data.Should().HaveCount(2);
            data[0].Sku.Should().Be("SKU-Z");
            data[1].Sku.Should().Be("SKU-A");
        }

        [Fact]
        public async Task GetProductsAsync_ShouldSortByNameDescending()
        {
            using var context = CreateDbContext();
            var accessor = CreateMockHttpContextAccessor();

            var category = new Category
            {
                Id = 1,
                Name = "Cat",
                IsDeleted = false,
            };
            context.Categories.Add(category);

            context.Products.AddRange(
                new Product
                {
                    Id = 1,
                    Name = "Alpha",
                    Sku = "SKU-1",
                    CategoryId = 1,
                    Category = category,
                    SellingPrice = 50,
                    CostPrice = 30,
                    IsDeleted = false,
                },
                new Product
                {
                    Id = 2,
                    Name = "Omega",
                    Sku = "SKU-2",
                    CategoryId = 1,
                    Category = category,
                    SellingPrice = 50,
                    CostPrice = 30,
                    IsDeleted = false,
                }
            );
            await context.SaveChangesAsync();

            var repository = new ProductRepository(context, accessor);

            var request = new GetProductsRequestDto
            {
                SortBy = "name",
                SortOrder = "desc",
                Page = 1,
            };

            var (data, totalRecords) = await repository.GetProductsAsync(request, 10);

            data.Should().HaveCount(2);
            data[0].Name.Should().Be("Omega");
            data[1].Name.Should().Be("Alpha");
        }

        [Fact]
        public async Task GetProductsAsync_ShouldDefaultPageToOne_WhenPageIsZeroOrNegative()
        {
            using var context = CreateDbContext();
            var accessor = CreateMockHttpContextAccessor();

            var category = new Category
            {
                Id = 1,
                Name = "Cat",
                IsDeleted = false,
            };
            context.Categories.Add(category);

            context.Products.Add(
                new Product
                {
                    Id = 1,
                    Name = "Tablet",
                    Sku = "SKU-TAB",
                    CategoryId = 1,
                    Category = category,
                    SellingPrice = 120,
                    CostPrice = 80,
                    IsDeleted = false,
                }
            );
            await context.SaveChangesAsync();

            var repository = new ProductRepository(context, accessor);

            var request = new GetProductsRequestDto { Page = 0 };

            var (data, totalRecords) = await repository.GetProductsAsync(request, 10);

            totalRecords.Should().Be(1);
            data.Should().HaveCount(1);
            data[0].Name.Should().Be("Tablet");
        }

        [Fact]
        public async Task GetProductsAsync_ShouldFilterBySku_WhenSearchMatchesSku()
        {
            using var context = CreateDbContext();
            var accessor = CreateMockHttpContextAccessor();

            var category = new Category
            {
                Id = 1,
                Name = "Cat",
                IsDeleted = false,
            };
            context.Categories.Add(category);

            context.Products.AddRange(
                new Product
                {
                    Id = 1,
                    Name = "Cable",
                    Sku = "HDMI-WIRE",
                    CategoryId = 1,
                    Category = category,
                    SellingPrice = 10,
                    CostPrice = 5,
                    IsDeleted = false,
                },
                new Product
                {
                    Id = 2,
                    Name = "Charger",
                    Sku = "TYPE-C-PLUG",
                    CategoryId = 1,
                    Category = category,
                    SellingPrice = 20,
                    CostPrice = 10,
                    IsDeleted = false,
                }
            );
            await context.SaveChangesAsync();

            var repository = new ProductRepository(context, accessor);

            var request = new GetProductsRequestDto { Search = "HDMI", Page = 1 };

            var (data, totalRecords) = await repository.GetProductsAsync(request, 10);

            totalRecords.Should().Be(1);
            data.Should().HaveCount(1);
            data[0].Sku.Should().Be("HDMI-WIRE");
        }
    }
}
