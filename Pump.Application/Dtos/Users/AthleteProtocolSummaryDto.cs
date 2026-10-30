namespace Pump.Application.Dtos.Users;

public record AthleteProtocolSummaryDto(
    int AssignmentId,
    int ProtocolId,
    string ProtocolTitle,
    DateOnly AssignedOnUtc,
    DateOnly ExpiresOnUtc,
    decimal PaidPrice);
