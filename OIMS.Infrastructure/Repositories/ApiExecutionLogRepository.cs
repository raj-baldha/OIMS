using OIMS.Application.Interfaces.Repositories;
using OIMS.Data.DbContext;
using OIMS.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Text;

namespace OIMS.Infrastructure.Repositories
{
    public class ApiExecutionLogRepository : IApiExecutionLogRepository
    {
        private readonly AppDbContext _context;

        public ApiExecutionLogRepository(AppDbContext context)
        {
            _context = context;
        }

        public async Task AddAsync(ApiExecutionLog log)
        {
            await _context.ApiExecutionLogs.AddAsync(log);
        }

        public async Task SaveChangesAsync()
        {
            await _context.SaveChangesAsync();
        }
    }
}
