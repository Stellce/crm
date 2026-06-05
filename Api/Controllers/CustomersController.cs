using Application.DTOs;
using Application.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Api.Security;
using Microsoft.AspNetCore.RateLimiting;

namespace Api.Controllers;

[Authorize(Policy = AppPolicies.ManageCustomers)]
[EnableRateLimiting(RateLimitPolicies.UserApi)]
[Route("api/customers")]
[ApiController]
public class CustomersController(
    CustomerService customerService
) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<PagedResponse<CustomerResponse>>> GetAllCustomers(
        [FromQuery] CustomerQueryParameters queryParams)
    {
        return Ok(await customerService.GetAllCustomersAsync(queryParams));
    }

    [HttpGet("{id:int}")]
    public async Task<ActionResult<CustomerResponse>> GetCustomerById(int id, CancellationToken cancellationToken)
    {
        var customerResponse = await customerService.GetCustomerByIdAsync(id, cancellationToken);
        return Ok(customerResponse);
    }

    [HttpGet("{id:int}/orders")]
    public async Task<ActionResult<PagedResponse<OrderResponse>>> GetCustomerOrders(
        [FromQuery] CustomerOrdersQueryParameters queryParams, 
        int id)
    {
        return Ok(await customerService.GetCustomerOrders(queryParams, id));
    }

    [HttpPost]
    public async Task<ActionResult<CustomerResponse>> CreateCustomer(
        [FromBody] CreateCustomerRequest request)
    {
        var createdCustomer = await customerService.CreateCustomer(request);

        return CreatedAtAction(
            nameof(GetCustomerById),
            new { id = createdCustomer.Id },
            createdCustomer
        );
    }

    [HttpPut("{id:int}")]
    public async Task<ActionResult> PutCustomer(
        [FromBody] CreateCustomerRequest request, 
        int id)
    {
        await customerService.PutCustomer(request, id);
        return NoContent();
    }

    [HttpPatch("{id:int}")]
    public async Task<ActionResult> PatchCustomer(
        [FromBody] PatchCustomerRequest request, 
        int id)
    {
        await customerService.PatchCustomer(request, id);
        return NoContent();
    }

    [HttpDelete("{id:int}")]
    public async Task<ActionResult> DeleteCustomer(int id)
    {
        await customerService.DeleteCustomer(id);
        return NoContent();
    }
}
