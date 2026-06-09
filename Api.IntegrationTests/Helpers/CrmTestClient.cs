using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using Application.DTOs;
using FluentAssertions;

namespace Api.IntegrationTests.Helpers;

public sealed class CrmTestClient(HttpClient client)
{
    private const string SuperAdminEmail = "superadmin@crm.local";
    private const string SuperAdminPassword = "SuperAdmin123!";

    public async Task<AuthResponse> LoginAsync(string email, string password)
    {
        var response = await client.PostAsJsonAsync(
            "/api/auth/login",
            new LoginRequest(email, password)
        );

        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var auth = await response.Content.ReadFromJsonAsync<AuthResponse>();

        auth.Should().NotBeNull();
        auth.AccessToken.Should().NotBeNullOrWhiteSpace();

        return auth;
    }

    public async Task AuthorizeAsSuperAdminAsync()
    {
        var auth = await LoginAsync(SuperAdminEmail, SuperAdminPassword);
        SetBearerToken(auth.AccessToken);
    }

    public async Task AuthorzeAsManager()
    {
        var email = CreateUniqueEmail("manager");
        const string password = "Manager123!";

        await AuthorizeAsSuperAdminAsync();

        await CreateManagerAsync(email, password);

        var auth = await LoginAsync(email, password);
        SetBearerToken(auth.AccessToken);
    }

    public async Task<UserResponse> CreateManagerAsync(
        string? email = null,
        string password = "Manager123!")
    {
        email ??= CreateUniqueEmail("manager");

        var response = await client.PostAsJsonAsync(
            "/api/users/manager",
            new CreateUserRequest(email, password));
        
        response.StatusCode.Should().Be(HttpStatusCode.Created);

        var manager = await response.Content.ReadFromJsonAsync<UserResponse>();

        manager.Should().NotBeNull();
        manager.Id.Should().BePositive();

        return manager;
    }

    public async Task<CustomerResponse> CreateCustomerAsync(
        string name = "Alex",
        string? email = null)
    {
        email ??= CreateUniqueEmail("customer");

        var response = await client.PostAsJsonAsync(
            "/api/customers",
            new CreateCustomerRequest(name, email));

        response.StatusCode.Should().Be(HttpStatusCode.Created);

        var customer = await response.Content.ReadFromJsonAsync<CustomerResponse>();

        customer.Should().NotBeNull();
        customer.Id.Should().BePositive();

        return customer;
    }

    public async Task<OrderResponse> CreateOrderAsync(
        int customerId,
        decimal totalAmount = 10.00m)
    {
        var response = await client.PostAsJsonAsync(
            "/api/orders",
            new CreateOrderRequest(customerId, totalAmount));

        response.StatusCode.Should().Be(HttpStatusCode.Created);

        var order = await response.Content.ReadFromJsonAsync<OrderResponse>();

        order.Should().NotBeNull();
        order.Id.Should().BePositive();

        return order;
    }

    public async Task<(CustomerResponse, OrderResponse[])> CreateCustomerWithOrdersAsync(params decimal[] orderAmounts)
    {
        var customer = await CreateCustomerAsync();
        var orders = new List<OrderResponse>();
        foreach (var orderAmount in orderAmounts)
        {
            orders.Add(await CreateOrderAsync(customer.Id, orderAmount));
        }
        return (customer, orders.ToArray());
    }

    private void SetBearerToken(string accessToken)
    {
        client.DefaultRequestHeaders.Authorization =
            new AuthenticationHeaderValue("Bearer", accessToken);
    }

    private static string CreateUniqueEmail(string prefix)
    {
        return $"{prefix}-{Guid.NewGuid():N}@test.com";
    }
}