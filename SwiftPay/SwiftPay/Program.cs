using System.Reflection;

using System.Text.Json.Serialization;

using Microsoft.EntityFrameworkCore;

using SwiftPay.Configuration;

using SwiftPay.Profiles;
 
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
 
// 4. Controllers & JSON

builder.Services.AddControllers()

	.AddJsonOptions(options =>

	{

		options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter());

		options.JsonSerializerOptions.PropertyNamingPolicy = null;

	});
 
builder.Services.AddOpenApi();
 
var app = builder.Build();
 
if (app.Environment.IsDevelopment()) app.MapOpenApi();
 
app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();
 