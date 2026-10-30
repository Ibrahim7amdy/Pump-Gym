using Microsoft.EntityFrameworkCore;
using Pump.Application.Common.Exceptions;
using Pump.Application.Dtos.Machines;
using Pump.Application.Interfaces.Common;
using Pump.Application.Interfaces.Machines;
using Pump.Domain.Entities;


namespace Pump.Application.Services.Machines;
public class MachineService : IMachineService
{
    private readonly IApplicationDbContext _db;
    private readonly IFileStorageService _fileStorage;

    public MachineService(IApplicationDbContext db, IFileStorageService fileStorage)
    {
        _db = db;
        _fileStorage = fileStorage;
    }

    public async Task<List<MachineDto>> GetAllAsync(string? muscleGroup, int? currentUserId, CancellationToken ct = default)
    {
        var query = _db.Machines.Where(m => m.IsActive);

        if (!string.IsNullOrWhiteSpace(muscleGroup) && muscleGroup != "ALL")
            query = query.Where(m => m.MuscleGroup == muscleGroup);

        var machines = await query
            .Include(m => m.Alternatives).ThenInclude(a => a.AlternativeMachine)
            .ToListAsync(ct);

        Dictionary<int, (decimal LastWeightKg, decimal PersonalRecordKg)> userSummaryByMachine = new();

        if (currentUserId is not null)
        {
            var machineIds = machines
                .Select(m => m.Id)
                .ToList();

            userSummaryByMachine = await _db.SessionSets
                .Where(s => s.SessionStation.Session.UserId == currentUserId.Value && machineIds.Contains(s.SessionStation.MachineId))
                .GroupBy(s => s.SessionStation.MachineId)
                .Select(g => new
                {
                    MachineId = g.Key,
                    PersonalRecordKg = g.Max(s => s.WeightKg),
                    LastWeightKg = g.OrderByDescending(s => s.LoggedAtUtc).Select(s => s.WeightKg).First()
                })
                .ToDictionaryAsync(x => x.MachineId, x => (x.LastWeightKg, x.PersonalRecordKg), ct);
        }

        return machines.Select(m =>
        {
            userSummaryByMachine.TryGetValue(m.Id, out var summary);

            return ToDto(m, summary.LastWeightKg, summary.PersonalRecordKg);
        })
        .ToList();
    }

    public async Task<MachineDto> GetByIdAsync(int id, int? currentUserId, CancellationToken ct = default)
    {
        var machine = await _db.Machines
            .Include(m => m.Alternatives).ThenInclude(a => a.AlternativeMachine)
            .FirstOrDefaultAsync(m => m.Id == id, ct)
            ?? throw new NotFoundException("Machine not found.");

        decimal? myPr = null;
        decimal? myLastWeight = null;

        if (currentUserId is not null)
        {
            var summary = await _db.SessionSets
                .Where(s => s.SessionStation.MachineId == id && s.SessionStation.Session.UserId == currentUserId.Value)
                .GroupBy(s => s.SessionStation.MachineId)
                .Select(g => new
                {
                    PersonalRecordKg = g.Max(s => s.WeightKg),
                    LastWeightKg = g.OrderByDescending(s => s.LoggedAtUtc).Select(s => s.WeightKg).First()
                })
                .FirstOrDefaultAsync(ct);

            if (summary is not null)
            {
                myPr = summary.PersonalRecordKg;
                myLastWeight = summary.LastWeightKg;
            }
        }

        return ToDto(machine, myLastWeight, myPr);
    }

    public async Task<MachineDto> CreateAsync(CreateMachineRequestDto request, Stream? imageStream, string? imageContentType, CancellationToken ct = default)
    {
        var alternativeIds = request.AlternativeMachineIds ?? new List<int>();
        await ValidateAlternativeMachinesAsync(alternativeIds, null, ct);

        var pinExists = await _db.Machines.AnyAsync(m => m.PinNumber == request.PinNumber, ct);

        if (pinExists)
            throw new ConflictException("Pin number is already in use.");

        var machine = new Machine
        {
            Name = request.Name,
            MuscleGroup = request.MuscleGroup,
            Category = request.Category,
            ImageUrl = imageStream is not null && imageContentType is not null
                ? await _fileStorage.SaveMachineImageAsync(imageStream, imageContentType, ct)
                : request.ImageUrl!,
            SafetyNotes = request.SafetyNotes,
            ExecutionSteps = request.ExecutionSteps ?? new List<string>(),
            PinNumber = request.PinNumber,
            IsActive = true
        };

        foreach (var alternativeId in alternativeIds)
        {
            machine.Alternatives.Add(new MachineAlternative { AlternativeMachineId = alternativeId });
        }

        _db.Machines.Add(machine);
        await _db.SaveChangesAsync(ct);

        return await GetByIdAsync(machine.Id, null, ct);
    }

