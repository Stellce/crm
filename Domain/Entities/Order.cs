namespace Domain.Entities;

public class Order
{
    public int Id { get; set; }
    public int CustomerId { get; set; }
    public decimal TotalAmount { get; set; }
    public DateTimeOffset CreatedAt { get; set; }

    public virtual Customer Customer { get; set; } = null!;
    public virtual ICollection<OrderAttachment> Attachments { get; set; } = [];
}
