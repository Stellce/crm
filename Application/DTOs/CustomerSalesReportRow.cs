namespace Application.DTOs;

public record CustomerSalesReportRow
{
    public int CustomerId { get; set; }
    public string CustomerName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public int OrdersCount { get; set; }
    public decimal TotalRevenue { get; set; }
    public DateTimeOffset? LastOrderAt { get; set; }
}