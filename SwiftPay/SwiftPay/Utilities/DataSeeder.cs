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

        // 2. Optional Seed Admin creation (safe: do not throw if missing)
        var seedEmail = config["SeedAdmin:Email"];
        var seedPassword = config["SeedAdmin:Password"];

        if (string.IsNullOrWhiteSpace(seedEmail) || string.IsNullOrWhiteSpace(seedPassword))
        {
            Console.WriteLine("[SEED] SeedAdmin credentials not provided; skipping creation of admin user.");
            return;
        }

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

        // 5. Seed one demo user per non-Customer role for easy login during demo / grading.
        // Password is the same as SeedAdmin:Password for convenience in dev.
        var demoUsers = new (string Email, string Name, string Phone, RoleType Role)[]
        {
            ("agent@swiftpay.local",      "Demo Agent",      "+10000000001", RoleType.Agent),
            ("compliance@swiftpay.local", "Demo Compliance", "+10000000002", RoleType.Compliance),
            ("ops@swiftpay.local",        "Demo Ops",        "+10000000003", RoleType.Ops),
            ("treasury@swiftpay.local",   "Demo Treasury",   "+10000000004", RoleType.Treasury),
        };

        foreach (var (email, name, phone, roleType) in demoUsers)
        {
            var existing = await userRepo.GetByEmailAsync(email);
            if (existing != null) continue;

            var demoEntity = new User
            {
                Name = name,
                Email = email,
                Phone = phone,
                PasswordHash = BCrypt.Net.BCrypt.EnhancedHashPassword(seedPassword),
                Status = UserStatus.Active,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };
            var created = await userRepo.CreateAsync(demoEntity);
            var role = existingRoles.First(r => r.RoleType == roleType);
            await userRoleRepo.CreateAsync(new UserRole
            {
                UserId = created.UserId,
                RoleId = role.RoleId,
                IsActive = true,
                CreatedAt = DateTime.UtcNow
            });
            Console.WriteLine($"[SEED] Created demo {roleType} user: {email}");
        }
    }
}