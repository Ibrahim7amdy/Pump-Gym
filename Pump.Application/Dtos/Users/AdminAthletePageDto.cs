namespace Pump.Application.Dtos.Users;

public record AdminAthletePageDto(
    List<AdminAthleteDto> Items,
    int Page,
    int PageSize,
    int TotalCount,
    int TotalPages);
