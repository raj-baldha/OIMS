using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using OIMS.Application.DTOs.Request;
using OIMS.Data.DbContext;
using OIMS.Domain.Entities;
using OIMS.Infrastructure.Repositories;
using Xunit;

namespace OIMS.Tests.Repository
{
    public class CustomerRepositoryTests
    {
        private AppDbContext CreateDbContext()
        {
            var options = new DbContextOptionsBuilder<AppDbContext>()
                .UseInMemoryDatabase(Guid.NewGuid().ToString())
                .Options;

            return new AppDbContext(options);
        }

        private Customer CreateCustomer(
            int id,
            string email,
            string firstName = "John",
            string lastName = "Doe",
            bool isDeleted = false
        )
        {
            return new Customer
            {
                Id = id,
                Email = email,
                FirstName = firstName,
                LastName = lastName,
                PasswordHash = "hashedpassword",
                Phone = "1234567890",
                City = "New York",
                IsActive = true,
                IsDeleted = isDeleted,
            };
        }

        [Fact]
        public async Task IsEmailExistsAsync_ShouldReturnTrue_WhenEmailExistsAndNotDeleted()
        {
            using var context = CreateDbContext();
            var customer = CreateCustomer(1, "test@example.com", isDeleted: false);
            context.Customers.Add(customer);
            await context.SaveChangesAsync();

            var repository = new CustomerRepository(context);

            var result = await repository.IsEmailExistsAsync("test@example.com");

            result.Should().BeTrue();
        }

        [Fact]
        public async Task IsEmailExistsAsync_ShouldReturnFalse_WhenCustomerIsDeleted()
        {
            using var context = CreateDbContext();
            var customer = CreateCustomer(1, "deleted@example.com", isDeleted: true);
            context.Customers.Add(customer);
            await context.SaveChangesAsync();

            var repository = new CustomerRepository(context);

            var result = await repository.IsEmailExistsAsync("deleted@example.com");

            result.Should().BeFalse();
        }

        [Fact]
        public async Task IsEmailExistsAsync_ShouldReturnFalse_WhenEmailDoesNotExist()
        {
            using var context = CreateDbContext();
            var repository = new CustomerRepository(context);

            var result = await repository.IsEmailExistsAsync("missing@example.com");

            result.Should().BeFalse();
        }

        [Fact]
        public async Task GetByEmailAsync_ShouldReturnCustomer_WhenEmailExistsAndNotDeleted()
        {
            using var context = CreateDbContext();
            var customer = CreateCustomer(1, "findme@example.com", firstName: "Alice");
            context.Customers.Add(customer);
            await context.SaveChangesAsync();

            var repository = new CustomerRepository(context);

            var result = await repository.GetByEmailAsync("findme@example.com");

            result.Should().NotBeNull();
            result!.FirstName.Should().Be("Alice");
            result.Email.Should().Be("findme@example.com");
        }

        [Fact]
        public async Task GetByEmailAsync_ShouldReturnNull_WhenCustomerIsDeleted()
        {
            using var context = CreateDbContext();
            var customer = CreateCustomer(1, "deleted@example.com", isDeleted: true);
            context.Customers.Add(customer);
            await context.SaveChangesAsync();

            var repository = new CustomerRepository(context);

            var result = await repository.GetByEmailAsync("deleted@example.com");

            result.Should().BeNull();
        }

        [Fact]
        public async Task GetByEmailAsync_ShouldReturnNull_WhenCustomerDoesNotExist()
        {
            using var context = CreateDbContext();
            var repository = new CustomerRepository(context);

            var result = await repository.GetByEmailAsync("notfound@example.com");

            result.Should().BeNull();
        }

        [Fact]
        public async Task GetByIdAsync_ShouldReturnCustomer_WhenCustomerExistsAndNotDeleted()
        {
            using var context = CreateDbContext();
            var customer = CreateCustomer(10, "ten@example.com", firstName: "David");
            context.Customers.Add(customer);
            await context.SaveChangesAsync();

            var repository = new CustomerRepository(context);

            var result = await repository.GetByIdAsync(10);

            result.Should().NotBeNull();
            result!.Id.Should().Be(10);
            result.FirstName.Should().Be("David");
        }

        [Fact]
        public async Task GetByIdAsync_ShouldReturnNull_WhenCustomerIsDeleted()
        {
            using var context = CreateDbContext();
            var customer = CreateCustomer(10, "deleted@example.com", isDeleted: true);
            context.Customers.Add(customer);
            await context.SaveChangesAsync();

            var repository = new CustomerRepository(context);

            var result = await repository.GetByIdAsync(10);

            result.Should().BeNull();
        }

        [Fact]
        public async Task GetByIdAsync_ShouldReturnNull_WhenCustomerDoesNotExist()
        {
            using var context = CreateDbContext();
            var repository = new CustomerRepository(context);

            var result = await repository.GetByIdAsync(999);

            result.Should().BeNull();
        }

        [Fact]
        public async Task AddAsync_ShouldAddCustomerToDatabase()
        {
            using var context = CreateDbContext();
            var repository = new CustomerRepository(context);

            var customer = CreateCustomer(1, "new@example.com", "Sarah", "Connor");

            await repository.AddAsync(customer);
            await repository.SaveChangesAsync();

            var savedCustomer = await context.Customers.FirstOrDefaultAsync(x =>
                x.Email == "new@example.com"
            );

            savedCustomer.Should().NotBeNull();
            savedCustomer!.FirstName.Should().Be("Sarah");
            savedCustomer.LastName.Should().Be("Connor");
        }

