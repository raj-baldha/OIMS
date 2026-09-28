using System.Text;
using FluentValidation;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using OIMS.API.Filters;
using OIMS.API.Middlewares;
using OIMS.Application.DTOs.Request;
using OIMS.Application.Interfaces.Helpers;
using OIMS.Application.Interfaces.Repositories;
using OIMS.Application.Interfaces.Services;
using OIMS.Application.Mappings;
using OIMS.Application.Services;
using OIMS.Application.Validators;
using OIMS.Data.DbContext;
using OIMS.Infrastructure.ExternalServices;
using OIMS.Infrastructure.Helpers;
using OIMS.Infrastructure.Repositories;
using QuestPDF.Infrastructure;

var builder = WebApplication.CreateBuilder(args);

QuestPDF.Settings.License = LicenseType.Community;

builder.Services.AddControllers(options =>
{
    options.Filters.AddService<ExecutionTimeFilter>();
});

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection"))
);

builder.Services.AddHttpContextAccessor();

builder.Services.AddScoped<IUserRepository, UserRepository>();
builder.Services.AddScoped<ICustomerRepository, CustomerRepository>();
builder.Services.AddScoped<ICategoryRepository, CategoryRepository>();
builder.Services.AddScoped<IProductRepository, ProductRepository>();
builder.Services.AddScoped<IInventoryRepository, InventoryRepository>();
builder.Services.AddScoped<IOrderRepository, OrderRepository>();
builder.Services.AddScoped<IProductImageRepository, ProductImageRepository>();
builder.Services.AddScoped<IApiExecutionLogRepository, ApiExecutionLogRepository>();
builder.Services.AddScoped<IApiExceptionLogRepository, ApiExceptionLogRepository>();

builder.Services.AddScoped<IUserService, UserService>();
builder.Services.AddScoped<IAuthService, AuthService>();
builder.Services.AddScoped<IEmailService, EmailService>();
builder.Services.AddScoped<ICategoryService, CategoryService>();
builder.Services.AddScoped<ICustomerService, CustomerService>();
builder.Services.AddScoped<ICustomerAuthService, CustomerAuthService>();
builder.Services.AddScoped<IProductService, ProductService>();
builder.Services.AddScoped<IInventoryService, InventoryService>();
builder.Services.AddScoped<IOrderService, OrderService>();
builder.Services.AddScoped<IProductImageService, ProductImageService>();

builder.Services.AddScoped<IJwtHelper, JwtHelper>();
builder.Services.AddScoped<IPasswordHelper, PasswordHelper>();

var jwtSettings = builder.Configuration.GetSection("JwtSettings");
var jwtKey = jwtSettings["Key"]!;

builder
    .Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = jwtSettings["Issuer"],
            ValidAudience = jwtSettings["Audience"],
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey)),
        };

        options.Events = new JwtBearerEvents
        {
            OnMessageReceived = context =>
            {
                var token = context.Request.Cookies["accessToken"];
                if (!string.IsNullOrEmpty(token))
                {
                    context.Token = token;
                }

                return Task.CompletedTask;
            },
        };
    });

builder.Services.AddAuthorization(
    (options) =>
    {
        options.AddPolicy(
            "CanManageProducts",
            policy =>
            {
                policy.RequireRole("Administrator", "Manager", "Employee");
            }
        );
    }
);

builder.Services.AddAuthorization();

builder.Services.AddAutoMapper(cfg =>
{
    cfg.AddProfile<MappingProfile>();
});

builder.Services.AddScoped<IValidator<CreateProductRequestDto>, CreateProductRequestValidator>();
builder.Services.AddScoped<ExecutionTimeFilter>();
builder.Services.AddHttpContextAccessor();

var app = builder.Build();

app.UseStaticFiles();
app.UseMiddleware<ExceptionMiddleware>();
app.UseSwagger();
app.UseSwaggerUI();
app.UseHttpsRedirection();
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();

app.Run();
