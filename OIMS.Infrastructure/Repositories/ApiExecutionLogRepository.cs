using System;
using System.Collections.Generic;
using System.Text;
using OIMS.Application.Interfaces.Repositories;
using OIMS.Data.DbContext;
using OIMS.Domain.Entities;

namespace OIMS.Infrastructure.Repositories
{
    public class ApiExecutionLogRepository : IApiExecutionLogRepository
    {
        private readonly AppDbContext _context;

        /// <summary>Initializes the repository with the application database context.</summary>
        public ApiExecutionLogRepository(AppDbContext context)
        {
            _context = context;
        }

        /// <summary>Adds an API execution log record to the database context.</summary>
        public async Task AddAsync(ApiExecutionLog log)
        {
            await _context.ApiExecutionLogs.AddAsync(log);
        }

        /// <summary>Persists pending database changes.</summary>
        public async Task SaveChangesAsync()
        {
            await _context.SaveChangesAsync();
        }
    }
}
