namespace Pump.Application.Dtos.Common;

public sealed record ImageUploadRequestDto(
    string? FileName,
    string? ContentType,
    long Length,
    byte[] Header);
