using System.Net;
using System.Net.Http.Json;
using Application.DTOs;
using Application.Exceptions;
using Azure;
using Domain.Entities;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using System.Linq;

namespace Api.IntegrationTests;

public class ReportsTests(
    SqlServerFixture sqlServer,
    RedisFixture redis) 
    : IntegrationTestBase(sqlServer, redis),
        IClassFixture<SqlServerFixture>,
        IClassFixture<RedisFixture>
{
    [Fact]
    public async Task GetCustomerSales_ReturnsPagedResponse()
    {
        await Api.AuthorizeAsSuperAdminAsync();
        
        var customer = await Api.CreateCustomerAsync();
        await Api.CreateOrderAsync(customer.Id);

        var response = await Client.GetAsync("/api/reports/customer-sales");
        
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var pageReport = await response.Content.ReadFromJsonAsync<PagedResponse<CustomerSalesReportRow>>();

        pageReport.Should().NotBeNull();
        pageReport.Items.Count.Should().Be(1);
        pageReport.Page.Should().Be(1);
        pageReport.PageSize.Should().BePositive();
        pageReport.TotalCount.Should().Be(1);
    }

    [Fact]
    public async Task GetCustomerSales_ReturnsAggregatedRevenuePerCustomer()
    {
        await Api.AuthorizeAsSuperAdminAsync();
        
        var (customer, orders) = await Api.CreateCustomerWithOrdersAsync(100m, 10m);

        var response = await Client.GetAsync("/api/reports/customer-sales");

        var pageReport = await response.Content.ReadFromJsonAsync<PagedResponse<CustomerSalesReportRow>>();

        var report = pageReport!.Items
            .Single(r => r.CustomerId == customer.Id);

        var expected = orders.Sum(o => o.TotalAmount);
        
        report.TotalRevenue.Should().Be(expected);
    }


    [Fact]
    public async Task GetCustomerSales_FiltersCustomersByMinRevenue()
    {
        await Api.AuthorizeAsSuperAdminAsync();

        var customers = new List<CustomerResponse>();

        for(var i = 100; i <= 300; i+=100)
        {
            var (customer, orders) = await Api.CreateCustomerWithOrdersAsync(i);
            customers.Add(customer);
        }

        var customerShouldBeMissing = customers[0];

        var response = await Client.GetAsync($"/api/reports/customer-sales?minRevenue={200}");
        var pageReport = await response.Content.ReadFromJsonAsync<PagedResponse<CustomerSalesReportRow>>();

        pageReport!.Items.Count.Should().Be(2);
        pageReport!.Items.Any(r => r.CustomerId == customerShouldBeMissing.Id).Should().BeFalse();
    }

    [Fact]
    public async Task GetCustomerSales_FiltersOrdersByDateRange()
    {
        await Api.AuthorizeAsSuperAdminAsync();

        var (customer, orders) = await Api.CreateCustomerWithOrdersAsync(10m, 100m, 200m);
        var expectedAmount = 300m;

        await ExecuteDbAsync(async context =>
        {
            var order1 = await context.Orders.FirstAsync(o => o.TotalAmount == 10m);
            order1.CreatedAt = new DateTimeOffset(2026, 2, 1, 0, 0, 0, TimeSpan.Zero);
            
            var order2 = await context.Orders.FirstAsync(o => o.TotalAmount == 100m);
            order2.CreatedAt = new DateTimeOffset(2026, 3, 1, 0, 0, 0, TimeSpan.Zero);
            
            var order3 = await context.Orders.FirstAsync(o => o.TotalAmount == 200m);
            order3.CreatedAt = new DateTimeOffset(2026, 4, 1, 0, 0, 0, TimeSpan.Zero);
            
            await context.SaveChangesAsync();
        });

        var response = await Client.GetAsync("/api/reports/customer-sales?from=2026-03-01&to=2026-04-01");
        var pageReport = await response.Content.ReadFromJsonAsync<PagedResponse<CustomerSalesReportRow>>();
        
        // Debug
        var fetchedOrdersResponse = await Client.GetAsync("/api/orders?from=2026-03-01&to=2026-04-01");
        var fetchedOrders = await fetchedOrdersResponse.Content.ReadFromJsonAsync<PagedResponse<OrderResponse>>();
        
        pageReport!.Items[0].OrdersCount.Should().Be(2);
        pageReport!.Items[0].TotalRevenue.Should().Be(expectedAmount);
    }

    [Fact]
    public async Task GetCustomerSales_ReturnsCustomersOrderedByTotalRevenueDescending()
    {
        await Api.AuthorizeAsSuperAdminAsync();

        var customers = new List<CustomerResponse>();

        for(var i = 100; i <= 300; i+=100)
        {
            var (customer, _) = await Api.CreateCustomerWithOrdersAsync(i);
            customers.Add(customer);
        }

        var response = await Client.GetAsync("/api/reports/customer-sales?sortBy=totalRevenue&sortDirection=desc");

        var pageReport = await response.Content.ReadFromJsonAsync<PagedResponse<CustomerSalesReportRow>>();

        pageReport!.Items[0].CustomerId.Should().Be(customers[2].Id); //
        pageReport!.Items[1].CustomerId.Should().Be(customers[1].Id);
        pageReport!.Items[2].CustomerId.Should().Be(customers[0].Id);
    }

    [Fact]
    public async Task GetCustomerSales_ReturnsCorrectTotalCount()
    {
        await Api.AuthorizeAsSuperAdminAsync();

        var expectedTotalCount = 3;

        for(var i = 1; i < expectedTotalCount; i++)
            await Api.CreateCustomerWithOrdersAsync(10m);
        
        var response = await Client.GetAsync("/api/reports/customer-sales");
        var pageReport = await response.Content.ReadFromJsonAsync<PagedResponse<CustomerSalesReportRow>>();

        pageReport!.TotalCount.Should().Be(2);
    }

    [Fact]
    public async Task GetCustomerSales_AppliesPagination()
    {
        await Api.AuthorizeAsSuperAdminAsync();

        for(var i = 1; i <= 3; i++)
            await Api.CreateCustomerWithOrdersAsync(10m);

        var response = await Client.GetAsync("/api/reports/customer-sales?page=2&pageSize=1");
        var pageResponse = await response.Content.ReadFromJsonAsync<PagedResponse<CustomerSalesReportRow>>();

        pageResponse.Should().NotBeNull();
        pageResponse.Items.Count.Should().Be(1);
        pageResponse.TotalCount.Should().Be(3);
        pageResponse.TotalPages.Should().Be(3);
        pageResponse.HasNextPage.Should().BeTrue();
        pageResponse.HasPreviousPage.Should().BeTrue();
    }

    [Fact]
    public async Task GetCustomerTimeline_ReturnsPagedResponse()
    {
        await Api.AuthorizeAsSuperAdminAsync();

        var (customer, _) = await Api.CreateCustomerWithOrdersAsync(10m, 100m);
        var response = await  Client.GetAsync($"/api/reports/customers/{customer.Id}/timeline");
        
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var pageResponse = await response.Content.ReadFromJsonAsync<PagedResponse<CustomerTimelineEvent>>();

        pageResponse.Should().NotBeNull();
        pageResponse.Items.Count.Should().Be(2);
    }

    [Fact]
    public async Task GetCustomerTimeline_ReturnsOrderAndAttachmentEvents()
    {
        await Api.AuthorizeAsSuperAdminAsync();

        var (customer, _) = await Api.CreateCustomerWithOrdersAsync(10m, 100m);
        var response = await  Client.GetAsync($"/api/reports/customers/{customer.Id}/timeline");
        
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var pageResponse = await response.Content.ReadFromJsonAsync<PagedResponse<CustomerTimelineEvent>>();

        foreach (var timelineEvent in pageResponse!.Items)
        {
            timelineEvent.EventType.Should().BeOneOf(["OrderCreated", "AttachmentUploaded"]);
        }
    }

    [Fact]
    public async Task GetCustomerTimeline_ReturnsEventsOrderedByEventDateDescending()
    {
        await Api.AuthorizeAsSuperAdminAsync();

        var (customer, _) = await Api.CreateCustomerWithOrdersAsync(10m, 100m);

        await ExecuteDbAsync(async context =>
        {
            var order1 = await context.Orders.FirstAsync(o => o.TotalAmount == 10m);
            order1.CreatedAt = new DateTimeOffset(2026, 2, 1, 0, 0, 0, TimeSpan.Zero);

            var order2 = await context.Orders.FirstAsync(o => o.TotalAmount == 100m);
            order2.CreatedAt = new DateTimeOffset(2026, 3, 1, 0, 0, 0, TimeSpan.Zero);

            await context.SaveChangesAsync();
        });

        var response = await Client.GetAsync($"/api/reports/customers/{customer.Id}/timeline?sortBy=eventDate&sortDirection=desc");

        var pageResponse = await response.Content.ReadFromJsonAsync<PagedResponse<CustomerTimelineEvent>>();

        pageResponse!.Items[0].EventDate.Should().Be(new DateTimeOffset(2026, 3, 1, 0, 0, 0, TimeSpan.Zero));
        pageResponse!.Items[1].EventDate.Should().Be(new DateTimeOffset(2026, 2, 1, 0, 0, 0, TimeSpan.Zero));
    }

    [Fact]
    public async Task GetCustomerTimeline_ReturnsOnlyEventsForRequestedCustomer()
    {
        await Api.AuthorizeAsSuperAdminAsync();

        var (customer, _) = await Api.CreateCustomerWithOrdersAsync(10m, 100m);
        var (_, _) = await Api.CreateCustomerWithOrdersAsync(10m, 100m);

        var response = await Client.GetAsync($"/api/reports/customers/{customer.Id}/timeline");

        var pageResponse = await response.Content.ReadFromJsonAsync<PagedResponse<CustomerTimelineEvent>>();

        var ordersResponse = await Client.GetAsync($"/api/customers/{customer.Id}/orders");
        var ordersPage = await ordersResponse.Content.ReadFromJsonAsync<PagedResponse<OrderResponse>>();
        var orderIdsSet = ordersPage!.Items.Select(o => o.Id).ToHashSet();

        pageResponse!.Items.Should().OnlyContain(x => x.OrderId.HasValue && orderIdsSet.Contains(x.OrderId.Value));
    }
}