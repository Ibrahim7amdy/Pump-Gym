using FluentValidation;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Pump.Application.Common.Exceptions;
using Pump.Application.Dtos.Sessions;
using Pump.Application.Interfaces.Sessions;
using System.IdentityModel.Tokens.Jwt;


namespace Pump.Api.Controllers;

[Authorize]
[ApiController]
[Route("api/sessions")]
public class SessionsController : ControllerBase
{
    private readonly ISessionService _service;
    private readonly IValidator<CreateSessionRequestDto> _createValidator;
    private readonly IValidator<LogSetRequestDto> _logSetValidator;
    private readonly IValidator<AddStationRequestDto> _addStationValidator;
    private readonly IValidator<ReplaceStationMachineRequestDto> _replaceMachineValidator;

    public SessionsController(
        ISessionService service,
        IValidator<CreateSessionRequestDto> createValidator,
        IValidator<LogSetRequestDto> logSetValidator,
        IValidator<AddStationRequestDto> addStationValidator,
        IValidator<ReplaceStationMachineRequestDto> replaceMachineValidator)
    {
        _service = service;
        _createValidator = createValidator;
        _logSetValidator = logSetValidator;
        _addStationValidator = addStationValidator;
        _replaceMachineValidator = replaceMachineValidator;
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

    [HttpPost]
    public async Task<ActionResult<SessionDto>> Start(CreateSessionRequestDto request, CancellationToken ct)
    {
        var result = await _createValidator.ValidateAsync(request, ct);
        if (!result.IsValid) throw new ValidationAppException(result.ToDictionary());

        return Ok(await _service.StartAsync(CurrentUserId, request, ct));
    }

    [HttpGet("current")]
    public async Task<ActionResult<SessionDto>> GetCurrent(CancellationToken ct)
        => Ok(await _service.GetCurrentAsync(CurrentUserId, ct));

    [HttpGet("{id}")]
    public async Task<ActionResult<SessionDto>> GetById(int id, CancellationToken ct)
        => Ok(await _service.GetByIdAsync(id, CurrentUserId, ct));

    [HttpPost("{id}/sets")]
    public async Task<ActionResult<SessionSetDto>> LogSet(int id, LogSetRequestDto request, CancellationToken ct)
    {
        var result = await _logSetValidator.ValidateAsync(request, ct);
        if (!result.IsValid) throw new ValidationAppException(result.ToDictionary());

        return Ok(await _service.LogSetAsync(id, CurrentUserId, request, ct));
    }

    [HttpPost("{id}/stations")]
    public async Task<ActionResult<SessionStationDto>> AddStation(int id, AddStationRequestDto request, CancellationToken ct)
    {
        var result = await _addStationValidator.ValidateAsync(request, ct);
        if (!result.IsValid) throw new ValidationAppException(result.ToDictionary());

        return Ok(await _service.AddStationAsync(id, CurrentUserId, request, ct));
    }

    [HttpPut("{id}/stations/{stationId}/machine")]
    public async Task<ActionResult<SessionStationDto>> ReplaceStationMachine(int id, int stationId, ReplaceStationMachineRequestDto request, CancellationToken ct)
    {
        var result = await _replaceMachineValidator.ValidateAsync(request, ct);
        if (!result.IsValid) throw new ValidationAppException(result.ToDictionary());

        return Ok(await _service.ReplaceStationMachineAsync(id, stationId, CurrentUserId, request, ct));
    }

    [HttpPost("{id}/finish")]
    public async Task<ActionResult<SessionDebriefDto>> Finish(int id, CancellationToken ct)
        => Ok(await _service.FinishAsync(id, CurrentUserId, ct));
}
