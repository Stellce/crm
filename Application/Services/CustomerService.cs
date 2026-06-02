using Application.DTOs;
using Application.Exceptions;
using Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Application.Abstractions;
using Microsoft.Extensions.Logging;
using Application.Caching;

namespace Application.Services;

public class CustomerService(
    IAppDbContext context,
    ILogger<CustomerService> logger,
    IAppCache cache
)
{
    public async Task<PagedResponse<CustomerResponse>> GetAllCustomersAsync(CustomerQueryParameters queryParams)
    {
        var customersQuery = context.Customers
            .AsNoTracking();

        if (!string.IsNullOrWhiteSpace(queryParams.Search))
        {
            var search = queryParams.Search.Trim();

            customersQuery = customersQuery.Where(customer =>
                customer.Name.Contains(search) ||
                customer.Email.Contains(search)
            );
        }

        customersQuery = queryParams.SortBy.ToLower() switch
        {
            "name" => queryParams.IsDescending
                ? customersQuery.OrderByDescending(customer => customer.Name)
                : customersQuery.OrderBy(customer => customer.Name),

            "email" => queryParams.IsDescending
                ? customersQuery.OrderByDescending(customer => customer.Email)
                : customersQuery.OrderBy(customer => customer.Email),

            _ => queryParams.IsDescending
                ? customersQuery.OrderByDescending(customer => customer.Id)
                : customersQuery.OrderBy(customer => customer.Id)
        };

        var totalCount = await customersQuery.CountAsync();

        var customers = await customersQuery
            .Skip((queryParams.Page - 1) * queryParams.PageSize)
            .Take(queryParams.PageSize)
            .Select(customer => new CustomerResponse(
                customer.Id,
                customer.Name,
                customer.Email
            ))
            .ToListAsync();

        return new PagedResponse<CustomerResponse>(
            customers,
            queryParams.Page,
            queryParams.PageSize,
            totalCount
        );
    }

    public async Task<CustomerResponse> GetCustomerByIdAsync(int id, CancellationToken cancellationToken)
    {
        var key = CacheKeys.CustomerById(id);

        var cachedCustomer = await cache.GetAsync<CustomerResponse>(key, cancellationToken);

        if (cachedCustomer is not null)
        {
            return cachedCustomer;
        }

        var customer = await context.Customers
            .AsNoTracking()
            .Where(customer => customer.Id == id)
            .Select(customer => new CustomerResponse(
                customer.Id,
                customer.Name,
                customer.Email
            ))
            .FirstOrDefaultAsync(cancellationToken) 
            ?? throw new AppException(ErrorCode.CustomerNotFound);
        
        await cache.SetAsync(
            key, 
            customer, 
            TimeSpan.FromMinutes(10), 
            cancellationToken);

        return customer;
    }

    public async Task<PagedResponse<OrderResponse>> GetCustomerOrders(CustomerOrdersQueryParameters queryParams, int customerId)
    {
        var ordersQuery = context.Orders
            .AsNoTracking()
            .Where(o => o.CustomerId == customerId);

        if (queryParams.CreatedFrom is DateTimeOffset createdFrom)
        {
            ordersQuery = ordersQuery.Where(order => order.CreatedAt >= createdFrom);
        }

        if (queryParams.CreatedTo is DateTimeOffset createdTo)
        {
            ordersQuery = ordersQuery.Where(order => order.CreatedAt <= createdTo);
        }

        if (queryParams.MinTotalAmount is decimal minTotalAmount)
        {
            ordersQuery = ordersQuery.Where(order => order.TotalAmount > minTotalAmount);
        }

        if (queryParams.MaxTotalAmount is decimal maxTotalAmount)
        {
            ordersQuery = ordersQuery.Where(order => order.TotalAmount < maxTotalAmount);
        }

        ordersQuery = queryParams.SortBy switch
        {
            "totalamount" => queryParams.IsDescending
                ? ordersQuery.OrderByDescending(order => order.TotalAmount)
                : ordersQuery.OrderBy(order => order.TotalAmount),

            "createdat" => queryParams.IsDescending
                ? ordersQuery.OrderByDescending(order => order.CreatedAt)
                : ordersQuery.OrderBy(order => order.CreatedAt),

            _ => queryParams.IsDescending
                ? ordersQuery.OrderByDescending(order => order.Id)
                : ordersQuery.OrderBy(order => order.Id)
        };

        var totalCount = await ordersQuery.CountAsync();

        var orders = await ordersQuery
            .Skip(queryParams.Skip)
            .Take(queryParams.PageSize)
            .Select(o => new OrderResponse(o.Id, customerId, o.TotalAmount, o.CreatedAt))
            .ToListAsync();

        return new PagedResponse<OrderResponse>(
            orders,
            queryParams.Page,
            queryParams.PageSize,
            totalCount
        );
    }

    public async Task<CustomerResponse> CreateCustomer(CreateCustomerRequest request)
    {
        if (await context.Customers.AnyAsync(c => c.Email == request.Email))
        {
            logger.LogWarning("Customer creation failed: email already exists");
            throw new AppException(ErrorCode.CustomerAlreadyExists);
        }

        var customer = new Customer
        {
            Name = request.Name,
            Email = request.Email
        };

        context.Customers.Add(customer);
        await context.SaveChangesAsync();

        logger.LogInformation("Customer {CustomerId} created", customer.Id);

        return new CustomerResponse(
            customer.Id,
            customer.Name,
            customer.Email
        );
    }

    public async Task PutCustomer(CreateCustomerRequest request, int id)
    {
        var customer = await context.Customers.FindAsync(id) ?? throw new AppException(ErrorCode.CustomerNotFound);
        customer.Name = request.Name;
        customer.Email = request.Email;

        await context.SaveChangesAsync();

        await cache.RemoveAsync(CacheKeys.CustomerById(id));
    }

    public async Task PatchCustomer(PatchCustomerRequest request, int id)
    {
        var customer = await context.Customers.FindAsync(id) ?? throw new AppException(ErrorCode.CustomerNotFound);

        customer.Name = request.Name ?? customer.Name;
        customer.Email = request.Email ?? customer.Email;

        await context.SaveChangesAsync();
        
        await cache.RemoveAsync(CacheKeys.CustomerById(id));
    }

    public async Task DeleteCustomer(int id)
    {
        var customer = await context.Customers.FindAsync(id) ?? throw new AppException(ErrorCode.CustomerNotFound);
        context.Customers.Remove(customer);

        await context.SaveChangesAsync();
        
        await cache.RemoveAsync(CacheKeys.CustomerById(id));
    }
}