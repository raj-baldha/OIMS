using OIMS.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Text;

namespace OIMS.Application.Interfaces.Repositories
{
    public interface IUserRepository
    {
        Task<bool> IsEmailExistsAsync(string email);
        Task<User?> GetByEmailAsync(string email);
        Task AddAsync(User user);
        Task SaveChangesAsync();
    }
}
