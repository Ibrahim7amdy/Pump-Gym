using FluentValidation;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Pump.Application.Common.Exceptions;
using Pump.Application.Dtos.Users;
using Pump.Application.Interfaces.Users;

namespace Pump.Api.Controllers;

[Authorize]
[ApiController]
[Route("api/leaderboard")]
public sealed class LeaderboardController : ControllerBase
{
    private readonly ILeaderboardService _service;
    private readonly IValidator<LeaderboardQueryDto> _validator;

    public LeaderboardController(ILeaderboardService service, IValidator<LeaderboardQueryDto> validator)
    {
        _service = service;
        _validator = validator;
    }

    [HttpGet]
    public async Task<ActionResult<IReadOnlyList<LeaderboardItemDto>>> Get(
        [FromQuery] string? tier = null,
        [FromQuery] string timeframe = "alltime",
        CancellationToken ct = default)
    {
        var request = new LeaderboardQueryDto(tier, timeframe.Trim());
        var validation = await _validator.ValidateAsync(request, ct);
        if (!validation.IsValid)
            throw new ValidationAppException(validation.ToDictionary());

        return Ok(await _service.GetAsync(request.Tier, request.Timeframe, ct));
    }
}
