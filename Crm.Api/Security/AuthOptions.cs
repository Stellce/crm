namespace Crm.Api.Security;

public class AuthOptions
{
    public TimeSpan AccessTokenLifetime { get; set; }
    public TimeSpan RefreshTokenLifetime { get; set; }
    public TimeSpan TokenClockSkew { get; set; } = TimeSpan.FromSeconds(30);
}