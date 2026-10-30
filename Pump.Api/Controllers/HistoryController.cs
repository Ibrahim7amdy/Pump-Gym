using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Pump.Application.Dtos.Sessions;
using Pump.Application.Interfaces.Sessions;
using System.IdentityModel.Tokens.Jwt;

namespace Pump.Api.Controllers;

[Authorize]
[ApiController]
[Route("api/history")]
public class HistoryController : ControllerBase
{
    private readonly ISessionService _sessions;

    public HistoryController(ISessionService sessions) => _sessions = sessions;

    [HttpGet]
    public async Task<ActionResult<WorkoutHistoryDto>> Get(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20,
        CancellationToken ct = default)
    {
        var claim = User.FindFirst(JwtRegisteredClaimNames.Sub)
            ?? throw new Pump.Application.Common.Exceptions.UnauthorizedException("User id claim not found in token.");
        return Ok(await _sessions.GetHistoryAsync(int.Parse(claim.Value), page, pageSize, ct));
    }
}
