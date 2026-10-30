using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Pump.Application.Dtos.Squads;
using Pump.Application.Interfaces.Squads;

namespace Pump.Api.Controllers;

[Authorize(Policy = "RequireAdmin")]
[ApiController]
[Route("api/admin/squads")]
public sealed class AdminSquadsController : ControllerBase
{
    private readonly ISquadService _service;
    public AdminSquadsController(ISquadService service) => _service = service;

    [HttpGet]
    public async Task<ActionResult<IReadOnlyList<SquadDto>>> GetAll(CancellationToken ct)
        => Ok(await _service.GetAllAsync(ct));

    [HttpGet("{squadId:int}")]
    public async Task<ActionResult<SquadDto>> GetById(int squadId, CancellationToken ct)
        => Ok(await _service.GetByIdAsync(squadId, ct));

}
