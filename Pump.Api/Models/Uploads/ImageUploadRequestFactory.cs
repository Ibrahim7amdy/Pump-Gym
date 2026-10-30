using Microsoft.AspNetCore.Http;
using Pump.Application.Dtos.Common;

namespace Pump.Api.Models.Uploads;

public static class ImageUploadRequestFactory
{
    private const int HeaderLength = 12;

    public static async Task<ImageUploadRequestDto> CreateAsync(IFormFile? file, CancellationToken ct = default)
    {
        if (file is null)
            return new ImageUploadRequestDto(null, null, 0, []);

        var buffer = new byte[HeaderLength];
        var bytesRead = 0;

        await using (var stream = file.OpenReadStream())
        {
            while (bytesRead < buffer.Length)
            {
                var read = await stream.ReadAsync(buffer.AsMemory(bytesRead), ct);
                if (read == 0)
                    break;
                bytesRead += read;
            }
        }

        return new ImageUploadRequestDto(
            file.FileName,
            file.ContentType,
            file.Length,
            buffer[..bytesRead]);
    }
}
