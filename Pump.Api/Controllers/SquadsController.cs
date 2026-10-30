using FluentValidation;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Pump.Application.Common.Exceptions;
using Pump.Application.Dtos.Squads;
using Pump.Application.Interfaces.Squads;
using System.IdentityModel.Tokens.Jwt;

namespace Pump.Api.Controllers;

[Authorize(Roles = "User")]
[ApiController]
[Route("api/squads")]
public sealed class SquadsController : ControllerBase
{
    private readonly ISquadService _service;
    private readonly IValidator<CreateSquadRequestDto> _createValidator;
    private readonly IValidator<UpdateSquadRequestDto> _updateValidator;
    private readonly IValidator<JoinSquadRequestDto> _joinValidator;
    private readonly IValidator<AddSquadMemberRequestDto> _addMemberValidator;

    public SquadsController(
        ISquadService service,
        IValidator<CreateSquadRequestDto> createValidator,
        IValidator<UpdateSquadRequestDto> updateValidator,
        IValidator<JoinSquadRequestDto> joinValidator,
        IValidator<AddSquadMemberRequestDto> addMemberValidator)
    {
        _service = service;
        _createValidator = createValidator;
        _updateValidator = updateValidator;
        _joinValidator = joinValidator;
        _addMemberValidator = addMemberValidator;
    }

    [HttpGet]
    public async Task<ActionResult<IReadOnlyList<SquadDto>>> GetMine(CancellationToken ct)
        => Ok(await _service.GetForUserAsync(CurrentUserId, ct));

    [HttpGet("{squadId:int}")]
    public async Task<ActionResult<SquadDto>> GetById(int squadId, CancellationToken ct)
        => Ok(await _service.GetMemberViewAsync(squadId, CurrentUserId, ct));

    [HttpPost]
    public async Task<ActionResult<SquadDto>> Create(CreateSquadRequestDto request, CancellationToken ct)
    {
        await ValidateAsync(_createValidator, request, ct);
        var squad = await _service.CreateAsync(CurrentUserId, request, ct);
        return CreatedAtAction(nameof(GetById), new { squadId = squad.Id }, squad);
    }

    [HttpPost("join")]
    public async Task<ActionResult<SquadDto>> Join(JoinSquadRequestDto request, CancellationToken ct)
    {
        await ValidateAsync(_joinValidator, request, ct);
        return Ok(await _service.JoinAsync(CurrentUserId, request.InviteCode, ct));
    }

    [HttpPut("{squadId:int}")]
    public async Task<ActionResult<SquadDto>> UpdateName(int squadId, UpdateSquadRequestDto request, CancellationToken ct)
    {
        await ValidateAsync(_updateValidator, request, ct);
        return Ok(await _service.UpdateNameAsync(squadId, CurrentUserId, request.Name, ct));
    }

    [HttpDelete("{squadId:int}")]
    public async Task<IActionResult> Delete(int squadId, CancellationToken ct)
    {
        await _service.DeleteAsync(squadId, CurrentUserId, ct);
        return NoContent();
    }

    [HttpPost("{squadId:int}/members")]
    public async Task<ActionResult<SquadDto>> AddMember(int squadId, AddSquadMemberRequestDto request, CancellationToken ct)
    {
        await ValidateAsync(_addMemberValidator, request, ct);
        return StatusCode(StatusCodes.Status201Created,
            await _service.AddMemberAsync(squadId, CurrentUserId, request, ct));
    }

    [HttpDelete("{squadId:int}/members/{userId:int}")]
    public async Task<IActionResult> RemoveMember(int squadId, int userId, CancellationToken ct)
    {
        await _service.RemoveMemberAsync(squadId, CurrentUserId, userId, ct);
        return NoContent();
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

    private static async Task ValidateAsync<T>(IValidator<T> validator, T request, CancellationToken ct)
    {
        var result = await validator.ValidateAsync(request, ct);
        if (!result.IsValid) throw new ValidationAppException(result.ToDictionary());
    }
}
