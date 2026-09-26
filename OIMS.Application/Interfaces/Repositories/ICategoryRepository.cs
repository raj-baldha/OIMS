using System;
using System.Collections.Generic;
using System.Text;
using OIMS.Domain.Entities;

namespace OIMS.Application.Interfaces.Repositories
{
    public interface ICategoryRepository
    {
        Task<bool> IsNameExistsAsync(string name);
        Task AddAsync(Category category);
        Task SaveChangesAsync();
    }
}
