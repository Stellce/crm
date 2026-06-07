using Application.Abstractions;
using Application.DTOs;

namespace Application.Services;

public class ReportService(
    IReportsReadDb reportsReadDb
)
{
    public Task<PagedResponse<CustomerSalesReportRow>> GetCustomerSalesAsync(
        CustomerSalesReportQueryParameters queryParams,
        CancellationToken cancellationToken)
    {
        return reportsReadDb.GetCustomerSalesAsync(
            queryParams,
            cancellationToken
        );
    }

    public Task<PagedResponse<CustomerTimelineEvent>> GetCustomerTimelineAsync(
        int customerId,
        CustomerTimelineReportQueryParameters queryParams,
        CancellationToken cancellationToken
    )
    {
        return reportsReadDb.GetCustomerTimelineAsync(customerId, queryParams, cancellationToken);
    }
}