using QRCoder;

namespace KSMS.Infrastructure.Utils;

public static class QrcodeUtil
{
    public static string GenerateQrCode(Guid guid)
    {
        try
        {
            var guidString = guid.ToString();
            using var qrGenerator = new QRCodeGenerator();
            var qrCodeData = qrGenerator.CreateQrCode(guidString, QRCodeGenerator.ECCLevel.Q);
            var qrCode = new PngByteQRCode(qrCodeData);
            return Convert.ToBase64String(qrCode.GetGraphic(20));
        } catch (Exception ex)
        {
            throw new Exception($"Error at Generate QR with error: {ex.Message}");
        }
    }
}
