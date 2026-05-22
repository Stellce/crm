using Application.DTOs;
using Domain.Entities;
using Application.Abstractions;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Application.Security;
using Application.Exceptions;

namespace Application.Services;

public class AuthService(
    IAppDbContext context,
    IPasswordHasher<User> hasher,
    IJwtService jwtService,
    IOptions<AuthOptions> authOptions
)
{
    public async Task<AuthResponse> LoginUser(LoginRequest request)
    {
        var user = await context.Users
            .SingleOrDefaultAsync(user => user.Email == request.Email) ?? throw new AppException(ErrorCode.InvalidCredentials);

        if (hasher.VerifyHashedPassword(user, user.PasswordHash, request.Password) == PasswordVerificationResult.Failed)
        {
            throw new AppException(ErrorCode.InvalidCredentials);
        }

        var refreshToken = jwtService.GenerateRefreshToken();
        var refreshTokenHash = jwtService.HashRefreshToken(refreshToken);
        context.RefreshTokens.Add(new RefreshToken
        {
            TokenHash = refreshTokenHash,
            UserId = user.Id,
            CreatedAt = DateTimeOffset.UtcNow,
            ExpiresAt = DateTimeOffset.UtcNow.Add(authOptions.Value.RefreshTokenLifetime)
        });

        await context.SaveChangesAsync();

        return new AuthResponse(jwtService.GenerateToken(user), refreshToken);
    }

    public async Task<AuthResponse> RefreshToken(string refreshToken)
    {
        var tokenHash = jwtService.HashRefreshToken(refreshToken);

        var storedToken = await context.RefreshTokens
            .Include(rt => rt.User)
            .SingleOrDefaultAsync(rt => rt.TokenHash == tokenHash);

        if (storedToken is null || !storedToken.IsActive)
        {
            throw new AppException(ErrorCode.InvalidCredentials);
        }

        var newAccessToken = jwtService.GenerateToken(storedToken.User);
        var newRefreshToken = jwtService.GenerateRefreshToken();
        var newRefreshTokenHash = jwtService.HashRefreshToken(newRefreshToken);

        storedToken.RevokedAt = DateTimeOffset.UtcNow;
        storedToken.ReplacedByTokenHash = newRefreshTokenHash;

        context.RefreshTokens.Add(new RefreshToken
        {
            TokenHash = newRefreshTokenHash,
            UserId = storedToken.UserId,
            CreatedAt = DateTimeOffset.UtcNow,
            ExpiresAt = DateTimeOffset.UtcNow.Add(authOptions.Value.RefreshTokenLifetime)
        });

        await context.SaveChangesAsync();

        return new AuthResponse(newAccessToken, newRefreshToken);
    }

    public async Task LogoutUser(string refreshToken)
    {
        var tokenHash = jwtService.HashRefreshToken(refreshToken);

        var storedToken = await context.RefreshTokens
            .SingleOrDefaultAsync(rt => rt.TokenHash == tokenHash);

        if (storedToken is null)
            return;

        storedToken.RevokedAt = DateTimeOffset.UtcNow;
        await context.SaveChangesAsync();
    }
}