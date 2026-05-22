using Application.DTOs;
using Domain.Entities;
using Application.Exceptions;
using Application.Abstractions;
using Microsoft.EntityFrameworkCore;

namespace Application.Services;

public class OrderService(
    IAppDbContext context
)
{
    public async Task<PagedResponse<OrderResponse>> GetAllOrdersAsync(OrderQueryParameters queryParams)
    {
        var orderQuery = context.Orders
            .AsNoTracking();

        if (queryParams.CustomerId is int customerId)
        {
            orderQuery = orderQuery
                .Where(order => order.CustomerId == customerId);
        }

        if (queryParams.MinTotalAmount is decimal minTotalAmount)
        {
            orderQuery = orderQuery
                .Where(order => order.TotalAmount >= minTotalAmount);
        }

        if (queryParams.MaxTotalAmount is decimal maxTotalAmount)
        {
            orderQuery = orderQuery
                .Where(order => order.TotalAmount <= maxTotalAmount);
        }

        if (queryParams.CreatedFrom is DateTimeOffset createdFrom)
        {
            orderQuery = orderQuery
                .Where(order => order.CreatedAt >= createdFrom);
        }

        if (queryParams.CreatedTo is DateTimeOffset createdTo)
        {
            orderQuery = orderQuery
                .Where(order => order.CreatedAt <= createdTo);
        }

        var totalCount = await orderQuery.CountAsync();

        orderQuery = queryParams.SortBy.ToLowerInvariant() switch
        {
            "customerid" => queryParams.IsDescending
                ? orderQuery.OrderByDescending(order => order.CustomerId)
                : orderQuery.OrderBy(order => order.CustomerId),

            "totalamount" => queryParams.IsDescending
                ? orderQuery.OrderByDescending(order => order.TotalAmount)
                : orderQuery.OrderBy(order => order.TotalAmount),

            "createdat" => queryParams.IsDescending
                ? orderQuery.OrderByDescending(order => order.CreatedAt)
                : orderQuery.OrderBy(order => order.CreatedAt),

            _ => queryParams.IsDescending
                ? orderQuery.OrderByDescending(order => order.Id)
                : orderQuery.OrderBy(order => order.Id)
        };

        var orders = await orderQuery
            .Skip((queryParams.Page - 1) * queryParams.PageSize)
            .Take(queryParams.PageSize)
            .Select(order => new OrderResponse(
                order.Id,
                order.CustomerId,
                order.TotalAmount,
                order.CreatedAt
            ))
            .ToListAsync();

        return new PagedResponse<OrderResponse>(
            orders,
            queryParams.Page,
            queryParams.PageSize,
            totalCount
        );
    }

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
