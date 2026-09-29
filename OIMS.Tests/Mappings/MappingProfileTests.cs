using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using FluentAssertions;
using OIMS.Domain.Entities;
using Xunit;

namespace OIMS.Tests.Entities
{
    public class CategoryTests
    {
        [Fact]
        public void Category_ShouldInitializeWithDefaultValues()
        {
            var category = new Category();

            category.Id.Should().Be(0);
            category.Name.Should().BeEmpty();
            category.CreatedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(2));
            category.CreatedBy.Should().BeNull();
            category.UpdatedAt.Should().BeNull();
            category.UpdatedBy.Should().BeNull();
            category.IsDeleted.Should().BeFalse();
            category.DeletedAt.Should().BeNull();
            category.DeletedBy.Should().BeNull();
            category.Products.Should().NotBeNull();
            category.Products.Should().BeEmpty();
        }

        [Fact]
        public void Category_ShouldSetAndGetPropertiesCorrectly()
        {
            var now = DateTime.UtcNow;
            var products = new List<Product>
            {
                new Product { Id = 1, Name = "Laptop" },
            };

            var category = new Category
            {
                Id = 10,
                Name = "Electronics",
                CreatedAt = now,
                CreatedBy = 1,
                UpdatedAt = now.AddHours(1),
                UpdatedBy = 2,
                IsDeleted = true,
                DeletedAt = now.AddHours(2),
                DeletedBy = 3,
                Products = products,
            };

            category.Id.Should().Be(10);
            category.Name.Should().Be("Electronics");
            category.CreatedAt.Should().Be(now);
            category.CreatedBy.Should().Be(1);
            category.UpdatedAt.Should().Be(now.AddHours(1));
            category.UpdatedBy.Should().Be(2);
            category.IsDeleted.Should().BeTrue();
            category.DeletedAt.Should().Be(now.AddHours(2));
            category.DeletedBy.Should().Be(3);
            category.Products.Should().HaveCount(1);
            category.Products.Should().BeSameAs(products);
        }

        [Fact]
        public void Category_ShouldFailValidation_WhenNameIsEmpty()
        {
            var category = new Category { Name = string.Empty };

            var validationContext = new ValidationContext(category);
            var validationResults = new List<ValidationResult>();

            bool isValid = Validator.TryValidateObject(
                category,
                validationContext,
                validationResults,
                true
            );

            isValid.Should().BeFalse();
            validationResults.Should().Contain(v => v.MemberNames.Contains("Name"));
        }

        [Fact]
        public void Category_ShouldFailValidation_WhenNameExceeds150Characters()
        {
            var category = new Category { Name = new string('A', 151) };

            var validationContext = new ValidationContext(category);
            var validationResults = new List<ValidationResult>();

            bool isValid = Validator.TryValidateObject(
                category,
                validationContext,
                validationResults,
                true
            );

            isValid.Should().BeFalse();
            validationResults.Should().Contain(v => v.MemberNames.Contains("Name"));
        }

        [Fact]
        public void Category_ShouldPassValidation_WhenNameIsValid()
        {
            var category = new Category { Name = "Valid Category Name" };

            var validationContext = new ValidationContext(category);
            var validationResults = new List<ValidationResult>();

            bool isValid = Validator.TryValidateObject(
                category,
                validationContext,
                validationResults,
                true
            );

            isValid.Should().BeTrue();
            validationResults.Should().BeEmpty();
        }
    }
}
