using System;
using System.Collections.Generic;
using System.Text;
using OIMS.Application.DTOs.Common;
using OIMS.Application.DTOs.Request;
using OIMS.Application.DTOs.Response;

namespace OIMS.Application.Interfaces.Services
{
    public interface ICustomerService
    {
        Task<CreatedCustomerResponseDto> CreateCustomerAsync(
            CreateCustomerRequestDto request,
            int userId
        );
        Task<PagedResponseDto<CustomerListResponseDto>> GetCustomersAsync(
            GetCustomersRequestDto request,
            string role,
            int userId
        );
        Task<CreatedCustomerResponseDto> UpdateCustomerAsync(
            int id,
            UpdateCustomerRequestDto request,
            int userId
        );
        Task DeactiveCustomersAsync(int id, int userId);
    }
}
