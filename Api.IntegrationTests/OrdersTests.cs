using System.Net;
using System.Net.Http.Json;
using Application.DTOs;
using FluentAssertions;

namespace Api.IntegrationTests;

public class OrdersTests(
    SqlServerFixture sqlServer,
    RedisFixture redis) 
    : IntegrationTestBase(sqlServer, redis), 
        IClassFixture<SqlServerFixture>,
        IClassFixture<RedisFixture>
{
    [Fact]
    public async Task CreateOrder_WithExistingCustomer_ReturnsCreatedOrder()
    {
        var authRequest = new LoginRequest(
            "superadmin@crm.local",
            "SuperAdmin123!"
        );

        var authResponse = await Client.PostAsJsonAsync("/api/auth/login", authRequest);
        var auth = await authResponse.Content.ReadFromJsonAsync<AuthResponse>();
        
        auth.Should().NotBeNull();
        
        var accessToken = auth.AccessToken;
        Client.DefaultRequestHeaders.Authorization =
            new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", accessToken);
        
        var customerRequest = new CreateCustomerRequest(
            "Alex",
            "alex@test.com"
        );

        var customerResponse = await Client.PostAsJsonAsync("/api/customers", customerRequest);

        var customer = await customerResponse.Content.ReadFromJsonAsync<CustomerResponse>();
        
        customer.Should().NotBeNull();
        customer.Id.Should().BePositive();

        var orderRequest = new CreateOrderRequest(
            customer.Id,
            10.00m
        );

        var orderResponse = await Client.PostAsJsonAsync("/api/orders", orderRequest);
        var order = await orderResponse.Content.ReadFromJsonAsync<OrderResponse>();
        order.Should().NotBeNull();
        order.Id.Should().BePositive();
        order.CustomerId.Should().Be(customer.Id);
        order.TotalAmount.Should().Be(10.00m);
        order.CreatedAt.Should().BeCloseTo(DateTimeOffset.UtcNow, TimeSpan.FromSeconds(10));
    }

    [Fact]
    public async Task CreateOrder_WithExistingCustomer_ReturnsTooManyRequests()
    {
        await Api.AuthorizeAsSuperAdminAsync();

        var customer = await CreateCustomer();

        var orderRequest = new CreateOrderRequest(
            customer.Id,
            10.00m
        );

        HttpResponseMessage response = null!;

        for(var i = 0; i < 61; i++)
        {
            response = await Client.PostAsJsonAsync("/api/orders", orderRequest);
        }

        response.StatusCode.Should().Be(HttpStatusCode.TooManyRequests);
    }

    private async Task<CustomerResponse> CreateCustomer()
    {
        var customerRequest = new CreateCustomerRequest(
            "Alex",
            "alex@test.com"
        );

        var customerResponse = await Client.PostAsJsonAsync("/api/customers", customerRequest);

        var customer = await customerResponse.Content.ReadFromJsonAsync<CustomerResponse>();

        customer.Should().NotBeNull();
        return customer;
    }
}