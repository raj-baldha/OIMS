using AutoMapper;
using OIMS.Application.DTOs.Request;
using OIMS.Application.DTOs.Response;
using OIMS.Application.Interfaces.Helpers;
using OIMS.Application.Interfaces.Repositories;
using OIMS.Application.Interfaces.Services;
using OIMS.Domain.Exceptions;

namespace OIMS.Application.Services
{
    public class CustomerAuthService : ICustomerAuthService
    {
        private readonly ICustomerRepository _customerRepository;
        private readonly IPasswordHelper _passwordHelper;
        private readonly IJwtHelper _jwtHelper;
        private readonly IMapper _mapper;

        public CustomerAuthService(
            ICustomerRepository customerRepository,
            IPasswordHelper passwordHelper,
            IJwtHelper jwtHelper,
            IMapper mapper)
        {
            _customerRepository = customerRepository;
            _passwordHelper = passwordHelper;
            _jwtHelper = jwtHelper;
            _mapper = mapper;
        }

        public async Task<(CustomerLoginResponseDto Customer, string Token)> LoginAsync(CustomerLoginRequestDto request)
        {
            var customer = await _customerRepository.GetByEmailAsync(request.Email);

            if (customer == null)
            {
                throw new UnauthorizedException("Invalid email or password.");
            }

            if (!customer.IsActive)
            {
                throw new UnauthorizedException("Your account is inactive.");
            }

            bool isPasswordValid = _passwordHelper.VerifyPassword(request.Password,customer.PasswordHash);

            if (!isPasswordValid)
            {
                throw new UnauthorizedException("Invalid email or password.");
            }

            string token =_jwtHelper.GenerateToken(customer.Id,customer.Email,"Customer");

            var response = _mapper.Map<CustomerLoginResponseDto>(customer);

            return (response, token);
        }
    }
}