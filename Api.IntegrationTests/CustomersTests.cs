using System.Net;
using System.Net.Http.Json;
using Application.DTOs;
using FluentAssertions;
using Microsoft.AspNetCore.Http;

namespace Api.IntegrationTests;

public class CustomersTests(
    SqlServerFixture sqlServer,
    RedisFixture redis) 
    : IntegrationTestBase(sqlServer, redis), 
        IClassFixture<SqlServerFixture>,
        IClassFixture<RedisFixture>
{    
    [Fact]
    public async Task GetCustomers_WithoutToken_ReturnsUnauthorized()
    {
        var response = await Client.GetAsync("/api/customers");

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task CreateCustomer_WithSuperAdminToken_ReturnsCreatedCustomer()
    {
        await Api.AuthorizeAsSuperAdminAsync();

        var request = new CreateCustomerRequest(
            "Alex",
            "alex@test.com"
        );

        var response = await Client.PostAsJsonAsync("/api/customers", request);

        response.StatusCode.Should().Be(HttpStatusCode.Created);
        var customer = await response.Content.ReadFromJsonAsync<CustomerResponse>();
        customer.Should().NotBeNull();
        customer.Id.Should().BePositive();
        customer.Name.Should().Be("Alex");
        customer.Email.Should().Be("alex@test.com");
    }

    [Fact]
    public async Task GetCustomers_WithSuperAdminToken_ReturnsCustomers()
    {
        await Api.AuthorizeAsSuperAdminAsync();

        var response = await Client.GetAsync("/api/customers");

        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var customersPage = await response.Content.ReadFromJsonAsync<PagedResponse<CustomerResponse>>();

        customersPage.Should().NotBeNull();
    }

    [Fact]
    public async Task CreateCustomer_WithDuplicateEmail_ReturnsConflict()
    {
        await Api.AuthorizeAsSuperAdminAsync();

        var customerRequest = new CreateCustomerRequest(
            "Alex",
            "alex@test.com"
        );

        var customerResponse = await Client.PostAsJsonAsync("/api/customers", customerRequest);
        customerResponse.StatusCode.Should().Be(HttpStatusCode.Created);

        var secondCustomerRequest = new CreateCustomerRequest(
            "Duplicate",
            "alex@test.com"
        );

        var response = await Client.PostAsJsonAsync("/api/customers", secondCustomerRequest);

        response.StatusCode.Should().Be(HttpStatusCode.Conflict);
    }

    [Fact]
    public async Task CreateCustomer_WithInvalidEmail_ReturnsBadRequest()
    {
        await Api.AuthorizeAsSuperAdminAsync();

        var request = new CreateCustomerRequest(
            "Alex",
            "badEmail"
        );

        var response = await Client.PostAsJsonAsync("/api/customers", request);

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task GetCustomers_WhenRateLimitExceeded_ReturnsTooManyRequests()
    {
        await Api.AuthorizeAsSuperAdminAsync();

        HttpResponseMessage response = null!;

        for(var i = 0; i < 61; i++)
        {
            response = await Client.GetAsync("/api/customers");
        }

        response.StatusCode.Should().Be(HttpStatusCode.TooManyRequests);
    }

    
    [Fact]
    public async Task GetCustomer_AfterChange_ReturnsChangedCustomer()
    {
        await Api.AuthorizeAsSuperAdminAsync();

        var customer = await Api.CreateCustomerAsync();

        var firstGetResponse = await Client.GetAsync($"/api/customers/{customer.Id}");
        var firstGetBody = await firstGetResponse.Content.ReadAsStringAsync();

        firstGetResponse.StatusCode.Should().Be(HttpStatusCode.OK, firstGetBody);

        var cachedCustomer = await firstGetResponse.Content.ReadFromJsonAsync<CustomerResponse>();

        cachedCustomer.Should().NotBeNull();
        cachedCustomer.Email.Should().Be(customer.Email);
        
        var newEmail = "changed-email@test.com";

        var request = new PatchCustomerRequest(customer.Name, newEmail);

        var patchResponse = await Client.PatchAsJsonAsync($"/api/customers/{customer.Id}", request);
        
        patchResponse.StatusCode.Should().Be(HttpStatusCode.NoContent);

        var secondGetResponse = await Client.GetAsync($"/api/customers/{customer.Id}");
        var secondGetBody = await secondGetResponse.Content.ReadAsStringAsync();

        secondGetResponse.StatusCode.Should().Be(HttpStatusCode.OK, secondGetBody);

        var changedCustomer = await secondGetResponse.Content.ReadFromJsonAsync<CustomerResponse>();

        changedCustomer.Should().NotBeNull();
        changedCustomer.Email.Should().Be(newEmail);
    }
}