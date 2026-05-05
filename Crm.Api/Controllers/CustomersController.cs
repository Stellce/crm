using Crm.Api.Dtos;
using Crm.Api.Services;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;

namespace Crm.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class CustomersController(
        CustomerService customerService
    ) : ControllerBase
    {
        [HttpGet]
        public async Task<ActionResult<List<CustomerResponse>>> GetAllCustomers()
        {
            return Ok(await customerService.GetAllCustomersAsync());
        }

        [HttpGet("{id:int}")]
        public async Task<ActionResult<CustomerResponse>> GetCustomerById(int id)
        {
            var customerResponse = await customerService.GetCustomerByIdAsync(id);
            return Ok(customerResponse);
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
}
