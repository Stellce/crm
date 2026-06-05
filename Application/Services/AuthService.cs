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
    IOptions<AuthOptions> authOptions,
    IEmailSender emailSender,
    IOptions<PasswordResetOptions> passwordResetOptions
)
{
    public async Task<AuthResponse> LoginUser(LoginRequest request, CancellationToken cancellationToken)
    {
        var user = await context.Users
            .SingleOrDefaultAsync(user => user.Email == request.Email, cancellationToken) 
            ?? throw new AppException(ErrorCode.InvalidCredentials);

        if (hasher.VerifyHashedPassword(user, user.PasswordHash, request.Password) == PasswordVerificationResult.Failed)
        {
            throw new AppException(ErrorCode.InvalidCredentials);
        }

        var refreshToken = TokenGenerator.GenerateBase64Token();
        var refreshTokenHash = TokenGenerator.Sha256Hash(refreshToken);
        context.RefreshTokens.Add(new RefreshToken
        {
            TokenHash = refreshTokenHash,
            UserId = user.Id,
            CreatedAt = DateTimeOffset.UtcNow,
            ExpiresAt = DateTimeOffset.UtcNow.Add(authOptions.Value.RefreshTokenLifetime)
        });

        await context.SaveChangesAsync(cancellationToken);

        return new AuthResponse(jwtService.GenerateToken(user), refreshToken);
    }

    public async Task<AuthResponse> RefreshToken(string refreshToken, CancellationToken cancellationToken)
    {
        var tokenHash = TokenGenerator.Sha256Hash(refreshToken);

        var storedToken = await context.RefreshTokens
            .Include(rt => rt.User)
            .SingleOrDefaultAsync(rt => rt.TokenHash == tokenHash, cancellationToken);

        if (storedToken is null || !storedToken.IsActive)
        {
            throw new AppException(ErrorCode.InvalidCredentials);
        }

        var newAccessToken = jwtService.GenerateToken(storedToken.User);
        var newRefreshToken = TokenGenerator.GenerateBase64Token();
        var newRefreshTokenHash = TokenGenerator.Sha256Hash(newRefreshToken);

        storedToken.RevokedAt = DateTimeOffset.UtcNow;
        storedToken.ReplacedByTokenHash = newRefreshTokenHash;

        context.RefreshTokens.Add(new RefreshToken
        {
            TokenHash = newRefreshTokenHash,
            UserId = storedToken.UserId,
            CreatedAt = DateTimeOffset.UtcNow,
            ExpiresAt = DateTimeOffset.UtcNow.Add(authOptions.Value.RefreshTokenLifetime)
        });

        await context.SaveChangesAsync(cancellationToken);

        return new AuthResponse(newAccessToken, newRefreshToken);
    }

    public async Task LogoutUser(string refreshToken, CancellationToken cancellationToken)
    {
        var tokenHash = TokenGenerator.Sha256Hash(refreshToken);

        var storedToken = await context.RefreshTokens
            .SingleOrDefaultAsync(rt => rt.TokenHash == tokenHash, cancellationToken);

        if (storedToken is null)
            return;

        storedToken.RevokedAt = DateTimeOffset.UtcNow;
        await context.SaveChangesAsync(cancellationToken);
    }

    public async Task RequestPasswordReset(
        ForgotPasswordRequest request,
        CancellationToken cancellationToken)
    {
        var user = await context.Users
            .SingleOrDefaultAsync(u => u.Email == request.Email, cancellationToken);
        
        if (user is null)
        {
            return;
        }

        var rawToken = TokenGenerator.GenerateUrlSafeToken();
        var tokenHash = TokenGenerator.Sha256Hash(rawToken);

        context.PasswordResetTokens.Add(new PasswordResetToken
        {
            UserId = user.Id,
            TokenHash = tokenHash,
            CreatedAt = DateTimeOffset.UtcNow,
            ExpiresAt = DateTimeOffset.UtcNow.Add(passwordResetOptions.Value.TokenLifetime)
        });

        await context.SaveChangesAsync(cancellationToken);

        var resetLink = $"{passwordResetOptions.Value.FrontendBaseUrl}/reset-password?resetToken={Uri.EscapeDataString(rawToken)}";

        await emailSender.SendAsync(
            user.Email,
            "Password reset",
            $"Reset your password: {resetLink}",
            cancellationToken);
    }

    public async Task ResetPassword(
        ResetPasswordRequest request,
        CancellationToken cancellationToken)
    {
        var tokenHash = TokenGenerator.Sha256Hash(request.Token);
        var passwordResetToken = await context.PasswordResetTokens
            .Include(token => token.User)
            .SingleOrDefaultAsync(token => token.TokenHash == tokenHash, cancellationToken);
        
        if (
            passwordResetToken is null ||
            passwordResetToken.UsedAt is not null || 
            passwordResetToken.ExpiresAt < DateTimeOffset.UtcNow)
        {
            throw new AppException(ErrorCode.InvalidResetToken);
        }


        passwordResetToken.User.PasswordHash = hasher.HashPassword(passwordResetToken.User, request.NewPassword);

        passwordResetToken.UsedAt = DateTimeOffset.UtcNow;

        await context.SaveChangesAsync(cancellationToken);
    }
}