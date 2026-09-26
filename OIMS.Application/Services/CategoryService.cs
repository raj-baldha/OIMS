using AutoMapper;
using OIMS.Application.DTOs.Request;
using OIMS.Application.DTOs.Response;
using OIMS.Application.Interfaces.Repositories;
using OIMS.Application.Interfaces.Services;
using OIMS.Domain.Entities;
using OIMS.Domain.Exceptions;

namespace OIMS.Application.Services
{
    public class CategoryService : ICategoryService
    {
        private readonly ICategoryRepository _categoryRepository;
        private readonly IMapper _mapper;

        public CategoryService(ICategoryRepository categoryRepository, IMapper mapper)
        {
            _categoryRepository = categoryRepository;
            _mapper = mapper;
        }

        public async Task<CreatedCategoryResponseDto> CreateCategoryAsync(
            CreateCategoryRequestDto request,
            int userId
        )
        {
            string name = request.Name.Trim();

            bool exists = await _categoryRepository.IsNameExistsAsync(name);

            if (exists)
            {
                throw new ConflictException("Category name already exists.");
            }

            var category = new Category
            {
                Name = name,
                CreatedAt = DateTime.UtcNow,
                CreatedBy = userId,
                IsDeleted = false,
            };

            await _categoryRepository.AddAsync(category);
            await _categoryRepository.SaveChangesAsync();

            var response = _mapper.Map<CreatedCategoryResponseDto>(category);

            return response;
        }
    }
}
