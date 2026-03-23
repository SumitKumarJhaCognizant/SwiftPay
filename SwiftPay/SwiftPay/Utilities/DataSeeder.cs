using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using SwiftPay.Constants.Enums;
using SwiftPay.Models;
using SwiftPay.Repositories.Interfaces;

namespace SwiftPay.Utilities;

public static class DataSeeder
{
    public static async Task SeedAsync(IServiceProvider services)
    {
        using var scope = services.CreateScope();
        var provider = scope.ServiceProvider;
        var config = provider.GetRequiredService<IConfiguration>();
        var roleRepo = provider.GetRequiredService<IRoleRepository>();
        var userRepo = provider.GetRequiredService<IUserRepository>();
        var userRoleRepo = provider.GetRequiredService<IUserRoleRepository>();
        var mapper = provider.GetRequiredService<AutoMapper.IMapper>();

        // 1. Seed Roles from Enum (Mandatory for RBAC)
        var existingRoles = (await roleRepo.GetAllAsync()).ToList();
            if (!existingRoles.Any())
            {
                foreach (RoleType rt in Enum.GetValues(typeof(RoleType)))
                {
                    var role = new Role { RoleType = rt, CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow };
                    await roleRepo.CreateAsync(role);
                }
                existingRoles = (await roleRepo.GetAllAsync()).ToList();
            }

        // 2. Strict Configuration Check (Production Ready)
        var seedEmail = config["SeedAdmin:Email"] 
            ?? throw new InvalidOperationException("SeedAdmin:Email is missing from appsettings.json");

        var seedPassword = config["SeedAdmin:Password"] 
            ?? throw new InvalidOperationException("SeedAdmin:Password is missing from appsettings.json");

        // 3. Create Master Admin User
        var user = await userRepo.GetByEmailAsync(seedEmail);
        if (user == null)
        {
            var createDto = new SwiftPay.DTOs.UserCustomerDTO.CreateUserDto
            {
                Name = "Master Admin",
                Email = seedEmail,
                Phone = "+10000000000",
                Password = seedPassword
            };

            var userEntity = mapper.Map<User>(createDto);
            userEntity.PasswordHash = await Task.Run(() => BCrypt.Net.BCrypt.EnhancedHashPassword(seedPassword));
            userEntity.CreatedAt = DateTime.UtcNow;
            userEntity.UpdatedAt = DateTime.UtcNow;
            userEntity.Status = UserStatus.Active;

            user = await userRepo.CreateAsync(userEntity);
            Console.WriteLine($"[SEED] Created Admin: {seedEmail}");
        }

        // 4. Create Admin Role Link
        var adminRole = existingRoles.First(r => r.RoleType == RoleType.Admin);
        var userRoles = await userRoleRepo.GetByRoleIdAsync(adminRole.RoleId);
        
        if (!userRoles.Any(ur => ur.UserId == user.UserId))
        {
            await userRoleRepo.CreateAsync(new UserRole
            {
                UserId = user.UserId,
                RoleId = adminRole.RoleId,
                IsActive = true,
                CreatedAt = DateTime.UtcNow
            });
            Console.WriteLine($"[SEED] Linked Admin Role to: {seedEmail}");
        }
    }
}