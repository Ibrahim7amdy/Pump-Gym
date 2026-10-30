namespace Pump.Domain.Entities;

public class AnalyticsDailySnapshot
{
    public int Id { get; set; }

    public DateOnly SnapshotDateUtc { get; set; }
    public decimal TotalRevenue { get; set; }
    public int NewMembers { get; set; }
    public int ActiveMembers { get; set; }
    public int TotalVisits { get; set; }
    public decimal AvgOccupancy { get; set; }
}