namespace Pump.Application.Dtos.Visits;

public record AthleteQrPageDto(
    List<AthleteQrDto> Items,
    int Page,
    int PageSize,
    int TotalCount,
    int TotalPages);
