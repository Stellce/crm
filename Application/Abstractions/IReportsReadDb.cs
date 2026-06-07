using Application.DTOs;

namespace Application.Abstractions;

public interface IReportsReadDb
{
    Task<PagedResponse<CustomerSalesReportRow>> GetCustomerSalesAsync(
        CustomerSalesReportQueryParameters queryParams, 
        CancellationToken cancellationToken);

    Task<PagedResponse<CustomerTimelineEvent>> GetCustomerTimelineAsync(
        int customerId,
        CustomerTimelineReportQueryParameters queryParams,
        CancellationToken cancellationToken);
}