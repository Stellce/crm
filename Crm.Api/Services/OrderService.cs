using Crm.Api.Data;
using Crm.Api.Dtos;
using Crm.Api.Entities;
using Crm.Api.Exceptions;
using Microsoft.EntityFrameworkCore;

namespace Crm.Api.Services;

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
