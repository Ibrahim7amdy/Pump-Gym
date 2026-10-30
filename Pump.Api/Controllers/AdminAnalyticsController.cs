using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Pump.Application.Dtos.Analytics;
using Pump.Application.Interfaces.Analytics;

namespace Pump.Api.Controllers;

[ApiController]
[Authorize(Policy = "RequireAdmin")]
[Route("api/admin/analytics")]
public sealed class AdminAnalyticsController : ControllerBase
{
    private readonly IAdminAnalyticsService _service;

    public AdminAnalyticsController(IAdminAnalyticsService service) => _service = service;

    [HttpGet]
    public async Task<ActionResult<AdminAnalyticsDashboardDto>> GetDashboard(CancellationToken ct)
        => Ok(await _service.GetDashboardAsync(ct));
}
