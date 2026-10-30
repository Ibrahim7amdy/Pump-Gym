
namespace Pump.Application.Dtos.Sessions
{
    public record SessionSetDto(
        int Id,
        int SetNumber,
        decimal WeightKg,
        int Reps,
        DateTime LoggedAtUtc);
}
