using FluentValidation;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Pump.Application.Dtos.Analytics;
using Pump.Application.Interfaces.Analytics;

namespace Pump.Api.Controllers;

[ApiController]
[Authorize(Policy = "RequireAdmin")]
[Route("api/admin/plan-builder/drop-in-price")]
public sealed class AdminDropInPricingController : ControllerBase
{
    private readonly IDropInPricingService _service;
    private readonly IValidator<DropInPricingDto> _validator;

    public AdminDropInPricingController(
        IDropInPricingService service,
        IValidator<DropInPricingDto> validator)
    {
        _service = service;
        _validator = validator;
    }

    [HttpGet]
    public async Task<ActionResult<DropInPricingDto>> Get(CancellationToken ct)
        => Ok(await _service.GetAsync(ct));

    [HttpPut]
    public async Task<ActionResult<DropInPricingDto>> Update(DropInPricingDto request, CancellationToken ct)
    {
        var result = await _validator.ValidateAsync(request, ct);
        if (!result.IsValid)
            throw new Pump.Application.Common.Exceptions.ValidationAppException(result.ToDictionary());

        return Ok(await _service.UpdateAsync(request, ct));
    }
}
