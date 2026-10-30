namespace Pump.Domain.Entities;

/// <summary>Singleton gym pricing settings. The current application uses row Id = 1.</summary>
public sealed class GymPricingSetting
{
    public int Id { get; set; } = 1;
    public decimal DropInFee { get; set; }
}
