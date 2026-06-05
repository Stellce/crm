using Application.DTOs;
using Application.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Api.Security;
using Microsoft.AspNetCore.RateLimiting;

namespace Api.Controllers;

[Authorize(Policy = AppPolicies.ManageOrders)]
[EnableRateLimiting(RateLimitPolicies.UserApi)]
[Route("api/orders")]
[ApiController]
public class OrdersController(
    OrderService orderService,
    OrderAttachmentService orderAttachmentService) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<PagedResponse<OrderResponse>>> GetAllOrders(
        [FromQuery] OrderQueryParameters queryParams,
        CancellationToken cancellationToken)
    {
        return Ok(await orderService.GetAllOrdersAsync(queryParams, cancellationToken));
    }

    [HttpGet("{id:int}")]
    public async Task<ActionResult<OrderResponse>> GetOrderById(int id, CancellationToken cancellationToken)
    {
        return Ok(await orderService.GetOrderById(id, cancellationToken));
    }

    [HttpPost]
    public async Task<ActionResult<OrderResponse>> CreateOrder(
        [FromBody] CreateOrderRequest request,
        CancellationToken cancellationToken)
    {
        var createdOrder = await orderService.CreateOrder(request, cancellationToken);
        return CreatedAtAction(
            nameof(GetOrderById),
            new { id = createdOrder.Id },
            createdOrder
        );
    }

    [HttpPost("{orderId:int}/attachments")]
    [Consumes("multipart/form-data")]
    [RequestSizeLimit(10_000_000)]
    public async Task<ActionResult<OrderAttachmentResponse>> UploadAttachment(
        int orderId,
        IFormFile file,
        CancellationToken cancellationToken)
    {
        await using var stream = file.OpenReadStream();

        var result = await orderAttachmentService.UploadAsync(
            orderId,
            stream,
            file.FileName,
            file.ContentType,
            file.Length,
            cancellationToken);
        
        return CreatedAtAction(
            nameof(DownloadAttachment),
            new { orderId, attachmentId = result.Id },
            result);
    }

    [HttpGet("{orderId:int}/attachments")]
    public async Task<ActionResult<ICollection<OrderAttachmentResponse>>> GetAttachments(
        int orderId,
        CancellationToken cancellationToken)
    {
        var attachments = await orderAttachmentService.GetAttachmentsAsync(orderId, cancellationToken);
        return Ok(attachments);
    }

    [HttpGet("{orderId:int}/attachments/{attachmentId:int}/download")]
    public async Task<IActionResult> DownloadAttachment(
        int orderId,
        int attachmentId,
        CancellationToken cancellationToken)
    {
        var attachment = await orderAttachmentService.GetAttachmentFileAsync(orderId, attachmentId, cancellationToken);
        
        return File(
            attachment.Content,
            attachment.ContentType,
            fileDownloadName: Path.GetFileName(attachment.OriginalFileName),
            enableRangeProcessing: true);
    }

    [HttpDelete("{orderId:int}/attachments/{attachmentId:int}")]
    public async Task DeleteAttachment(
        int orderId,
        int attachmentId,
        CancellationToken cancellationToken
    )
    {
        await orderAttachmentService.DeleteAttachment(attachmentId, orderId, cancellationToken);
    }
}
