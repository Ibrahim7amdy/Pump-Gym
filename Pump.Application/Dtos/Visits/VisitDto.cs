namespace Pump.Application.Dtos.Visits;

public record VisitDto(
    int Id,
    int? UserId,
    string? Username,
    string DisplayName,
    DateTime CheckInAtUtc,
    DateTime? CheckOutAtUtc,
    string Source,
    string VisitorType,
    bool IsRegisteredAthlete,
    bool IsCurrentlyInside,
    int? CheckedInByStaffUserId,
    int? CheckedOutByStaffUserId);
