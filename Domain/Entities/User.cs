using Domain.Security;

namespace Domain.Entities;

public class User
{
    public int Id { get; set; }
    public required string Email { get; set; }
    public string PasswordHash { get; set; } = null!;
    public UserRole Role { get; set; }
    public DateTimeOffset CreatedAt { get; set; }
}
