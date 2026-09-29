using System.Text;
using FluentValidation;
using Hangfire;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using OIMS.API.Filters;
using OIMS.API.Middlewares;
using OIMS.Application.BackgroundJobs;
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

// QuestPDF

QuestPDF.Settings.License = LicenseType.Community;

// Controllers

builder.Services.AddControllers(options =>
{
    options.Filters.AddService<ExecutionTimeFilter>();
});

// Swagger

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Database

builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection"))
);

// HttpContextAccessor

builder.Services.AddHttpContextAccessor();

// Repositories

builder.Services.AddScoped<IUserRepository, UserRepository>();
builder.Services.AddScoped<ICustomerRepository, CustomerRepository>();
builder.Services.AddScoped<ICategoryRepository, CategoryRepository>();
builder.Services.AddScoped<IProductRepository, ProductRepository>();
builder.Services.AddScoped<IInventoryRepository, InventoryRepository>();
builder.Services.AddScoped<IOrderRepository, OrderRepository>();
builder.Services.AddScoped<IProductImageRepository, ProductImageRepository>();
builder.Services.AddScoped<IApiExecutionLogRepository, ApiExecutionLogRepository>();
builder.Services.AddScoped<IApiExceptionLogRepository, ApiExceptionLogRepository>();
builder.Services.AddScoped<INotificationRepository, NotificationRepository>();

// Services

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
builder.Services.AddScoped<INotificationService, NotificationService>();

// Helpers

builder.Services.AddScoped<IJwtHelper, JwtHelper>();
builder.Services.AddScoped<IPasswordHelper, PasswordHelper>();

// JWT Authentication

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

// Authorization

builder.Services.AddAuthorization(options =>
{
    options.AddPolicy(
        "CanManageProducts",
        policy =>
        {
            policy.RequireRole("Administrator", "Manager", "Employee");
        }
    );
});

// AutoMapper

builder.Services.AddAutoMapper(cfg =>
{
    cfg.AddProfile<MappingProfile>();
});

// FluentValidation

builder.Services.AddScoped<IValidator<CreateProductRequestDto>, CreateProductRequestValidator>();

// Action Filter

builder.Services.AddScoped<ExecutionTimeFilter>();

// Hangfire

builder.Services.AddHangfire(config =>
{
    config.UseSqlServerStorage(builder.Configuration.GetConnectionString("DefaultConnection"));
});

builder.Services.AddHangfireServer();

// Background Jobs

builder.Services.AddScoped<LowStockNotificationJob>();
builder.Services.AddScoped<SendOrderConfirmationEmailJob>();

var app = builder.Build();

// Middleware

app.UseStaticFiles();

app.UseMiddleware<ExceptionMiddleware>();

app.UseSwagger();

app.UseSwaggerUI();

app.UseHttpsRedirection();

app.UseAuthentication();

app.UseAuthorization();

// Hangfire Dashboard

app.UseHangfireDashboard("/hangfire");

// Controllers

app.MapControllers();

// Schedule the recurring job for low-stock notifications

using (var scope = app.Services.CreateScope())
{
    var recurringJobManager = scope.ServiceProvider.GetRequiredService<IRecurringJobManager>();

    recurringJobManager.AddOrUpdate<LowStockNotificationJob>(
        "low-stock-notification",
        job => job.ExecuteAsync(),
        "0 9 * * *",
        new RecurringJobOptions
        {
            TimeZone = TimeZoneInfo.FindSystemTimeZoneById("India Standard Time"),
        }
    );
}

//using (var scope = app.Services.CreateScope())
//{
//    var recurringJobManager = scope.ServiceProvider.GetRequiredService<IRecurringJobManager>();

//    recurringJobManager.AddOrUpdate<LowStockNotificationJob>(
//        "low-stock-notification",
//        job => job.ExecuteAsync(),
//        "*/10 * * * * *",
//        new RecurringJobOptions
//        {
//            TimeZone = TimeZoneInfo.FindSystemTimeZoneById("India Standard Time"),
//        }
//    );
//}

app.Run();
