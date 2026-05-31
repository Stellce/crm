namespace Infrastructure.Email;

public class SmtpEmailOptions
{
    public bool Enabled { get; init; }
    public string Host { get; init; } = string.Empty;
    public int Port { get; init; }
    public string From { get; init; } = string.Empty;
    public string? User { get; init; }
    public string? Password { get; init; }
    public bool EnableSsl { get; init; }
}