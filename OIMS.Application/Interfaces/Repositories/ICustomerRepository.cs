using OIMS.Application.DTOs.Common;
using OIMS.Application.DTOs.Request;
using OIMS.Application.DTOs.Response;
using OIMS.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Text;

namespace OIMS.Application.Interfaces.Repositories
{
    public interface ICustomerRepository
    {
        Task<bool> IsEmailExistsAsync(string email);
        Task<Customer?> GetByEmailAsync(string email);
        Task AddAsync(Customer customer);
        Task SaveChangesAsync();
        Task<PagedResponseDto<CustomerListResponseDto>> GetCustomersAsync(GetCustomersRequestDto request,int pageSize,int? customerId);
        Task<Customer?> GetByIdAsync(int id);
    }
}
