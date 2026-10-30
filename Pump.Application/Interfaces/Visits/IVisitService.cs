using Pump.Application.Dtos.Visits;

namespace Pump.Application.Interfaces.Visits;

public interface IVisitService
{
    Task<QrCodeDto> GetMyQrAsync(int userId, CancellationToken ct = default);
    QrCodeDto GetGymQr();
    Task<AthleteQrPageDto> SearchAthleteQrsAsync(string? search, int page, int pageSize, CancellationToken ct = default);
    Task<VisitDto> CheckInAsync(int callerId, bool isAdmin, CheckInRequestDto request, CancellationToken ct = default);
    Task<VisitDto> CreateWalkInAsync(CreateWalkInVisitRequestDto request, int adminUserId, CancellationToken ct = default);
    Task<VisitDto> CheckOutAsync(int visitId, int adminUserId, CancellationToken ct = default);
    Task<VisitDto> GetMyCurrentAsync(int userId, CancellationToken ct = default);
    Task<List<VisitDto>> GetCurrentAsync(CancellationToken ct = default);
    Task<OccupancyDto> GetOccupancyAsync(CancellationToken ct = default);
    Task<VisitPageDto> GetMyHistoryAsync(int userId, DateOnly? date, int page, int pageSize, CancellationToken ct = default);
    Task<VisitPageDto> GetHistoryAsync(DateOnly date, int page, int pageSize, int? userId = null, CancellationToken ct = default);
    Task<List<DateOnly>> GetDatesWithActivityAsync(DateOnly month, int? userId, CancellationToken ct = default);
}
