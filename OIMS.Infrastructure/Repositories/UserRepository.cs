using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.EntityFrameworkCore;
using OIMS.Application.Interfaces.Repositories;
using OIMS.Data.DbContext;
using OIMS.Domain.Entities;

namespace OIMS.Infrastructure.Repositories
{
    public class UserRepository : IUserRepository
    {
        private readonly AppDbContext _context;

        /// <summary>Initializes the repository with the application database context.</summary>
        public UserRepository(AppDbContext context)
        {
            _context = context;
        }

        /// <summary>Determines whether a user with the specified email exists.</summary>
        public async Task<bool> IsEmailExistsAsync(string email)
        {
            return await _context.Users.AnyAsync(u => u.Email == email && !u.IsDeleted);
        }

        /// <summary>Finds a user by email address.</summary>
        public async Task<User?> GetByEmailAsync(string email)
        {
            return await _context.Users.FirstOrDefaultAsync(u => u.Email == email && !u.IsDeleted);
        }

        /// <summary>Adds a user to the database context.</summary>
        public async Task AddAsync(User user)
        {
            await _context.Users.AddAsync(user);
        }

        /// <summary>Persists pending database changes.</summary>
        public async Task SaveChangesAsync()
        {
            await _context.SaveChangesAsync();
        }

        /// <summary>Retrieves active users whose roles match the supplied list.</summary>
        public async Task<List<User>> GetActiveUsersByRolesAsync(List<string> roles)
        {
            return await _context
                .Users.AsNoTracking()
                .Where(x => !x.IsDeleted && x.IsActive && roles.Contains(x.Role))
                .ToListAsync();
        }
    }
}