        [Fact]
        public async Task GetCustomersAsync_ShouldFilterByCustomerId()
        {
            using var context = CreateDbContext();

            var c1 = CreateCustomer(1, "c1@example.com");
            var c2 = CreateCustomer(2, "c2@example.com");
            context.Customers.AddRange(c1, c2);
            await context.SaveChangesAsync();

            var repository = new CustomerRepository(context);

            var request = new GetCustomersRequestDto { Page = 1 };

            var result = await repository.GetCustomersAsync(request, 10, customerId: 1);

            result.TotalRecords.Should().Be(1);
            result.Data.Should().HaveCount(1);
            result.Data[0].Id.Should().Be(1);
        }

        [Fact]
        public async Task GetCustomersAsync_ShouldFilterBySearchMatchingFirstNameLastNameOrEmail()
        {
            using var context = CreateDbContext();

            var c1 = CreateCustomer(1, "alex@example.com", firstName: "Alex", lastName: "Smith");
            var c2 = CreateCustomer(2, "bob@example.com", firstName: "Robert", lastName: "Taylor");
            var c3 = CreateCustomer(3, "clark@testmail.com", firstName: "Clark", lastName: "Kent");
            context.Customers.AddRange(c1, c2, c3);
            await context.SaveChangesAsync();

            var repository = new CustomerRepository(context);

            var requestSearchFirstName = new GetCustomersRequestDto { Search = "Alex", Page = 1 };
            var result1 = await repository.GetCustomersAsync(requestSearchFirstName, 10, null);
            result1.Data.Should().HaveCount(1);
            result1.Data[0].FirstName.Should().Be("Alex");

            var requestSearchLastName = new GetCustomersRequestDto { Search = "Taylor", Page = 1 };
            var result2 = await repository.GetCustomersAsync(requestSearchLastName, 10, null);
            result2.Data.Should().HaveCount(1);
            result2.Data[0].LastName.Should().Be("Taylor");

            var requestSearchEmail = new GetCustomersRequestDto { Search = "testmail", Page = 1 };
            var result3 = await repository.GetCustomersAsync(requestSearchEmail, 10, null);
            result3.Data.Should().HaveCount(1);
            result3.Data[0].Email.Should().Be("clark@testmail.com");
        }

        [Fact]
        public async Task GetCustomersAsync_ShouldSortByEmailAscendingAndDescending()
        {
            using var context = CreateDbContext();

            var c1 = CreateCustomer(1, "b@example.com");
            var c2 = CreateCustomer(2, "a@example.com");
            context.Customers.AddRange(c1, c2);
            await context.SaveChangesAsync();

            var repository = new CustomerRepository(context);

            var reqAsc = new GetCustomersRequestDto
            {
                SortBy = "email",
                SortOrder = "asc",
                Page = 1,
            };
            var resultAsc = await repository.GetCustomersAsync(reqAsc, 10, null);
            resultAsc.Data[0].Email.Should().Be("a@example.com");

            var reqDesc = new GetCustomersRequestDto
            {
                SortBy = "email",
                SortOrder = "desc",
                Page = 1,
            };
            var resultDesc = await repository.GetCustomersAsync(reqDesc, 10, null);
            resultDesc.Data[0].Email.Should().Be("b@example.com");
        }

        [Fact]
        public async Task GetCustomersAsync_ShouldSortByNameAscendingAndDescending()
        {
            using var context = CreateDbContext();

            var c1 = CreateCustomer(1, "c1@example.com", firstName: "Zack", lastName: "Brown");
            var c2 = CreateCustomer(2, "c2@example.com", firstName: "Adam", lastName: "Smith");
            context.Customers.AddRange(c1, c2);
            await context.SaveChangesAsync();

            var repository = new CustomerRepository(context);

            var reqAsc = new GetCustomersRequestDto
            {
                SortBy = "name",
                SortOrder = "asc",
                Page = 1,
            };
            var resultAsc = await repository.GetCustomersAsync(reqAsc, 10, null);
            resultAsc.Data[0].FirstName.Should().Be("Adam");

            var reqDesc = new GetCustomersRequestDto
            {
                SortBy = "name",
                SortOrder = "desc",
                Page = 1,
            };
            var resultDesc = await repository.GetCustomersAsync(reqDesc, 10, null);
            resultDesc.Data[0].FirstName.Should().Be("Zack");
        }

        [Fact]
        public async Task GetCustomersAsync_ShouldPaginateProperlyAndCalculateTotalPages()
        {
            using var context = CreateDbContext();

            var c1 = CreateCustomer(1, "c1@example.com", firstName: "A");
            var c2 = CreateCustomer(2, "c2@example.com", firstName: "B");
            var c3 = CreateCustomer(3, "c3@example.com", firstName: "C");
            context.Customers.AddRange(c1, c2, c3);
            await context.SaveChangesAsync();

            var repository = new CustomerRepository(context);

            var request = new GetCustomersRequestDto { SortOrder = "asc", Page = 2 };

            var result = await repository.GetCustomersAsync(request, pageSize: 2, null);

            result.TotalRecords.Should().Be(3);
            result.TotalPages.Should().Be(2);
            result.Page.Should().Be(2);
            result.Data.Should().HaveCount(1);
            result.Data[0].FirstName.Should().Be("C");
        }

        [Fact]
        public async Task GetCustomersAsync_ShouldDefaultPageToOne_WhenPageIsLessThanOne()
        {
            using var context = CreateDbContext();

            var customer = CreateCustomer(1, "single@example.com", firstName: "Test");
            context.Customers.Add(customer);
            await context.SaveChangesAsync();

            var repository = new CustomerRepository(context);

            var request = new GetCustomersRequestDto { Page = -5 };

            var result = await repository.GetCustomersAsync(request, 10, null);

            result.Page.Should().Be(1);
            result.TotalRecords.Should().Be(1);
            result.Data.Should().HaveCount(1);
        }
    }
}
