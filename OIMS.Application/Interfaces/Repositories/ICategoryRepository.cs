using OIMS.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Text;

namespace OIMS.Application.Interfaces.Repositories
{
    public interface ICategoryRepository
    {
        Task<bool> IsNameExistsAsync(string name);
        Task AddAsync(Category category);
        Task SaveChangesAsync();
    }
}
