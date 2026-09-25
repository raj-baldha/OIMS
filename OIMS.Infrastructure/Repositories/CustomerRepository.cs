using Microsoft.EntityFrameworkCore;
using OIMS.Application.DTOs.Common;
using OIMS.Application.DTOs.Request;
using OIMS.Application.DTOs.Response;
using OIMS.Application.Interfaces.Repositories;
using OIMS.Data.DbContext;
using OIMS.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Text;

namespace OIMS.Infrastructure.Repositories
{
    public class CustomerRepository : ICustomerRepository
    {
        private readonly AppDbContext _context;

        public CustomerRepository(AppDbContext context)
        {
            _context = context;
        }

        public async Task<bool> IsEmailExistsAsync(string email)
        {
            return await _context.Customers
                .AnyAsync(x =>
                    x.Email == email &&
                    !x.IsDeleted);
        }

        public async Task<Customer?> GetByEmailAsync(string email)
        {
            return await _context.Customers
                .FirstOrDefaultAsync(x =>
                    x.Email == email &&
                    !x.IsDeleted);
        }

        public async Task AddAsync(Customer customer)
        {
            await _context.Customers.AddAsync(customer);
        }

        public async Task SaveChangesAsync()
        {
            await _context.SaveChangesAsync();
        }

        public async Task<PagedResponseDto<CustomerListResponseDto>>GetCustomersAsync(GetCustomersRequestDto request,int pageSize,int? customerId)
        {
            var query = _context.Customers.AsNoTracking().Where(x => !x.IsDeleted);

            if (customerId.HasValue)
            {
                query = query.Where(x => x.Id == customerId.Value);
            }

            if (!string.IsNullOrWhiteSpace(request.Search))
            {
                string search = request.Search.Trim();

                query = query.Where(x =>
                    x.FirstName.Contains(search) ||
                    x.LastName.Contains(search) ||
                    x.Email.Contains(search));
            }

            if (request.SortBy == "email")
            {
                if (request.SortOrder == "desc")
                {
                    query = query.OrderByDescending(x => x.Email);
                }
                else
                {
                    query = query.OrderBy(x => x.Email);
                }
            }
            else
            {
                if (request.SortOrder == "desc")
                {
                    query = query
                        .OrderByDescending(x => x.FirstName)
                        .ThenByDescending(x => x.LastName);
                }
                else
                {
                    query = query
                        .OrderBy(x => x.FirstName)
                        .ThenBy(x => x.LastName);
                }
            }

            int totalRecords = await query.CountAsync();

            int page = request.Page;

            if (page < 1)
            {
                page = 1;
            }

            int totalPages = (int)Math.Ceiling((double)totalRecords / pageSize);

            int skip = (page - 1) * pageSize;

            var customers = await query
                .Skip(skip)
                .Take(pageSize)
                .Select(x => new CustomerListResponseDto
                {
                    Id = x.Id,
                    FirstName = x.FirstName,
                    LastName = x.LastName,
                    Email = x.Email,
                    Phone = x.Phone,
                    City = x.City,
                    IsActive = x.IsActive
                })
                .ToListAsync();

            return new PagedResponseDto<CustomerListResponseDto>
            {
                Data = customers,
                Page = page,
                PageSize = pageSize,
                TotalRecords = totalRecords,
                TotalPages = totalPages
            };
        }
        public async Task<Customer?> GetByIdAsync(int id)
        {
            return await _context.Customers
                .FirstOrDefaultAsync(x =>
                    x.Id == id &&
                    !x.IsDeleted);
        }
    }
}
