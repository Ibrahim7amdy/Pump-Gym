using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Pump.Application.Common.Exceptions;
using Pump.Application.Dtos.Visits;
using Pump.Application.Interfaces.Visits;
using System.IdentityModel.Tokens.Jwt;

namespace Pump.Api.Controllers;

[Authorize]
[ApiController]
[Route("api/qr")]
public class QrController : ControllerBase
{
    private readonly IVisitService _visitService;

    public QrController(IVisitService visitService) => _visitService = visitService;

    [HttpGet("me")]
    public async Task<ActionResult<QrCodeDto>> GetMyQr(CancellationToken ct)
    {
        var userId = GetCurrentUserId();
        return Ok(await _visitService.GetMyQrAsync(userId, ct));
    }

    [Authorize(Policy = "RequireAdmin")]
    [HttpGet("gym")]
    public ActionResult<QrCodeDto> GetGymQr()
        => Ok(_visitService.GetGymQr());

    [Authorize(Policy = "RequireAdmin")]
    [HttpGet("athletes")]
    public async Task<ActionResult<AthleteQrPageDto>> SearchAthletes(
        [FromQuery] string? search,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 25,
        CancellationToken ct = default)
        => Ok(await _visitService.SearchAthleteQrsAsync(search, page, pageSize, ct));

    private int GetCurrentUserId()
    {
        var claim = User.FindFirst(JwtRegisteredClaimNames.Sub)
            ?? throw new UnauthorizedException("User id claim not found in token.");
        return int.Parse(claim.Value);
    }
}
