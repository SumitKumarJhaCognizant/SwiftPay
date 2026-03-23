using System.Reflection;
using System.Text;
using System.Text.Json.Serialization;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.Authorization;
using SwiftPay.Configuration;
using SwiftPay.Profiles;
using SwiftPay.Utilities;

var builder = WebApplication.CreateBuilder(args);

// 1. Database
builder.Services.AddDbContext<AppDbContext>(opt =>
	opt.UseSqlServer(builder.Configuration.GetConnectionString("SwiftPayDb")));

// 2. Automatic Registration (The "Short" Way)
// This finds all classes ending in "Repository" or "Service" and registers them against their IInterface
var assemblies = new[] { Assembly.GetExecutingAssembly() };
foreach (var assembly in assemblies)
{
	var types = assembly.GetTypes()
		.Where(t => t.IsClass && !t.IsAbstract && (t.Name.EndsWith("Repository") || t.Name.EndsWith("Service")));

	foreach (var type in types)
	{
		var interfaceType = type.GetInterface($"I{type.Name}");
		if (interfaceType != null)
			builder.Services.AddScoped(interfaceType, type);
	}
}

// 3. Automatic AutoMapper
// This finds ALL profiles in your project automatically
builder.Services.AddAutoMapper(AppDomain.CurrentDomain.GetAssemblies());

// 4. Controllers & JSON (global authorization applied)

builder.Services.Configure<JwtTokenSettings>(builder.Configuration.GetSection("Jwt"));

builder.Services.AddControllers(options =>
{
	// Apply a global authorization policy (all endpoints require authenticated user by default)
	var policy = new AuthorizationPolicyBuilder()
					.RequireAuthenticatedUser()
					.Build();
	options.Filters.Add(new AuthorizeFilter(policy));
})
	.AddJsonOptions(options =>
	{
		options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter());
		options.JsonSerializerOptions.PropertyNamingPolicy = null;
	});

builder.Services.AddOpenApi();

// JWT Authentication
builder.Services.AddAuthentication(options =>
{
	options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
	options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
	var key = builder.Configuration["Jwt:Key"] ?? string.Empty;
	options.TokenValidationParameters = new TokenValidationParameters
	{
		ValidateIssuer = false,
		ValidateAudience = false,
		ValidateIssuerSigningKey = true,
		IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(key)),
		ValidateLifetime = true
	};
});

builder.Services.AddAuthorization();

var app = builder.Build();

if (app.Environment.IsDevelopment()) app.MapOpenApi();

app.UseHttpsRedirection();
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();
// Seed roles and initial admin user (if needed)
await DataSeeder.SeedAsync(app.Services);

app.Run();