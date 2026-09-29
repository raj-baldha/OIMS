using FluentAssertions;
using FluentValidation.TestHelper;
using OIMS.Application.DTOs.Request;
using OIMS.Application.Validators;
using Xunit;

namespace OIMS.Tests.Validators
{
    public class CreateProductRequestValidatorTests
    {
        private readonly CreateProductRequestValidator _validator;

        public CreateProductRequestValidatorTests()
        {
            _validator = new CreateProductRequestValidator();
        }

        private CreateProductRequestDto CreateValidDto()
        {
            return new CreateProductRequestDto
            {
                Name = "Mechanical Keyboard",
                Sku = "SKU-KEY-100",
                CategoryId = 1,
                SellingPrice = 99.99m,
                CostPrice = 50.00m,
                QuantityOnHand = 20,
                MinStockLevel = 5,
            };
        }

        [Fact]
        public void Validate_ShouldPass_WhenAllFieldsAreValid()
        {
            var dto = CreateValidDto();

            var result = _validator.TestValidate(dto);

            result.ShouldNotHaveAnyValidationErrors();
        }

        [Theory]
        [InlineData(null)]
        [InlineData("")]
        [InlineData("   ")]
        public void Validate_ShouldHaveError_WhenNameIsEmpty(string? invalidName)
        {
            var dto = CreateValidDto();
            dto.Name = invalidName!;

            var result = _validator.TestValidate(dto);

            result
                .ShouldHaveValidationErrorFor(x => x.Name)
                .WithErrorMessage("Product name is required.");
        }

        [Fact]
        public void Validate_ShouldHaveError_WhenNameExceeds200Characters()
        {
            var dto = CreateValidDto();
            dto.Name = new string('A', 201);

            var result = _validator.TestValidate(dto);

            result
                .ShouldHaveValidationErrorFor(x => x.Name)
                .WithErrorMessage("Product name cannot exceed 200 characters.");
        }

        [Theory]
        [InlineData(null)]
        [InlineData("")]
        [InlineData("   ")]
        public void Validate_ShouldHaveError_WhenSkuIsEmpty(string? invalidSku)
        {
            var dto = CreateValidDto();
            dto.Sku = invalidSku!;

            var result = _validator.TestValidate(dto);

            result.ShouldHaveValidationErrorFor(x => x.Sku).WithErrorMessage("SKU is required.");
        }

        [Fact]
        public void Validate_ShouldHaveError_WhenSkuExceeds50Characters()
        {
            var dto = CreateValidDto();
            dto.Sku = new string('S', 51);

            var result = _validator.TestValidate(dto);

            result
                .ShouldHaveValidationErrorFor(x => x.Sku)
                .WithErrorMessage("SKU cannot exceed 50 characters.");
        }

        [Theory]
        [InlineData(0)]
        [InlineData(-1)]
        [InlineData(-100)]
        public void Validate_ShouldHaveError_WhenCategoryIdIsZeroOrNegative(int invalidCategoryId)
        {
            var dto = CreateValidDto();
            dto.CategoryId = invalidCategoryId;

            var result = _validator.TestValidate(dto);

            result
                .ShouldHaveValidationErrorFor(x => x.CategoryId)
                .WithErrorMessage("CategoryId must be greater than 0.");
        }

        [Theory]
        [InlineData(-0.01)]
        [InlineData(-10)]
        public void Validate_ShouldHaveError_WhenSellingPriceIsNegative(decimal invalidPrice)
        {
            var dto = CreateValidDto();
            dto.SellingPrice = invalidPrice;

            var result = _validator.TestValidate(dto);

            result
                .ShouldHaveValidationErrorFor(x => x.SellingPrice)
                .WithErrorMessage("Selling price cannot be negative.");
        }

        [Fact]
        public void Validate_ShouldPass_WhenSellingPriceIsZero()
        {
            var dto = CreateValidDto();
            dto.SellingPrice = 0;

            var result = _validator.TestValidate(dto);

            result.ShouldNotHaveValidationErrorFor(x => x.SellingPrice);
        }

        [Theory]
        [InlineData(-0.01)]
        [InlineData(-15)]
        public void Validate_ShouldHaveError_WhenCostPriceIsNegative(decimal invalidPrice)
        {
            var dto = CreateValidDto();
            dto.CostPrice = invalidPrice;

            var result = _validator.TestValidate(dto);

            result
                .ShouldHaveValidationErrorFor(x => x.CostPrice)
                .WithErrorMessage("Cost price cannot be negative.");
        }

        [Fact]
        public void Validate_ShouldPass_WhenCostPriceIsZero()
        {
            var dto = CreateValidDto();
            dto.CostPrice = 0;

            var result = _validator.TestValidate(dto);

            result.ShouldNotHaveValidationErrorFor(x => x.CostPrice);
        }

        [Theory]
        [InlineData(-1)]
        [InlineData(-50)]
        public void Validate_ShouldHaveError_WhenQuantityOnHandIsNegative(int invalidQuantity)
        {
            var dto = CreateValidDto();
            dto.QuantityOnHand = invalidQuantity;

            var result = _validator.TestValidate(dto);

            result
                .ShouldHaveValidationErrorFor(x => x.QuantityOnHand)
                .WithErrorMessage("Quantity on hand cannot be negative.");
        }

        [Fact]
        public void Validate_ShouldPass_WhenQuantityOnHandIsZero()
        {
            var dto = CreateValidDto();
            dto.QuantityOnHand = 0;

            var result = _validator.TestValidate(dto);

            result.ShouldNotHaveValidationErrorFor(x => x.QuantityOnHand);
        }

        [Theory]
        [InlineData(-1)]
        [InlineData(-10)]
        public void Validate_ShouldHaveError_WhenMinStockLevelIsNegative(int invalidStockLevel)
        {
            var dto = CreateValidDto();
            dto.MinStockLevel = invalidStockLevel;

            var result = _validator.TestValidate(dto);

            result
                .ShouldHaveValidationErrorFor(x => x.MinStockLevel)
                .WithErrorMessage("Minimum stock level cannot be negative.");
        }

        [Fact]
        public void Validate_ShouldPass_WhenMinStockLevelIsZero()
        {
            var dto = CreateValidDto();
            dto.MinStockLevel = 0;

            var result = _validator.TestValidate(dto);

            result.ShouldNotHaveValidationErrorFor(x => x.MinStockLevel);
        }
    }
}
