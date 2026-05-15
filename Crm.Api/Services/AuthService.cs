using Crm.Api.Data;
using Crm.Api.Dtos;
using Crm.Api.Entities;
using Crm.Api.Exceptions;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace Crm.Api.Services;

public class AuthService(
    AppDbContext context,
    IPasswordHasher<User> hasher,
    JwtService jwtService
)
{
    public async Task<AuthResponse> LoginUser(LoginRequest request)
    {
        var user = await context.Users
            .SingleOrDefaultAsync(user => user.Email == request.Email) ?? throw new AppException(ErrorCode.Unauthorized);

        if (hasher.VerifyHashedPassword(user, user.PasswordHash, request.Password) == PasswordVerificationResult.Failed)
        {
            throw new AppException(ErrorCode.Unauthorized);
        }

        return new AuthResponse(jwtService.GenerateToken(user));
    }
}