using Api.Dtos;
using Api.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Api.Controllers;

[Authorize(Roles = "SuperAdmin, Admin, Manager")]
[Route("api/[controller]")]
[ApiController]
public class OrdersController(OrderService orderService) : ControllerBase
{
    [HttpGet("{id:int}")]
    public async Task<ActionResult<OrderResponse>> GetOrderById(int id)
    {
        return Ok(await orderService.GetOrderById(id));
    }

    [HttpPost]
    public async Task<ActionResult> CreateOrder(CreateOrderRequest request)
    {
        var createdOrder = await orderService.CreateOrder(request);
        return CreatedAtAction(
            nameof(GetOrderById),
            new { id = createdOrder.Id },
            createdOrder
        );
    }
}
