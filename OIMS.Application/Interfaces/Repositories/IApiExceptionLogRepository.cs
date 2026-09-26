using System;
using System.Collections.Generic;
using System.Text;
using OIMS.Domain.Entities;

namespace OIMS.Application.Interfaces.Repositories
{
    public interface IApiExceptionLogRepository
    {
        Task AddAsync(ApiExceptionLog log);
        Task SaveChangesAsync();
    }
}
