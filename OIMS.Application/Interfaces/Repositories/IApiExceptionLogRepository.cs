using OIMS.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Text;

namespace OIMS.Application.Interfaces.Repositories
{
    public interface IApiExceptionLogRepository
    {
        Task AddAsync(ApiExceptionLog log);
        Task SaveChangesAsync();
    }
}
