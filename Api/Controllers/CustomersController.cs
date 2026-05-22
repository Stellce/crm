using Application.DTOs;
using Application.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Api.Security;

namespace Api.Controllers;

[Authorize(Policy = AppPolicies.ManageCustomers)]
[Route("api/[controller]")]
[ApiController]
public class CustomersController(
    CustomerService customerService
) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<PagedResponse<CustomerResponse>>> GetAllCustomers(CustomerQueryParameters queryParams)
    {
        return Ok(await customerService.GetAllCustomersAsync(queryParams));
    }

    [HttpGet("{id:int}")]
    public async Task<ActionResult<CustomerResponse>> GetCustomerById(int id)
    {
        var customerResponse = await customerService.GetCustomerByIdAsync(id);
        return Ok(customerResponse);
    }

    [HttpGet("{id:int}/orders")]
    public async Task<ActionResult<PagedResponse<OrderResponse>>> GetCustomerOrders(CustomerOrdersQueryParameters queryParams, int id)
    {
        return Ok(await customerService.GetCustomerOrders(queryParams, id));
    }

    [HttpPost]
    public async Task<ActionResult<CustomerResponse>> CreateCustomer(CreateCustomerRequest request)
    {
        var createdCustomer = await customerService.CreateCustomer(request);

        return CreatedAtAction(
            nameof(GetCustomerById),
            new { id = createdCustomer.Id },
            createdCustomer
        );
    }

    [HttpPut("{id:int}")]
    public async Task<ActionResult> PutCustomer(CreateCustomerRequest request, int id)
    {
        await customerService.PutCustomer(request, id);
        return NoContent();
    }

    [HttpPatch("{id:int}")]
    public async Task<ActionResult> PatchCustomer(PatchCustomerRequest request, int id)
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
