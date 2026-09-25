using OIMS.Application.DTOs.Request;
using OIMS.Application.DTOs.Response;
using System;
using System.Collections.Generic;
using System.Text;

namespace OIMS.Application.Interfaces.Services
{
    public interface ICustomerAuthService
    {
        Task<(CustomerLoginResponseDto Customer, string Token)> LoginAsync(CustomerLoginRequestDto request);
    }
}
