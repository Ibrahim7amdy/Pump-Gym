using FluentValidation;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Pump.Application.Common.Exceptions;
using Pump.Application.Dtos.Visits;
using Pump.Application.Interfaces.Visits;
using System.Globalization;
using System.IdentityModel.Tokens.Jwt;

namespace Pump.Api.Controllers;

[Authorize]
[ApiController]
[Route("api/visits")]
public class VisitsController : ControllerBase
{
    private readonly IVisitService _visitService;
    private readonly IValidator<CheckInRequestDto> _checkInValidator;
    private readonly IValidator<CreateWalkInVisitRequestDto> _walkInValidator;

    public VisitsController(
        IVisitService visitService,
        IValidator<CheckInRequestDto> checkInValidator,
        IValidator<CreateWalkInVisitRequestDto> walkInValidator)
    {
        _visitService = visitService;
        _checkInValidator = checkInValidator;
        _walkInValidator = walkInValidator;
    }

    [HttpPost("check-in")]
    public async Task<ActionResult<VisitDto>> CheckIn(CheckInRequestDto request, CancellationToken ct)
    {
        var validation = await _checkInValidator.ValidateAsync(request, ct);
        if (!validation.IsValid)
            throw new ValidationAppException(validation.ToDictionary());

        var visit = await _visitService.CheckInAsync(CurrentUserId, User.IsInRole("Admin"), request, ct);
        return StatusCode(StatusCodes.Status201Created, visit);
    }

    [HttpPost("walk-in")]
    [Authorize(Policy = "RequireAdmin")]
    public async Task<ActionResult<VisitDto>> CreateWalkIn(CreateWalkInVisitRequestDto request, CancellationToken ct)
    {
        var validation = await _walkInValidator.ValidateAsync(request, ct);
        if (!validation.IsValid)
            throw new ValidationAppException(validation.ToDictionary());

        var visit = await _visitService.CreateWalkInAsync(request, CurrentUserId, ct);
        return StatusCode(StatusCodes.Status201Created, visit);
    }

    [HttpPost("{id:int}/check-out")]
    [Authorize(Policy = "RequireAdmin")]
    public async Task<ActionResult<VisitDto>> CheckOut(int id, CancellationToken ct)
        => Ok(await _visitService.CheckOutAsync(id, CurrentUserId, ct));

    [HttpGet("me/current")]
    public async Task<ActionResult<VisitDto>> GetMyCurrent(CancellationToken ct)
        => Ok(await _visitService.GetMyCurrentAsync(CurrentUserId, ct));

    [HttpGet("me")]
    public async Task<ActionResult<VisitPageDto>> GetMyHistory(
        [FromQuery] DateOnly? date,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 30,
        CancellationToken ct = default)
        => Ok(await _visitService.GetMyHistoryAsync(CurrentUserId, date, page, pageSize, ct));

    [HttpGet("me/dates-with-activity")]
    public async Task<ActionResult<List<DateOnly>>> GetMyDatesWithActivity([FromQuery] string month, CancellationToken ct)
        => Ok(await _visitService.GetDatesWithActivityAsync(ParseMonth(month), CurrentUserId, ct));

    [HttpGet("occupancy")]
    public async Task<ActionResult<OccupancyDto>> GetOccupancy(CancellationToken ct)
        => Ok(await _visitService.GetOccupancyAsync(ct));

    [HttpGet("current")]
    [Authorize(Policy = "RequireAdmin")]
    public async Task<ActionResult<List<VisitDto>>> GetCurrent(CancellationToken ct)
        => Ok(await _visitService.GetCurrentAsync(ct));

    [HttpGet("dates-with-activity")]
    [Authorize(Policy = "RequireAdmin")]
    public async Task<ActionResult<List<DateOnly>>> GetDatesWithActivity(
        [FromQuery] string month,
        [FromQuery] int? userId,
        CancellationToken ct)
        => Ok(await _visitService.GetDatesWithActivityAsync(ParseMonth(month), userId, ct));

    [HttpGet]
    [Authorize(Policy = "RequireAdmin")]
    public async Task<ActionResult<VisitPageDto>> GetHistory(
        [FromQuery] DateOnly? date,
        [FromQuery] int? userId,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 30,
        CancellationToken ct = default)
    {
        if (date is null)
        {
            throw new ValidationAppException(new Dictionary<string, string[]>
            {
                ["date"] = ["A date in yyyy-MM-dd format is required."]
            });
        }

        return Ok(await _visitService.GetHistoryAsync(date.Value, page, pageSize, userId, ct));
    }

    private int CurrentUserId
    {
        get
        {
            var claim = User.FindFirst(JwtRegisteredClaimNames.Sub)
                ?? throw new UnauthorizedException("User id claim not found in token.");
            return int.Parse(claim.Value);
        }
    }

    private static DateOnly ParseMonth(string? month)
    {
        if (DateOnly.TryParseExact($"{month}-01", "yyyy-MM-dd", CultureInfo.InvariantCulture,
                DateTimeStyles.None, out var parsed))
            return parsed;

        throw new ValidationAppException(new Dictionary<string, string[]>
        {
            ["month"] = ["Month must use yyyy-MM format."]
        });
    }
}
