using FluentValidation;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Pump.Application.Dtos.Users;
using Pump.Application.Interfaces.Users;

namespace Pump.Api.Controllers;

[Authorize(Policy = "RequireAdmin")]
[ApiController]
[Route("api/admin/athletes")]
public class AdminAthletesController : ControllerBase
{
    private readonly IAdminAthleteService _service;
    private readonly IValidator<UpdateAthleteCoachRequestDto> _coachValidator;

    public AdminAthletesController(
        IAdminAthleteService service,
        IValidator<UpdateAthleteCoachRequestDto> coachValidator)
    {
        _service = service;
        _coachValidator = coachValidator;
    }

    [HttpGet]
    public async Task<ActionResult<AdminAthletePageDto>> GetAthletes(
        [FromQuery] string? search,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 30,
        CancellationToken ct = default)
        => Ok(await _service.GetAthletesAsync(search, page, pageSize, ct));

    [HttpPut("{athleteId:int}/verification")]
    public async Task<ActionResult<AdminAthleteDto>> SetVerification(
        int athleteId,
        SetAthleteVerificationRequestDto request,
        CancellationToken ct)
        => Ok(await _service.SetVerificationAsync(athleteId, request.IsVerified, ct));

    [HttpPut("{athleteId:int}/coach")]
    public async Task<ActionResult<AdminAthleteDto>> UpdateCoach(
        int athleteId,
        UpdateAthleteCoachRequestDto request,
        CancellationToken ct)
    {
        var validation = await _coachValidator.ValidateAsync(request, ct);
        if (!validation.IsValid)
            throw new Pump.Application.Common.Exceptions.ValidationAppException(validation.ToDictionary());

        return Ok(await _service.UpdateCoachAsync(athleteId, request.CoachName, ct));
    }
}
