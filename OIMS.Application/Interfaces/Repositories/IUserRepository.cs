using System;
using System.Collections.Generic;
using System.Text;
using OIMS.Domain.Entities;

namespace OIMS.Application.Interfaces.Repositories
{
    public interface IUserRepository
    {
        Task<bool> IsEmailExistsAsync(string email);
        Task<User?> GetByEmailAsync(string email);
        Task AddAsync(User user);
        Task SaveChangesAsync();
        Task<List<User>> GetActiveUsersByRolesAsync(List<string> roles);
    }
}
