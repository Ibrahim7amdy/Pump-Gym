namespace Pump.Application.Interfaces.Visits;

public interface IGymQrSettings
{
    string GymName { get; }
    TimeZoneInfo GymTimeZone { get; }
    string CreatePayload();
    bool IsValidPayload(string payload);
}
