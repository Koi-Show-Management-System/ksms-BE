using Microsoft.AspNetCore.Http;

namespace KSMS.Infrastructure.Utils;

public static class FileUtils
{
    public static IFormFile ConvertBase64ToFile(string base64String)
    {
        var bytes = Convert.FromBase64String(base64String);
        var stream = new MemoryStream(bytes);

        var file = new FormFile(stream, 0, bytes.Length, "file", "undefined_name.jpg")
        {
            Headers = new HeaderDictionary(),
            ContentType = "image/jpeg"
        };

        return file;
    }
}