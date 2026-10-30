using FluentValidation;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Pump.Application.Common.Exceptions;
using Pump.Application.Dtos.Machines;
using Pump.Application.Dtos.Common;
using Pump.Application.Interfaces.Machines;
using Pump.Api.Models.Machines;
using Pump.Api.Models.Uploads;
using System.IdentityModel.Tokens.Jwt;

namespace Pump.Api.Controllers;

[ApiController]
[Route("api/machines")]
public class MachinesController : ControllerBase
{
    private readonly IMachineService _service;
    private readonly IValidator<CreateMachineRequestDto> _createValidator;
    private readonly IValidator<UpdateMachineRequestDto> _updateValidator;

    public MachinesController(
        IMachineService service,
        IValidator<CreateMachineRequestDto> createValidator,
        IValidator<UpdateMachineRequestDto> updateValidator)
    {
        _service = service;
        _createValidator = createValidator;
        _updateValidator = updateValidator;
    }

    // بيرجع الـ userId لو فيه توكن صحيح، أو null لو زائر — من غير ما يرفض الـ request
    private int? CurrentUserIdOrNull()
    {
        var claim = User.FindFirst(JwtRegisteredClaimNames.Sub);
        return claim is not null ? int.Parse(claim.Value) : null;
    }

    [HttpGet]
    public async Task<ActionResult<List<MachineDto>>> GetAll([FromQuery] string? muscleGroup, CancellationToken ct)
        => Ok(await _service.GetAllAsync(muscleGroup, CurrentUserIdOrNull(), ct));

    [HttpGet("{id}")]
    public async Task<ActionResult<MachineDto>> GetById(int id, CancellationToken ct)
        => Ok(await _service.GetByIdAsync(id, CurrentUserIdOrNull(), ct));

    [Authorize(Policy = "RequireAdmin")]
    [HttpPost]
    [Consumes("multipart/form-data")]
    public async Task<ActionResult<MachineDto>> Create([FromForm] CreateMachineFormRequest form, CancellationToken ct)
    {
        var imageMetadata = form.Image is null
            ? null
            : await ImageUploadRequestFactory.CreateAsync(form.Image, ct);
        var request = form.ToDto(imageMetadata);
        var result = await _createValidator.ValidateAsync(request, ct);
        if (!result.IsValid) throw new ValidationAppException(result.ToDictionary());

        await using var stream = form.Image?.OpenReadStream();
        return Ok(await _service.CreateAsync(request, stream, form.Image?.ContentType, ct));
    }

    [Authorize(Policy = "RequireAdmin")]
    [HttpPut("{id}")]
    [Consumes("multipart/form-data")]
    public async Task<ActionResult<MachineDto>> Update(int id, [FromForm] UpdateMachineFormRequest form, CancellationToken ct)
    {
        var imageMetadata = form.Image is null
            ? null
            : await ImageUploadRequestFactory.CreateAsync(form.Image, ct);
        var request = form.ToDto(imageMetadata);
        var result = await _updateValidator.ValidateAsync(request, ct);
        if (!result.IsValid) throw new ValidationAppException(result.ToDictionary());

        await using var stream = form.Image?.OpenReadStream();
        return Ok(await _service.UpdateAsync(id, request, stream, form.Image?.ContentType, ct));
    }

    [Authorize(Policy = "RequireAdmin")]
    [HttpPatch("{id}/toggle-active")]
    public async Task<ActionResult<bool>> ToggleActive(int id, CancellationToken ct)
    => Ok(await _service.ToggleActiveAsync(id, ct));
}
