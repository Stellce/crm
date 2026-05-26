using Application.DTOs;
using Application.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Api.Security;

namespace Api.Controllers;

[Authorize(Policy = AppPolicies.ManageOrders)]
[Route("api/[controller]")]
[ApiController]
public class OrdersController(OrderService orderService) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<PagedResponse<OrderResponse>>> GetAllOrders(
        [FromQuery] OrderQueryParameters queryParams)
    {
        return Ok(await orderService.GetAllOrdersAsync(queryParams));
    }

    [HttpGet("{id:int}")]
    public async Task<ActionResult<OrderResponse>> GetOrderById(int id)
    {
        return Ok(await orderService.GetOrderById(id));
    }

    [HttpPost]
    public async Task<ActionResult<OrderResponse>> CreateOrder(
        [FromBody] CreateOrderRequest request)
    {
        var createdOrder = await orderService.CreateOrder(request);
        return CreatedAtAction(
            nameof(GetOrderById),
            new { id = createdOrder.Id },
            createdOrder
        );
    }
}
