using Api.Data;
using Api.Dtos;
using Api.Entities;
using Api.Exceptions;
using Microsoft.EntityFrameworkCore;

namespace Api.Services;

public class OrderService(
    AppDbContext context
)
{
    public async Task<OrderResponse> GetOrderById(int id)
    {
        return await context.Orders
            .Where(order => order.Id == id)
            .Select(order => new OrderResponse(
                order.Id,
                order.CustomerId,
                order.TotalAmount,
                order.CreatedAt
            ))
            .SingleOrDefaultAsync() ?? throw new AppException(ErrorCode.OrderNotFound);
    }

    public async Task<OrderResponse> CreateOrder(CreateOrderRequest orderRequest)
    {
        if (!await context.Customers.AnyAsync(c => c.Id == orderRequest.CustomerId))
        {
            throw new AppException(ErrorCode.CustomerNotFound);
        }

        var order = new Order
        {
            CustomerId = orderRequest.CustomerId,
            TotalAmount = orderRequest.TotalAmount,
            CreatedAt = DateTimeOffset.UtcNow
        };

        context.Orders.Add(order);
        await context.SaveChangesAsync();

        return new OrderResponse(
            order.Id,
            order.CustomerId,
            order.TotalAmount,
            order.CreatedAt
        );
    }
}
