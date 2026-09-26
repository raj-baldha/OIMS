using OIMS.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Text;

namespace OIMS.Application.Interfaces.Repositories
{
    public interface IApiExecutionLogRepository
    {
        Task AddAsync(ApiExecutionLog log);
        Task SaveChangesAsync();
    }
}
