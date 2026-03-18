using AutoMapper;
using Microsoft.EntityFrameworkCore;
using SwiftPay.Configuration;
using SwiftPay.Mapper;
using SwiftPay.Mapper;
using SwiftPay.Profiles;
using SwiftPay.Repositories;
using SwiftPay.Repositories.Interfaces;
using SwiftPay.Services;
using SwiftPay.Services.Interfaces;
using System.Text.Json.Serialization;


var builder = WebApplication.CreateBuilder(args);

builder.Services.AddDbContext<AppDbContext>(
    options => options.UseSqlServer(
        builder.Configuration.GetConnectionString("SwiftPayDb")));

// Register Remittance Repository and Service
builder.Services.AddScoped<SwiftPay.Repositories.Interfaces.IRemittanceRepository, SwiftPay.Repositories.RemittanceRepository>();
builder.Services.AddScoped<SwiftPay.Services.Interfaces.IRemittanceService, SwiftPay.Services.RemittanceService>();

// Register User Repository and Service
builder.Services.AddScoped<SwiftPay.Repositories.Interfaces.IUserRepository, SwiftPay.Repositories.UserRepository>();
builder.Services.AddScoped<SwiftPay.Services.Interfaces.IUserService, SwiftPay.Services.UserService>();

// Register Customer Repository and Service
builder.Services.AddScoped<SwiftPay.Repositories.Interfaces.ICustomerRepository, SwiftPay.Repositories.CustomerRepository>();
builder.Services.AddScoped<SwiftPay.Services.Interfaces.ICustomerService, SwiftPay.Services.CustomerService>();

// Register Beneficiary Repository and Service
builder.Services.AddScoped<SwiftPay.Repositories.Interfaces.IBeneficiaryRepository, SwiftPay.Repositories.BeneficiaryRepository>();
builder.Services.AddScoped<SwiftPay.Services.Interfaces.IBeneficiaryService, SwiftPay.Services.BeneficiaryService>();

// Register AuditLog Repository and Service
builder.Services.AddScoped<SwiftPay.Repositories.Interfaces.IAuditLogRepository, SwiftPay.Repositories.AuditLogRepository>();
builder.Services.AddScoped<SwiftPay.Services.Interfaces.IAuditLogService, SwiftPay.Services.AuditLogService>();

// Register KYCRecord Repository and Service
builder.Services.AddScoped<SwiftPay.Repositories.Interfaces.IKYCRecordRepository, SwiftPay.Repositories.KYCRecordRepository>();
builder.Services.AddScoped<SwiftPay.Services.Interfaces.IKYCRecordService, SwiftPay.Services.KYCRecordService>();

// Register NotificationAlert Repository and Service
builder.Services.AddScoped<SwiftPay.Repositories.Interfaces.INotificationAlertRepository, SwiftPay.Repositories.NotificationAlertRepository>();
builder.Services.AddScoped<SwiftPay.Services.Interfaces.INotificationAlertService, SwiftPay.Services.NotificationAlertService>();

builder.Services.AddScoped<SwiftPay.Services.Interfaces.IAmendmentService, SwiftPay.Services.AmendmentService>();       
builder.Services.AddScoped<SwiftPay.Repositories.Interfaces.IAmendmentRepository, SwiftPay.Repositories.AmendmentRepository>();     

// AutoMapper registration - consolidated mapper profiles for all entities
builder.Services.AddAutoMapper(typeof(RemittanceProfile),
    typeof(ConfigurationMapperProfile),
    typeof(SwiftPay.Profiles.KYCRecordMapperProfile),
    typeof(SwiftPay.Profiles.NotificationAlertMapperProfile),
    typeof(SwiftPay.Profiles.AuditLogMapperProfile));
// Register ComplianceCheck repository and service so controllers can resolve them
builder.Services.AddScoped<SwiftPay.Repositories.Interfaces.IComplianceCheckRepository, SwiftPay.Repositories.ComplianceCheckRepository>();
builder.Services.AddScoped<SwiftPay.Services.Interfaces.IComplianceCheckService, SwiftPay.Services.ComplianceCheckService>();

//Compliance Decision//
// 1. Register the Repository
builder.Services.AddScoped<IComplianceDecisionRepository, ComplianceDecisionRepository>();
// 2. Register the Service
builder.Services.AddScoped<IComplianceDecisionService, ComplianceDecisionService>();
// AutoMapper registration - ensure AutoMapper and its extensions package versions are compatible.
// Register mapper profiles so IMapper is available for services.



builder.Services.AddAutoMapper(typeof(ConfigurationMapperProfile));


// Add services to the container.

builder.Services.AddControllers()
    .AddJsonOptions(options =>
    {
        // Allow enums to be serialized/deserialized as strings
        options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter());
        // Use camelCase for JSON properties
        options.JsonSerializerOptions.PropertyNamingPolicy = null; // Keep original casing
    });
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();