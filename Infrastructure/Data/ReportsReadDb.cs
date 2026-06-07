using System.Data;
using Application.Abstractions;
using Application.DTOs;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Data;

public class ReportsReadDb(AppDbContext context) : IReportsReadDb
{
    public async Task<PagedResponse<CustomerSalesReportRow>> GetCustomerSalesAsync(
        CustomerSalesReportQueryParameters queryParams, 
        CancellationToken cancellationToken)
    {
        var totalCount = await context.Database
            .SqlQuery<int>($"""
                SELECT COUNT(*) AS [Value]
                FROM (
                    SELECT 
                        c.Id
                    FROM Customers AS c
                    LEFT JOIN Orders as o
                        ON o.CustomerId = c.Id
                        AND({queryParams.From} IS NULL OR o.CreatedAt >= {queryParams.From})
                        AND({queryParams.To} IS NULL OR CreatedAt <= {queryParams.To})
                    GROUP BY
                        c.Id,
                        c.Name,
                        c.Email
                    HAVING COALESCE(SUM(o.TotalAmount), 0) >= {queryParams.MinRevenue}
                ) AS report
            """)
            .SingleAsync(cancellationToken);

        List<CustomerSalesReportRow> result = [];
        
        if (totalCount > 0)
        {
            var sortColumn = queryParams.SortBy?.ToLowerInvariant() switch
            {
                "customername" => "CustomerName",
                "email" => "Email",
                "orderscount" => "OrdersCount",
                "totalrevenue" => "TotalRevenue",
                "lastorderat" => "LastOrderAt",
                _ => "TotalRevenue"
            };

            var sortDirection = queryParams.SortDirection == SortDirection.Asc 
                ? "ASC"
                : "DESC";

            var sql = $"""
                    SELECT
                        c.Id AS CustomerId,
                        c.Name AS CustomerName,
                        c.Email AS Email,
                        COUNT(o.Id) AS OrdersCount,
                        COALESCE(SUM(o.TotalAmount), 0) AS TotalRevenue,
                        MAX(o.CreatedAt) AS LastOrderAt
                    FROM Customers AS c
                    LEFT JOIN Orders AS o
                        ON o.CustomerId = c.Id
                        AND(@from IS NULL OR o.CreatedAt >= @from)
                        AND(@to IS NULL OR o.CreatedAt <= @to)
                    GROUP BY
                        c.Id,
                        c.Name,
                        c.Email
                    HAVING COALESCE(SUM(o.TotalAmount), 0) >= @minRevenue
                    ORDER BY {sortColumn} {sortDirection}
                    OFFSET @skip ROWS
                    FETCH NEXT @pageSize ROWS ONLY
                """;
            result = await context.Database
                .SqlQueryRaw<CustomerSalesReportRow>(
                    sql,
                    new SqlParameter("@from", SqlDbType.DateTimeOffset)
                    {
                        Value = queryParams.From is null ? DBNull.Value : queryParams.From
                    },
                    new SqlParameter("@to", SqlDbType.DateTimeOffset)
                    {
                        Value = queryParams.To is null ? DBNull.Value : queryParams.To
                    },
                    new SqlParameter("@minRevenue", SqlDbType.Decimal)
                    {
                        Precision = 18,
                        Scale = 2,
                        Value = queryParams.MinRevenue
                    },
                    new SqlParameter("@skip", SqlDbType.Int)
                    {
                        Value = queryParams.Skip
                    },
                    new SqlParameter("@pageSize", SqlDbType.Int)
                    {
                        Value = queryParams.PageSize
                    })
                .ToListAsync(cancellationToken);
        }
        
        return new PagedResponse<CustomerSalesReportRow>(
            result,
            queryParams.Page,
            queryParams.PageSize,
            totalCount
        );
    }

    public async Task<PagedResponse<CustomerTimelineEvent>> GetCustomerTimelineAsync(
        int customerId, 
        CustomerTimelineReportQueryParameters queryParams,
        CancellationToken cancellationToken)
    {
        var totalCount = await context.Database
            .SqlQuery<int>($"""
                SELECT COUNT(*) AS [Value]
                FROM (
                    SELECT 
                        o.CreatedAt AS EventDate,
                        'OrderCreated' AS EventType,
                        CONCAT('Order #', o.Id, ' created') AS Title,
                        CONCAT('Order created with total amount ', o.TotalAmount) AS Description,
                        o.Id AS OrderId,
                        o.TotalAmount AS Amount,
                        NULL AS AttachmentId,
                        NULL AS FileName
                    FROM Orders AS o
                    WHERE o.CustomerId = {customerId}
                    
                    UNION ALL

                    SELECT
                        a.CreatedAt AS EventDate,
                        'AttachmentUploaded' AS EventType,
                        'Attachment uploaded' AS Title,
                        CONCAT('File ', a.OriginalFileName, ' uploaded for order #', o.Id) AS Description,
                        o.Id AS OrderId,
                        o.TotalAmount AS Amount,
                        a.Id AS AttachmentId,
                        a.OriginalFileName AS FileName
                    FROM OrderAttachments AS a
                    JOIN Orders AS o
                        ON a.OrderId = o.Id
                    WHERE o.CustomerId = {customerId}
                ) as timeline
            """)
            .SingleAsync(cancellationToken);
        
        List<CustomerTimelineEvent> result = [];

        if (totalCount > 0)
        {
            result = await context.Database
            .SqlQuery<CustomerTimelineEvent>($"""
                SELECT *
                FROM (
                    SELECT 
                        o.CreatedAt AS EventDate,
                        'OrderCreated' AS EventType,
                        CONCAT('Order #', o.Id, ' created') AS Title,
                        CONCAT('Order created with total amount ', o.TotalAmount) AS Description,
                        o.Id AS OrderId,
                        o.TotalAmount AS Amount,
                        NULL AS AttachmentId,
                        NULL AS FileName
                    FROM Orders AS o
                    WHERE o.CustomerId = {customerId}
                    
                    UNION ALL

                    SELECT
                        a.CreatedAt AS EventDate,
                        'AttachmentUploaded' AS EventType,
                        'Attachment uploaded' AS Title,
                        CONCAT('File ', a.OriginalFileName, ' uploaded for order #', o.Id) AS Description,
                        o.Id AS OrderId,
                        o.TotalAmount AS Amount,
                        a.Id AS AttachmentId,
                        a.OriginalFileName AS FileName
                    FROM OrderAttachments AS a
                    JOIN Orders AS o
                        ON a.OrderId = o.Id
                    WHERE o.CustomerId = {customerId}
                ) as timeline
                ORDER BY EventDate DESC
                OFFSET {queryParams.Skip} ROWS
                FETCH NEXT {queryParams.PageSize} ROWS ONLY
            """)
            .ToListAsync(cancellationToken);
        }

        return new PagedResponse<CustomerTimelineEvent>(
            result,
            queryParams.Page,
            queryParams.PageSize,
            totalCount
        );
    }
}