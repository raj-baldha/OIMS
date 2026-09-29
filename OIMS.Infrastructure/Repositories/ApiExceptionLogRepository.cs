using OIMS.Application.Interfaces.Repositories;
using OIMS.Data.DbContext;
using OIMS.Domain.Entities;

namespace OIMS.Infrastructure.Repositories
{
    public class ApiExceptionLogRepository : IApiExceptionLogRepository
    {
        private readonly AppDbContext _context;

        /// <summary>Initializes the repository with the application database context.</summary>
        public ApiExceptionLogRepository(AppDbContext context)
        {
            _context = context;
        }

        /// <summary>Adds an API exception log record to the database context.</summary>
        public async Task AddAsync(ApiExceptionLog log)
        {
            await _context.ApiExceptionLogs.AddAsync(log);
        }

        /// <summary>Persists pending database changes.</summary>
        public async Task SaveChangesAsync()
        {
            await _context.SaveChangesAsync();
        }
    }
}
