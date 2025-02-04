//using QRCoder;

//namespace KSMS.Infrastructure.Utils;

//public static class QrcodeUtil
//{
//    public static string GenerateQrCode(Guid guid)
//    {
//        try
//        {
//            var guidString = guid.ToString();
//            using var qrGenerator = new QRCodeGenerator();
//            var qrCodeData = qrGenerator.CreateQrCode(guidString, QRCodeGenerator.ECCLevel.Q);
//            var qrCode = new PngByteQRCode(qrCodeData);
//            return Convert.ToBase64String(qrCode.GetGraphic(20));
//        } catch (Exception ex)
//        {
//            throw new Exception($"Error at Generate QR with error: {ex.Message}");
//        }
//    }
//}
using QRCoder;
using System;

namespace KSMS.Infrastructure.Utils
{
    public static class QrcodeUtil
    {
        // Tạo mã QR và chuyển thành Base64 string
        public static string GenerateQrCode(Guid ticketId)
        {
            try
            {
                // Chuyển TicketId thành chuỗi
                var ticketIdString = ticketId.ToString();

                // Khởi tạo QRCodeGenerator
                using var qrGenerator = new QRCodeGenerator();

                // Tạo mã QR từ chuỗi TicketId
                var qrCodeData = qrGenerator.CreateQrCode(ticketIdString, QRCodeGenerator.ECCLevel.Q);

                // Chuyển đổi dữ liệu mã QR thành hình ảnh PNG (chế độ PngByteQRCode)
                var qrCode = new PngByteQRCode(qrCodeData);

                // Trả về hình ảnh dưới dạng chuỗi Base64
                return Convert.ToBase64String(qrCode.GetGraphic(20)); // 20 là kích thước của hình ảnh
            }
            catch (Exception ex)
            {
                throw new Exception($"Error at Generate QR with error: {ex.Message}");
            }
        }
    }
}
