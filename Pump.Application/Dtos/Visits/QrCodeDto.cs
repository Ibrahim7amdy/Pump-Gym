namespace Pump.Application.Dtos.Visits;

public record QrCodeDto(string Payload, string? GymName = null, DateTime? ExpiresAtUtc = null);
