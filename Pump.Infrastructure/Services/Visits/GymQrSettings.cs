using Microsoft.Extensions.Configuration;
using Pump.Application.Interfaces.Visits;
using System.Security.Cryptography;
using System.Text;

namespace Pump.Infrastructure.Services.Visits;

public class GymQrSettings : IGymQrSettings
{
    private readonly IConfiguration _configuration;

    public GymQrSettings(IConfiguration configuration) => _configuration = configuration;

    public string GymName => _configuration["Gym:Name"] ?? "PUMP Gym";
    private string BranchId => _configuration["Gym:BranchId"] ?? "main-branch";
    public TimeZoneInfo GymTimeZone
    {
        get
        {
            var timeZoneId = _configuration["Gym:TimeZone"] ?? "Africa/Cairo";
            try
            {
                return TimeZoneInfo.FindSystemTimeZoneById(timeZoneId);
            }
            catch (TimeZoneNotFoundException) when (timeZoneId == "Africa/Cairo")
            {
                return TimeZoneInfo.FindSystemTimeZoneById("Egypt Standard Time");
            }
        }
    }

    public string CreatePayload()
    {
        var payloadPrefix = $"PUMP:GYM:{BranchId}";
        var signature = Convert.ToHexString(HMACSHA256.HashData(GetSigningKey(), Encoding.UTF8.GetBytes(payloadPrefix)));

        return $"{payloadPrefix}:{signature}";
    }

    public bool IsValidPayload(string payload)
    {
        var parts = payload.Split(':');
        if (parts.Length != 4
            || parts[0] != "PUMP"
            || parts[1] != "GYM"
            || parts[2] != BranchId)
            return false;

        var payloadPrefix = string.Join(':', parts.Take(3));
        var expected = HMACSHA256.HashData(GetSigningKey(), Encoding.UTF8.GetBytes(payloadPrefix));
        byte[] actual;
        try
        {
            actual = Convert.FromHexString(parts[3]);
        }
        catch (FormatException)
        {
            return false;
        }

        return actual.Length == expected.Length && CryptographicOperations.FixedTimeEquals(actual, expected);
    }

    private byte[] GetSigningKey()
    {
        var key = _configuration["Jwt:Key"];
        if (string.IsNullOrWhiteSpace(key) || Encoding.UTF8.GetByteCount(key) < 32)
            throw new InvalidOperationException("Jwt:Key must contain at least 32 bytes to sign gym QR codes.");

        return Encoding.UTF8.GetBytes(key);
    }
}
