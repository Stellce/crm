namespace Application.DTOs;

public record CreateOrderRequest(
    int CustomerId,
    decimal TotalAmount
);