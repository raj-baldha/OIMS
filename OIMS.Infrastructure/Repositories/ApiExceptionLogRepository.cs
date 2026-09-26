using OIMS.Application.Interfaces.Repositories;
using OIMS.Data.DbContext;
using OIMS.Domain.Entities;

namespace OIMS.Infrastructure.Repositories
{
    public class ApiExceptionLogRepository
        : IApiExceptionLogRepository
    {
        private readonly AppDbContext _context;

        public ApiExceptionLogRepository(AppDbContext context)
        {
            _context = context;
        }

        public async Task AddAsync(ApiExceptionLog log)
        {
            await _context.ApiExceptionLogs.AddAsync(log);
        }

        public async Task SaveChangesAsync()
        {
            await _context.SaveChangesAsync();
        }
    }
}