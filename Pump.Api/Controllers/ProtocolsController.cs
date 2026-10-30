using FluentValidation;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http;
using Pump.Application.Common.Exceptions;
using Pump.Application.Dtos.Common;
using Pump.Application.Dtos.Protocols;
using Pump.Application.Interfaces.Protocols;
using Pump.Api.Models.Protocols;
using Pump.Api.Models.Uploads;
using System.IdentityModel.Tokens.Jwt;

namespace Pump.Api.Controllers;

[ApiController]
[Authorize]
[Route("api/protocols")]
public class ProtocolsController : ControllerBase
{
    private readonly IProtocolService _service;
    private readonly IValidator<CreateProtocolRequestDto> _createValidator;
    private readonly IValidator<UpdateProtocolRequestDto> _updateValidator;
    private readonly IValidator<AssignProtocolRequestDto> _assignValidator;
    private readonly IValidator<ImageUploadRequestDto> _imageUploadValidator;

    public ProtocolsController(IProtocolService service, IValidator<CreateProtocolRequestDto> createValidator, IValidator<UpdateProtocolRequestDto> updateValidator, IValidator<AssignProtocolRequestDto> assignValidator, IValidator<ImageUploadRequestDto> imageUploadValidator)
    {
        _service = service;
        _createValidator = createValidator;
        _updateValidator = updateValidator;
        _assignValidator = assignValidator;
        _imageUploadValidator = imageUploadValidator;
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

    [HttpGet]
    public async Task<ActionResult<List<ProtocolListItemDto>>> GetAll(CancellationToken ct)
        => Ok(await _service.GetAllAsync(ct));

    [HttpGet("{id}")]
    public async Task<ActionResult<ProtocolDetailDto>> GetById(int id, CancellationToken ct)
        => Ok(await _service.GetByIdAsync(id, ct));

    [HttpGet("active")]
    public async Task<ActionResult<ActiveProtocolDto>> GetActive(CancellationToken ct)
        => Ok(await _service.GetActiveAsync(CurrentUserId, ct));

    [HttpGet("completed")]
    public async Task<ActionResult<List<ProtocolAchievementDto>>> GetCompleted(CancellationToken ct)
        => Ok(await _service.GetCompletedAsync(CurrentUserId, ct));

    [Authorize(Policy = "RequireAdmin")]
    [HttpPost]
    [Consumes("multipart/form-data")]
    public async Task<ActionResult<ProtocolDetailDto>> Create([FromForm] CreateProtocolFormRequest form, CancellationToken ct)
    {
        await ValidateImageAsync(form.Image, ct);
        var request = form.ToDto();
        var result = await _createValidator.ValidateAsync(request, ct);
        if (!result.IsValid) throw new ValidationAppException(result.ToDictionary());

        await using var stream = form.Image?.OpenReadStream();
        return Ok(await _service.CreateAsync(CurrentUserId, request, stream, form.Image?.ContentType, ct));
    }

    [Authorize(Policy = "RequireAdmin")]
    [HttpPut("{id}")]
    [Consumes("multipart/form-data")]
    public async Task<ActionResult<ProtocolDetailDto>> Update(int id, [FromForm] UpdateProtocolFormRequest form, CancellationToken ct)
    {
        await ValidateImageAsync(form.Image, ct);
        var request = form.ToDto();
        var result = await _updateValidator.ValidateAsync(request, ct);
        if (!result.IsValid) throw new ValidationAppException(result.ToDictionary());

        await using var stream = form.Image?.OpenReadStream();
        return Ok(await _service.UpdateAsync(id, request, stream, form.Image?.ContentType, ct));
    }

    [Authorize(Policy = "RequireAdmin")]
    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(int id, CancellationToken ct)
    {
        await _service.DeleteAsync(id, ct);
        return NoContent();
    }

    [Authorize(Policy = "RequireAdmin")]
    [HttpPost("{id}/assign")]
    public async Task<IActionResult> Assign(int id, AssignProtocolRequestDto request, CancellationToken ct)
    {
        var result = await _assignValidator.ValidateAsync(request, ct);
        if (!result.IsValid) throw new ValidationAppException(result.ToDictionary());

        await _service.AssignAsync(id, CurrentUserId, request, ct);
        return NoContent();
    }

    private async Task ValidateImageAsync(IFormFile? file, CancellationToken ct)
    {
        if (file is null)
            return;

        var request = await ImageUploadRequestFactory.CreateAsync(file, ct);
        var result = await _imageUploadValidator.ValidateAsync(request, ct);
        if (!result.IsValid)
            throw new ValidationAppException(result.ToDictionary());
    }
}
