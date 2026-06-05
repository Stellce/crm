namespace Domain.Entities;

public class OrderAttachment
{
    public int Id { get; set; }

    public int OrderId { get; set; }

    public required string OriginalFileName { get; set; }
    public required string StoredFileName { get; set; }

    public required string ContentType { get; set; }
    public long SizeBytes { get; set; }

    public DateTimeOffset CreatedAt { get; set; }

    public virtual Order Order { get; set; } = null!;
}