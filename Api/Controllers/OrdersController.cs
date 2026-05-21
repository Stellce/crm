using Api.Dtos;
using Api.Security;
using Api.Services;
using Api.Validators;
using FluentValidation;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;

namespace Api.Controllers;

[Authorize(Policy = AppPolicies.ManageOrders)]
[Route("api/[controller]")]
[ApiController]
public class OrdersController(OrderService orderService) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<PagedResponse<OrderResponse>>> GetAllOrders(OrderQueryParameters queryParams)
    {
        return Ok(await orderService.GetAllOrdersAsync(queryParams));
    }

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
