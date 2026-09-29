using Microsoft.EntityFrameworkCore;
using OIMS.Application.Interfaces.Repositories;
using OIMS.Data.DbContext;
using OIMS.Domain.Entities;

namespace OIMS.Infrastructure.Repositories
{
    public class CategoryRepository : ICategoryRepository
    {
        private readonly AppDbContext _context;

        /// <summary>Initializes the repository with the application database context.</summary>
        public CategoryRepository(AppDbContext context)
        {
            _context = context;
        }

        /// <summary>Determines whether a category with the specified name exists.</summary>
        public async Task<bool> IsNameExistsAsync(string name)
        {
            return await _context.Categories.AnyAsync(x => x.Name == name && !x.IsDeleted);
        }

        /// <summary>Adds a category to the database context.</summary>
        public async Task AddAsync(Category category)
        {
            await _context.Categories.AddAsync(category);
        }

        /// <summary>Persists pending database changes.</summary>
        public async Task SaveChangesAsync()
        {
            await _context.SaveChangesAsync();
        }
    }
}
