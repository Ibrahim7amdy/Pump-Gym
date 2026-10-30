using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Pump.Application.Common.Exceptions;
using Pump.Application.Dtos.Home;
using Pump.Application.Interfaces.Home;
using System.IdentityModel.Tokens.Jwt;

namespace Pump.Api.Controllers;

[Authorize]
[ApiController]
[Route("api/home")]
public sealed class HomeController : ControllerBase
{
    private readonly IHomeDashboardService _service;

    public HomeController(IHomeDashboardService service) => _service = service;

    [HttpGet]
    public async Task<ActionResult<HomeDashboardDto>> Get(CancellationToken ct)
    {
        var claim = User.FindFirst(JwtRegisteredClaimNames.Sub)
            ?? throw new UnauthorizedException("User id claim not found in token.");

        if (!int.TryParse(claim.Value, out var userId))
            throw new UnauthorizedException("User id claim is invalid.");

        return Ok(await _service.GetAsync(userId, ct));
    }
}
