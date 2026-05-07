using Crm.Api.Data;
using Crm.Api.Dtos;
using Crm.Api.Entities;
using Crm.Api.Exceptions;
using Crm.Api.Security;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace Crm.Api.Services;

public class AuthService(
    AppDbContext context,
    IPasswordHasher<User> hasher,
    JwtService jwtService
)
{
    public async Task<AuthResponse> RegisterUser(RegisterRequest registerRequest)
    {
        var user = new User
        {
            Email = registerRequest.Email,
            Role = UserRole.Manager,
            CreatedAt = DateTimeOffset.UtcNow
        };

        user.PasswordHash = hasher.HashPassword(user, registerRequest.Password);


        context.Users.Add(user);

        await context.SaveChangesAsync();

        return new AuthResponse(jwtService.GenerateToken(user));
    }

    public async Task<AuthResponse> LoginUser(LoginRequest request)
    {
        var user = await context.Users
            .SingleOrDefaultAsync(user => user.Email == request.Email) ?? throw new AppException(ErrorCode.InvalidCredentials);

        if (hasher.VerifyHashedPassword(user, user.PasswordHash, request.Password) == PasswordVerificationResult.Failed)
        {
            throw new AppException(ErrorCode.InvalidCredentials);
        }

        return new AuthResponse(jwtService.GenerateToken(user));
    }

    public async Task<UserResponse> GetMe(int userId)
    {
        return await context.Users
            .AsNoTracking()
            .Where(u => u.Id == userId)
            .Select(u => new UserResponse(
                    u.Email,
                    u.Role,
                    u.CreatedAt
            ))
            .SingleOrDefaultAsync()
            ?? throw new AppException(ErrorCode.UserNotFound);
    }
}