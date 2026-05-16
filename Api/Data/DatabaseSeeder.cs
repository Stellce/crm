using Api.Entities;
using Api.Security;
using Microsoft.AspNetCore.Identity;

namespace Api.Data;

public static class DatabaseSeeder
{
    public static void Seed(AppDbContext context, IConfiguration configuration)
    {
        if (!context.Users.Any(user => user.Email == configuration["Seed:SuperAdmin:Email"]))
        {
            var superAdminSection = configuration.GetSection("Seed:SuperAdmin");

            var email = superAdminSection["Email"];
            var password = superAdminSection["Password"];

            if (string.IsNullOrEmpty(email) || string.IsNullOrEmpty(password))
            {
                throw new Exception("SuperAdmin credentials are not properly configured.");
            }

            var hasher = new PasswordHasher<User>();
            var user = new User
            {
                Email = email,
                Role = UserRole.SuperAdmin,
                CreatedAt = DateTimeOffset.UtcNow
            };
            user.PasswordHash = hasher.HashPassword(user, password);

            context.Users.Add(user);
            context.SaveChanges();
        }
    }
    public static async Task SeedAsync(AppDbContext context, IConfiguration configuration)
    {
        if (!context.Users.Any(user => user.Email == configuration["Seed:SuperAdmin:Email"]))
        {
            var superAdminSection = configuration.GetSection("Seed:SuperAdmin");

            var email = superAdminSection["Email"];
            var password = superAdminSection["Password"];

            if (string.IsNullOrEmpty(email) || string.IsNullOrEmpty(password))
            {
                throw new Exception("SuperAdmin credentials are not properly configured.");
            }

            var hasher = new PasswordHasher<User>();
            var user = new User
            {
                Email = email,
                Role = UserRole.SuperAdmin,
                CreatedAt = DateTimeOffset.UtcNow
            };
            user.PasswordHash = hasher.HashPassword(user, password);

            context.Users.Add(user);
            await context.SaveChangesAsync();
        }
    }
}
