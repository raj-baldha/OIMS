using AutoMapper;
using Microsoft.Extensions.Configuration;
using OIMS.Application.DTOs.Common;
using OIMS.Application.DTOs.Request;
using OIMS.Application.DTOs.Response;
using OIMS.Application.Interfaces.Helpers;
using OIMS.Application.Interfaces.Repositories;
using OIMS.Application.Interfaces.Services;
using OIMS.Domain.Entities;
using OIMS.Domain.Exceptions;

namespace OIMS.Application.Services
{
    public class CustomerService : ICustomerService
    {
        private readonly ICustomerRepository _customerRepository;
        private readonly IPasswordHelper _passwordHelper;
        private readonly IEmailService _emailService;
        private readonly IConfiguration _configuration;
        private readonly IMapper _mapper;

        public CustomerService(
            ICustomerRepository customerRepository,
            IPasswordHelper passwordHelper,
            IEmailService emailService,
            IConfiguration configuration,
            IMapper mapper)
        {
            _customerRepository = customerRepository;
            _passwordHelper = passwordHelper;
            _emailService = emailService;
            _configuration = configuration;
            _mapper = mapper;
        }

        public async Task<CreatedCustomerResponseDto>CreateCustomerAsync(CreateCustomerRequestDto request,int userId)
        {
            bool emailExists = await _customerRepository.IsEmailExistsAsync(request.Email);

            if (emailExists)
            {
                throw new ConflictException("Email already exists.");
            }

            string passwordHash = _passwordHelper.HashPassword(request.Password);

            var customer = new Customer
            {
                Email = request.Email,
                PasswordHash = passwordHash,

                FirstName = request.FirstName,
                LastName = request.LastName,
                Phone = request.Phone,
                Address = request.Address,
                City = request.City,
                State = request.State,
                Country = request.Country,

                IsActive = true,
                IsDeleted = false,
                CreatedAt = DateTime.UtcNow,
                CreatedBy = userId
            };

            await _customerRepository.AddAsync(customer);

            await _customerRepository.SaveChangesAsync();

            var templatePath = Path.Combine(
                AppContext.BaseDirectory,
                "EmailTemplate",
                "WelcomeEmail.html");

            var body =
                await File.ReadAllTextAsync(templatePath);

            body = body
               .Replace("{{Username}}", $"{customer.FirstName} {customer.LastName}")
               .Replace("{{Email}}", customer.Email)
               .Replace("{{Role}}", "Customer")
               .Replace("{{Password}}", request.Password);

            await _emailService.SendEmailAsync(customer.Email,"Welcome to OIMS",body);

            return new CreatedCustomerResponseDto
            {
                Id = customer.Id,
                FirstName = customer.FirstName,
                LastName = customer.LastName,
                Email = customer.Email,
                Phone = customer.Phone,
                Address = customer.Address,
                City = customer.City,
                State = customer.State,
                Country = customer.Country,
                IsActive = customer.IsActive,
                CreatedAt = customer.CreatedAt
            };
        }

        public async Task<PagedResponseDto<CustomerListResponseDto>>GetCustomersAsync(GetCustomersRequestDto request,string role,int userId)
        {
            int? customerId = null;

            if (role == "Customer")
            {
                customerId = userId;
            }

            int pageSize = int.Parse(_configuration["Pagination:PageSize"]!);

            if (request.Page < 1)
            {
                request.Page = 1;
            }

            if (string.IsNullOrWhiteSpace(request.SortBy))
            {
                request.SortBy = "name";
            }

            if (string.IsNullOrWhiteSpace(request.SortOrder))
            {
                request.SortOrder = "asc";
            }

            return await _customerRepository.GetCustomersAsync(
                request,
                pageSize,
                customerId);
        }

        public async Task<CreatedCustomerResponseDto>UpdateCustomerAsync(int id,UpdateCustomerRequestDto request,int userId)
        {
            var customer = await _customerRepository.GetByIdAsync(id);

            if (customer == null)
            {
                throw new NotFoundException("Customer not found.");
            }

            customer.FirstName = request.FirstName;
            customer.LastName = request.LastName;
            customer.Phone = request.Phone;
            customer.Address = request.Address;
            customer.City = request.City;
            customer.State = request.State;
            customer.Country = request.Country;

            customer.UpdatedAt = DateTime.UtcNow;
            customer.UpdatedBy = userId;

            await _customerRepository.SaveChangesAsync();

            return new CreatedCustomerResponseDto
            {
                Id = customer.Id,
                FirstName = customer.FirstName,
                LastName = customer.LastName,
                Email = customer.Email,
                Phone = customer.Phone,
                Address = customer.Address,
                City = customer.City,
                State = customer.State,
                Country = customer.Country,
                IsActive = customer.IsActive,
                CreatedAt = customer.CreatedAt,
            };
        }
    
        public async Task DeactiveCustomersAsync(int id, int userId)
        {
            var customer = await _customerRepository.GetByIdAsync(id);

            if (customer == null)
            {
                throw new NotFoundException("Customer not found.");
            }

            customer.IsActive = false;
            customer.UpdatedAt = DateTime.UtcNow;
            customer.UpdatedBy = userId;

            await _customerRepository.SaveChangesAsync();
        }
    }
}