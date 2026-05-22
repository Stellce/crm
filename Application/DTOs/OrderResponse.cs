namespace Application.DTOs;

public record OrderResponse(
    int Id,
    int CustomerId,
    decimal TotalAmount,
    DateTimeOffset CreatedAt
);
