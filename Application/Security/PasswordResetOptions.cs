namespace Application.Security;

public class PasswordResetOptions
{
    public string FrontendBaseUrl { get; set; } = "";
    public TimeSpan TokenLifetime { get; set; } = TimeSpan.FromMinutes(30);
}