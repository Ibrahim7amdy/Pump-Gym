using Pump.Application.Dtos.Visits;

namespace Pump.Application.Interfaces.Visits;

public interface IVisitEventPublisher
{
    Task PublishCheckedInAsync(VisitDto visit, CancellationToken ct = default);
    Task PublishCheckedOutAsync(VisitDto visit, CancellationToken ct = default);
}
