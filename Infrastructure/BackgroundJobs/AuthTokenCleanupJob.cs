using Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Infrastructure.BackgroundJobs;

public class AuthTokenCleanupJob(
    AppDbContext context,
    ILogger<AuthTokenCleanupJob> logger)
{
    public async Task RunAsync()
    {
        var now = DateTimeOffset.UtcNow;
        var retentionThreshold = now.AddDays(-7);

        var deletePasswordResetTokens = await context.PasswordResetTokens
            .Where(token => 
                token.ExpiresAt < retentionThreshold ||
                token.UsedAt != null && token.UsedAt < retentionThreshold)
            .ExecuteDeleteAsync();

        var deletedRefreshTokens = await context.RefreshTokens
            .Where(token => 
                token.ExpiresAt < retentionThreshold)
            .ExecuteDeleteAsync();

        logger.LogInformation(
            "Token cleanup completed. Deleted {RefreshTokensCount} refresh tokens and {PasswordResetTokensCount} password reset tokens",
            deletedRefreshTokens,
            deletePasswordResetTokens);
    }
}