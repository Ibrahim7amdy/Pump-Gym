using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using Pump.Api.Hubs;
using Pump.Application.Dtos.Visits;
using Pump.Application.Interfaces.Common;
using Pump.Application.Interfaces.Visits;

namespace Pump.Api.Services;

public class VisitEventPublisher : IVisitEventPublisher
{
    private readonly IHubContext<OccupancyHub> _hub;
    private readonly IApplicationDbContext _db;
    private readonly ILogger<VisitEventPublisher> _logger;

    public VisitEventPublisher(
        IHubContext<OccupancyHub> hub,
        IApplicationDbContext db,
        ILogger<VisitEventPublisher> logger)
    {
        _hub = hub;
        _db = db;
        _logger = logger;
    }

    public async Task PublishCheckedInAsync(VisitDto visit, CancellationToken ct = default)
    {
        await TryPublishAsync("VisitorCheckedIn", () => _hub.Clients.Group(OccupancyHub.AdminsGroup)
            .SendAsync("VisitorCheckedIn", visit, ct));

        if (visit.UserId is int userId)
            await TryPublishAsync("VisitorCheckedIn", () => _hub.Clients.Group(OccupancyHub.UserGroup(userId))
                .SendAsync("VisitorCheckedIn", visit, ct));

        await PublishOccupancyAsync(ct);
    }

    public async Task PublishCheckedOutAsync(VisitDto visit, CancellationToken ct = default)
    {
        await TryPublishAsync("VisitorCheckedOut", () => _hub.Clients.Group(OccupancyHub.AdminsGroup)
            .SendAsync("VisitorCheckedOut", visit, ct));

        if (visit.UserId is int userId)
            await TryPublishAsync("VisitorCheckedOut", () => _hub.Clients.Group(OccupancyHub.UserGroup(userId))
                .SendAsync("VisitorCheckedOut", visit, ct));

        await PublishOccupancyAsync(ct);
    }

    private async Task PublishOccupancyAsync(CancellationToken ct)
    {
        try
        {
            var count = await _db.Visits.CountAsync(visit => visit.CheckOutAtUtc == null, ct);
            await _hub.Clients.All.SendAsync("OccupancyUpdated", new OccupancyDto(count), ct);
        }
        catch (Exception exception)
        {
            _logger.LogWarning(exception, "Failed to publish the latest gym occupancy.");
        }
    }

    private async Task TryPublishAsync(string eventName, Func<Task> publish)
    {
        try
        {
            await publish();
        }
        catch (Exception exception)
        {
            _logger.LogWarning(exception, "Failed to publish SignalR event {EventName}.", eventName);
        }
    }
}
