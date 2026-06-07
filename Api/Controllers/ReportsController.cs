using Api.Security;
using Application.DTOs;
using Application.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;

namespace Api.Controllers;


[Authorize(AppPolicies.ManageReports)]
[EnableRateLimiting(RateLimitPolicies.UserApi)]
[Route("api/reports")]
[ApiController]
public class ReportsController(
    ReportService reportService
) : ControllerBase
{
    [HttpGet("customer-sales")]
    public async Task<ActionResult<PagedResponse<CustomerSalesReportRow>>> GetCustomerSalesReport(
        [FromQuery] CustomerSalesReportQueryParameters queryParams,
        CancellationToken cancellationToken)
    {
        return Ok(await reportService.GetCustomerSalesAsync(queryParams, cancellationToken));
    }

    [HttpGet("customers/{customerId:int}/timeline")]
    public async Task<ActionResult<PagedResponse<CustomerTimelineEvent>>> GetCustomerTimelineReport(
        [FromRoute] int customerId,
        [FromQuery] CustomerTimelineReportQueryParameters queryParams,
        CancellationToken cancellationToken)
    {
        return Ok(await reportService.GetCustomerTimelineAsync(customerId, queryParams, cancellationToken));
    }
}