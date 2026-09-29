using System;
using System.Threading.Tasks;
using AutoMapper;
using FluentAssertions;
using Moq;
using OIMS.Application.DTOs.Request;
using OIMS.Application.DTOs.Response;
using OIMS.Application.Interfaces.Repositories;
using OIMS.Application.Services;
using OIMS.Domain.Entities;
using OIMS.Domain.Exceptions;
using Xunit;

namespace OIMS.Tests.Services
{
    public class CategoryServiceTests
    {
        private readonly Mock<ICategoryRepository> _categoryRepositoryMock;
        private readonly Mock<IMapper> _mapperMock;
        private readonly CategoryService _categoryService;

        public CategoryServiceTests()
        {
            _categoryRepositoryMock = new Mock<ICategoryRepository>();
            _mapperMock = new Mock<IMapper>();

            _categoryService = new CategoryService(
                _categoryRepositoryMock.Object,
                _mapperMock.Object
            );
        }

        [Fact]
        public async Task CreateCategoryAsync_ShouldThrowConflictException_WhenCategoryNameAlreadyExists()
        {
            var request = new CreateCategoryRequestDto { Name = "  Electronics  " };

            _categoryRepositoryMock
                .Setup(repo => repo.IsNameExistsAsync("Electronics"))
                .ReturnsAsync(true);

            var action = async () => await _categoryService.CreateCategoryAsync(request, userId: 1);

            await action
                .Should()
                .ThrowAsync<ConflictException>()
                .WithMessage("Category name already exists.");

            _categoryRepositoryMock.Verify(
                repo => repo.AddAsync(It.IsAny<Category>()),
                Times.Never
            );
            _categoryRepositoryMock.Verify(repo => repo.SaveChangesAsync(), Times.Never);
        }

        [Fact]
        public async Task CreateCategoryAsync_ShouldCreateCategoryAndReturnResponse_WhenNameIsUnique()
        {
            var request = new CreateCategoryRequestDto { Name = "  Hardware  " };

            int userId = 10;

            var expectedResponse = new CreatedCategoryResponseDto
            {
                Id = 1,
                Name = "Hardware",
                CreatedAt = DateTime.UtcNow,
                CreatedBy = userId,
            };

            _categoryRepositoryMock
                .Setup(repo => repo.IsNameExistsAsync("Hardware"))
                .ReturnsAsync(false);

            _categoryRepositoryMock
                .Setup(repo => repo.AddAsync(It.IsAny<Category>()))
                .Returns(Task.CompletedTask);

            _categoryRepositoryMock
                .Setup(repo => repo.SaveChangesAsync())
                .Returns(Task.CompletedTask);

            _mapperMock
                .Setup(mapper =>
                    mapper.Map<CreatedCategoryResponseDto>(
                        It.Is<Category>(c =>
                            c.Name == "Hardware" && c.CreatedBy == userId && !c.IsDeleted
                        )
                    )
                )
                .Returns(expectedResponse);

            var result = await _categoryService.CreateCategoryAsync(request, userId);

            result.Should().NotBeNull();
            result.Id.Should().Be(1);
            result.Name.Should().Be("Hardware");
            result.CreatedBy.Should().Be(userId);

            _categoryRepositoryMock.Verify(
                repo =>
                    repo.AddAsync(
                        It.Is<Category>(c =>
                            c.Name == "Hardware" && c.CreatedBy == userId && !c.IsDeleted
                        )
                    ),
                Times.Once
            );

            _categoryRepositoryMock.Verify(repo => repo.SaveChangesAsync(), Times.Once);
        }
    }
}
