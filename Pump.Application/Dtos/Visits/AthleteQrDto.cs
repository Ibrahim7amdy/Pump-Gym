namespace Pump.Application.Dtos.Visits;

public record AthleteQrDto(
    int Id,
    string Username,
    string DisplayName,
    string? AvatarUrl,
    string Payload,
    bool IsCheckedIn);
