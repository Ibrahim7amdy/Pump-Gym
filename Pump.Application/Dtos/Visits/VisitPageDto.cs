namespace Pump.Application.Dtos.Visits;

public record VisitPageDto(
    List<VisitDto> Items,
    int Page,
    int PageSize,
    int TotalCount,
    int TotalPages);
