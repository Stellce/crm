using Application.Abstractions;
using Application.DTOs;
using Application.Exceptions;
using Application.Storage;
using Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Application.Services;

public class OrderAttachmentService(
    IAppDbContext context,
    ILogger<OrderAttachmentService> logger,
    IOptions<FileStorageOptions> options,
    IFileStorage storage
)
{
    private readonly FileStorageOptions _options = options.Value;

    public async Task<OrderAttachmentResponse> UploadAsync(
        int orderId,
        Stream content,
        string originalFileName,
        string contentType,
        long sizeBytes,
        CancellationToken cancellationToken)
    {
        var orderExists = await context.Orders
            .AsNoTracking()
            .AnyAsync(o => o.Id == orderId, cancellationToken);

        if (!orderExists)
        {
            throw new AppException(ErrorCode.OrderNotFound);
        }

        if (sizeBytes <= 0)
        {
            throw new AppException(ErrorCode.FileIsEmpty);
        }

        if (sizeBytes > _options.MaxFileSizeBytes)
        {
            throw new AppException(ErrorCode.MaxFileSizeExceeded);
        }

        var extension = Path.GetExtension(originalFileName);

        if (string.IsNullOrWhiteSpace(extension) || !_options.AllowedExtensions.Contains(extension, StringComparer.OrdinalIgnoreCase))
        {
            throw new AppException(ErrorCode.InvalidFileType);
        }

        var storedFileName = await storage.SaveAsync(
            content,
            extension,
            cancellationToken
        );

        var attachment = new OrderAttachment
        {
            OrderId = orderId,
            OriginalFileName = originalFileName,
            StoredFileName = storedFileName,
            ContentType = contentType,
            SizeBytes = sizeBytes,
            CreatedAt = DateTimeOffset.UtcNow
        };

        context.OrderAttachments.Add(attachment);

        await context.SaveChangesAsync(cancellationToken);

        logger.LogInformation(
            "Order attachment uploaded. OrderId={OrderId}, AttachmentId={AttachmentId}, SizeBytes={SizeBytes}, ContentType={ContentType}",
            orderId,
            attachment.Id,
            attachment.SizeBytes,
            attachment.ContentType);

        return new OrderAttachmentResponse(
            attachment.Id,
            attachment.OriginalFileName,
            attachment.ContentType,
            attachment.SizeBytes,
            attachment.CreatedAt
        );
    }

    public async Task<ICollection<OrderAttachmentResponse>> GetAttachmentsAsync(int orderId, CancellationToken cancellationToken)
    {
        var orderExists = await context.Orders
            .AnyAsync(o => o.Id == orderId, cancellationToken);

        if (!orderExists)
        {
            throw new AppException(ErrorCode.OrderNotFound);
        }

        return await context.OrderAttachments
            .AsNoTracking()
            .Where(a => a.OrderId == orderId)
            .Select(a => new OrderAttachmentResponse(
                a.Id,
                a.OriginalFileName,
                a.ContentType,
                a.SizeBytes,
                a.CreatedAt
            ))
            .ToListAsync(cancellationToken);
    }

    public async Task<OrderAttachmentDownload> GetAttachmentFileAsync(int orderId, int attachmentId, CancellationToken cancellationToken)
    {
        var orderExists = await context.Orders
            .AsNoTracking()
            .AnyAsync(o => o.Id == orderId, cancellationToken);

        if (!orderExists)
        {
            throw new AppException(ErrorCode.OrderNotFound);
        }

        var attachment = await context.OrderAttachments
            .AsNoTracking()
            .SingleOrDefaultAsync(
                a => a.OrderId == orderId && a.Id == attachmentId, 
                cancellationToken)
            ?? throw new AppException(ErrorCode.AttachmentNotFound);

        var content = await storage.OpenReadAsync(attachment.StoredFileName, cancellationToken);

        return new OrderAttachmentDownload(
            content,
            attachment.OriginalFileName,
            attachment.ContentType,
            attachment.SizeBytes
        );
    }

    public async Task DeleteAttachment(int attachmentId, int orderId, CancellationToken cancellationToken)
    {
        var orderExists = await context.Orders
            .AnyAsync(o => o.Id == orderId, cancellationToken);
        
        if (!orderExists)
        {
            throw new AppException(ErrorCode.OrderNotFound);
        }

        var attachment = await context.OrderAttachments
            .Where(a => a.OrderId == orderId && a.Id == attachmentId)
            .SingleOrDefaultAsync(cancellationToken)
            ?? throw new AppException(ErrorCode.AttachmentNotFound);

        await context.OrderAttachments
            .Where(a => a.OrderId == orderId && a.Id == attachmentId)
            .ExecuteDeleteAsync(cancellationToken);

        await storage.DeleteAsync(attachment.StoredFileName, cancellationToken);
    }
}