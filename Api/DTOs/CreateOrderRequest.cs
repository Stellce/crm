namespace Api.Dtos;

public record CreateOrderRequest(
    int CustomerId,
    decimal TotalAmount
);