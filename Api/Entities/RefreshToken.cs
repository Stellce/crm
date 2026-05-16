namespace Api.Entities;

public class RefreshToken {
    public int Id { get; set; }
    public required string TokenHash { get; set; }
    public DateTimeOffset ExpiresAt { get; set; }
    public DateTimeOffset CreatedAt { get; set; }

    public DateTimeOffset? RevokedAt { get; set; }
    public string? ReplacedByTokenHash { get; set; }

    public int UserId { get; set; }
    public virtual User User { get; set; } = null!;

    public bool IsActive => RevokedAt == default && DateTimeOffset.UtcNow < ExpiresAt;
}