    public async Task<MachineDto> UpdateAsync(int id, UpdateMachineRequestDto request, Stream? imageStream, string? imageContentType, CancellationToken ct = default)
    {
        var machine = await _db.Machines
            .FirstOrDefaultAsync(m => m.Id == id, ct)
            ?? throw new NotFoundException("Machine not found.");

        if (request.PinNumber is not null)
        {
            var pinExists = await _db.Machines.AnyAsync(m => m.PinNumber == request.PinNumber.Value && m.Id != id, ct);

            if (pinExists)
                throw new ConflictException("Pin number is already in use.");

            machine.PinNumber = request.PinNumber.Value;
        }

        if (request.Name is not null)
            machine.Name = request.Name;

        if (request.MuscleGroup is not null)
            machine.MuscleGroup = request.MuscleGroup;

        if (request.Category is not null)
            machine.Category = request.Category;

        if (imageStream is not null && imageContentType is not null)
            machine.ImageUrl = await _fileStorage.SaveMachineImageAsync(imageStream, imageContentType, ct);
        else if (request.ImageUrl is not null)
            machine.ImageUrl = request.ImageUrl;

        if (request.SafetyNotes is not null)
            machine.SafetyNotes = request.SafetyNotes;

        if (request.ExecutionSteps is not null)
            machine.ExecutionSteps = request.ExecutionSteps;


        if (request.AlternativeMachineIds is not null)
        {
            await ValidateAlternativeMachinesAsync(request.AlternativeMachineIds, id, ct);

            var existing = await _db.MachineAlternatives
                .Where(a => a.MachineId == id)
                .ToListAsync(ct);

            _db.MachineAlternatives.RemoveRange(existing);

            foreach (var altId in request.AlternativeMachineIds)
            {
                _db.MachineAlternatives.Add(new MachineAlternative
                {
                    MachineId = id,
                    AlternativeMachineId = altId
                });
            }
        }

        await _db.SaveChangesAsync(ct);

        return await GetByIdAsync(machine.Id, null, ct);
    }

    private async Task ValidateAlternativeMachinesAsync(IReadOnlyCollection<int> alternativeIds, int? machineId, CancellationToken ct)
    {
        if (alternativeIds.Any(id => id <= 0) || alternativeIds.Distinct().Count() != alternativeIds.Count)
            throw InvalidAlternativeMachines("Alternative machine IDs must be positive and unique.");

        if (machineId is not null && alternativeIds.Contains(machineId.Value))
            throw InvalidAlternativeMachines("A machine cannot be an alternative to itself.");

        if (alternativeIds.Count == 0)
            return;

        var foundIds = await _db.Machines
            .Where(m => m.IsActive && alternativeIds.Contains(m.Id))
            .Select(m => m.Id)
            .ToListAsync(ct);

        if (foundIds.Count != alternativeIds.Count)
            throw InvalidAlternativeMachines("One or more alternative machines do not exist or are inactive.");
    }

    private static ValidationAppException InvalidAlternativeMachines(string message)
        => new(new Dictionary<string, string[]> { ["AlternativeMachineIds"] = new[] { message } });

    public async Task<bool> ToggleActiveAsync(int id, CancellationToken ct = default)
    {
        var machine = await _db.Machines.FirstOrDefaultAsync(m => m.Id == id, ct)
            ?? throw new NotFoundException("Machine not found.");

        machine.IsActive = !machine.IsActive;
        await _db.SaveChangesAsync(ct);

        return machine.IsActive;
    }

    private static MachineDto ToDto(Machine m, decimal? myLastWeight, decimal? myPr)
    {
        return new MachineDto(
            m.Id,
            m.Name,
            m.MuscleGroup,
            m.Category,
            m.ImageUrl,
            m.SafetyNotes,
            m.ExecutionSteps ?? new List<string>(),
            m.Alternatives.Select(a => new MachineAlternativeDto(a.AlternativeMachineId, a.AlternativeMachine.Name)).ToList(),
            myPr,
            myLastWeight);
    }
}
