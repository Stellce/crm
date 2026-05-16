using Api.Data;
using Api.Dtos;
using Api.Exceptions;
using Api.Entities;
using Microsoft.EntityFrameworkCore;

namespace Api.Services;

public class CustomerService(
    AppDbContext context
)
{
    public async Task<List<CustomerResponse>> GetAllCustomersAsync()
    {
        return await context.Customers
            .AsNoTracking()
            .Select((customer) => new CustomerResponse(customer.Id, customer.Name, customer.Email))
            .ToListAsync();
    }

    public async Task<CustomerResponse> GetCustomerByIdAsync(int id)
    {
        return await context.Customers
            .AsNoTracking()
            .Where(customer => customer.Id == id)
            .Select(customer => new CustomerResponse(
                customer.Id,
                customer.Name,
                customer.Email
            ))
            .FirstOrDefaultAsync() ?? throw new AppException(ErrorCode.CustomerNotFound);
    }

    public async Task<List<OrderResponse>> GetCustomerOrders(int customerId)
    {
        return await context.Orders
            .AsNoTracking()
            .Where(o => o.CustomerId == customerId)
            .Select(o => new OrderResponse(o.Id, customerId, o.TotalAmount, o.CreatedAt))
            .ToListAsync();
    }

    public async Task<CustomerResponse> CreateCustomer(CreateCustomerRequest request)
    {
        if (await context.Customers.AnyAsync(c => c.Email == request.Email))
        {
            throw new AppException(ErrorCode.CustomerAlreadyExists);
        }

        var customer = new Customer
        {
            Name = request.Name,
            Email = request.Email
        };

        context.Customers.Add(customer);
        await context.SaveChangesAsync();

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
    }

    public async Task PatchCustomer(PatchCustomerRequest request, int id)
    {
        var customer = await context.Customers.FindAsync(id) ?? throw new AppException(ErrorCode.CustomerNotFound);

        customer.Name = request.Name ?? customer.Name;
        customer.Email = request.Email ?? customer.Email;

        await context.SaveChangesAsync();
    }

    public async Task DeleteCustomer(int id)
    {
        var customer = await context.Customers.FindAsync(id) ?? throw new AppException(ErrorCode.CustomerNotFound);
        context.Customers.Remove(customer);

        await context.SaveChangesAsync();
    }
}