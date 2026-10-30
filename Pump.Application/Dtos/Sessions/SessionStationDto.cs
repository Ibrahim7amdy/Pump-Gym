
namespace Pump.Application.Dtos.Sessions
{
    public record SessionStationDto(
    int Id,
    int MachineId,
    string MachineName,
    string MuscleGroup,            
    int TargetSets,
    int RestSeconds,
    int SortOrder,
    List<SessionSetDto> Sets);
}
