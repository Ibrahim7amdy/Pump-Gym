
namespace Pump.Application.Dtos.Protocols
{
    public record ProtocolListItemDto(
    int Id,
    string Title,
    string Category,
    string Difficulty,
    decimal Price,
    int DurationDays,
    string? ExpectedGainLabel,
    string? ExpectedMuscleGainLabel,
    string? ExpectedStrengthGainLabel,
    int? ExpectedPoints,
    string? ImageUrl,
    string? CoachName);
}
