namespace Pump.Application.Interfaces.Common;

public interface IFileStorageService
{
    Task<string> SaveAsync(Stream fileStream, string fileName, string contentType, CancellationToken ct = default);
    Task<string> SaveProtocolImageAsync(Stream fileStream, string contentType, CancellationToken ct = default);
    Task<string> SaveMachineImageAsync(Stream fileStream, string contentType, CancellationToken ct = default);
}
