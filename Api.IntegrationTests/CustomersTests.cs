using System.Net;
using System.Net.Http.Json;
using Application.DTOs;
using FluentAssertions;
using Api.IntegrationTests.Helpers;

namespace Api.IntegrationTests;

public class CustomersTests(SqlServerFixture sqlServer) 
    : IntegrationTestBase(sqlServer), IClassFixture<SqlServerFixture>
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
}