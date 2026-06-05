using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using Application.DTOs;
using FluentAssertions;

namespace Api.IntegrationTests;

public class OrderAttachmentsTests(
    SqlServerFixture sqlServer,
    RedisFixture redis
) : IntegrationTestBase(sqlServer, redis),
    IClassFixture<SqlServerFixture>,
    IClassFixture<RedisFixture>
{
    [Fact]
    public async Task UploadAttachment_WithValidPdf_ReturnsCreatedAttachment()
    {
        await Api.AuthorizeAsSuperAdminAsync();

        var order = await CreateOrderAsync();
        
        var fileBytes = CreateFakePdfBytes();

        using var content = CreateMultipartFileContent(
            fileBytes,
            "invoice.pdf",
            "application/pdf");

        var response = await Client.PostAsync(
            $"/api/orders/{order.Id}/attachments",
            content);

        response.StatusCode.Should().Be(HttpStatusCode.Created);
        response.Headers.Location.Should().NotBeNull();

        var attachment = await response.Content.ReadFromJsonAsync<OrderAttachmentResponse>();

        attachment.Should().NotBeNull();
        attachment.Id.Should().BePositive();
        attachment.OriginalFileName.Should().Be("invoice.pdf");
        attachment.ContentType.Should().Be("application/pdf");
        attachment.SizeBytes.Should().Be(fileBytes.Length);

        response.Headers.Location.Should().NotBeNull();
        response.Headers.Location.AbsolutePath
            .Should().Be($"/api/orders/{order.Id}/attachments/{attachment.Id}/download");
    }

    [Fact]
    public async Task DownloadAttachment_AfterUpload_ReturnsSameFileBytes()
    {
        await Api.AuthorizeAsSuperAdminAsync();

        var order = await CreateOrderAsync();
        var originalBytes = CreateFakePdfBytes();
        
        var attachment = await UploadAttachmentAsync(
            order.Id,
            originalBytes,
            "invoice.pdf",
            "application/pdf");

        var response = await Client.GetAsync(
            $"/api/orders/{order.Id}/attachments/{attachment.Id}/download");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        response.Content.Headers.ContentType!.MediaType.Should().Be("application/pdf");

        var downloadedBytes = await response.Content.ReadAsByteArrayAsync();
        
        downloadedBytes.Should().Equal(originalBytes);
    }

    [Fact]
    public async Task GetAttachments_AfterUpload_ReturnsUploadedAttachment()
    {
        await Api.AuthorizeAsSuperAdminAsync();

        var order = await CreateOrderAsync();

        var uploaded = await UploadAttachmentAsync(
            order.Id,
            CreateFakePdfBytes(),
            "invoice.pdf",
            "application/pdf");

        var response = await Client.GetAsync($"/api/orders/{order.Id}/attachments");

        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var attachments = await response.Content.ReadFromJsonAsync<List<OrderAttachmentResponse>>();

        attachments.Should().NotBeNull();
        attachments.Should().ContainSingle(a => 
            a.Id == uploaded.Id &&
            a.OriginalFileName == "invoice.pdf" &&
            a.ContentType == "application/pdf");
    }

    [Fact]
    public async Task UploadAttachment_WithInvalidFileType_ReturnsConflict()
    {
        await Api.AuthorizeAsSuperAdminAsync();

        var order = await CreateOrderAsync();

        using var content = CreateMultipartFileContent(
            "not really an exe"u8.ToArray(),
            "malware.exe",
            "application/octet-stream");
        
        var response = await Client.PostAsync(
            $"/api/orders/{order.Id}/attachments",
            content);

        response.StatusCode.Should().Be(HttpStatusCode.Conflict);

        var problem = await response.Content.ReadFromJsonAsync<Microsoft.AspNetCore.Mvc.ProblemDetails>();

        problem.Should().NotBeNull();
        problem.Title.Should().Be("FILE_TYPE_NOT_ALLOWED");
    }

    private async Task<OrderAttachmentResponse> UploadAttachmentAsync(
        int orderId,
        byte[] fileBytes,
        string fileName,
        string contentType)
    {
        using var content = CreateMultipartFileContent(fileBytes, fileName, contentType);

        var response = await Client.PostAsync(
            $"api/orders/{orderId}/attachments",
            content);

        response.StatusCode.Should().Be(HttpStatusCode.Created);

        var attachment = await response.Content.ReadFromJsonAsync<OrderAttachmentResponse>();

        attachment.Should().NotBeNull();

        return attachment;
    }

    private static MultipartFormDataContent CreateMultipartFileContent(
        byte[] fileBytes,
        string fileName,
        string contentType
    )
    {
        var multipart = new MultipartFormDataContent();

        var fileContent = new ByteArrayContent(fileBytes);
        fileContent.Headers.ContentType = new MediaTypeHeaderValue(contentType);

        multipart.Add(fileContent, "file", fileName);

        return multipart;
    }

    private static byte[] CreateFakePdfBytes()
    {
        return "%PDF-1.4\n1 0 obj\n<<>>\nendobj\ntrailer\n<<>>\n%%EOF"u8.ToArray();
    }

    private async Task<OrderResponse> CreateOrderAsync()
    {
        var customer = await Api.CreateCustomerAsync();
        return await Api.CreateOrderAsync(customer.Id);
    }
}