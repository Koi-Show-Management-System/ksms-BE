using System;
using System.Net.Http.Json;
using System.Threading.Tasks;
using KSMS.Domain.Dtos.Responses;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

public class GeminiChatService : IChatService
{
    private readonly HttpClient _httpClient;
    private readonly string _apiKey;
    private readonly ILogger<GeminiChatService> _logger;
    private const string API_URL = "https://generativelanguage.googleapis.com/v1beta/models/gemini-pro:generateContent";
    private const string SYSTEM_PROMPT = @"
        Bạn là trợ lý của Hệ thống Quản lý Giải đấu Cá Koi (Koi Show Management System). Hỗ trợ người dùng về:

        1. Quy trình tổ chức và tham gia giải đấu:
           - Đăng ký tổ chức giải đấu (dành cho ban tổ chức)
           - Đăng ký tham gia giải đấu (dành cho người nuôi cá)
           - Quy trình chấm điểm và công bố kết quả
           - Thanh toán phí tham gia

        2. Quy định về hồ sơ cá Koi:
           - Thông tin cần thiết: variety, size, pattern, age
           - Yêu cầu về hình ảnh cá
           - Giới hạn số lượng cá đăng ký

        3. Hệ thống chấm điểm:
           - Tiêu chí chấm điểm
           - Cách tính điểm
           - Xem kết quả realtime
           - Bảng xếp hạng

        4. Hỗ trợ kỹ thuật:
           - Đăng nhập/Đăng ký tài khoản
           - Xác thực email
           - Quản lý hồ sơ
           - Thanh toán qua PayOS

        Trả lời bằng tiếng Việt, chuyên nghiệp và rõ ràng.
        Nếu được hỏi về thông tin cụ thể của một giải đấu, phí tham gia hoặc giải thưởng, 
        hãy đề nghị người dùng liên hệ trực tiếp với ban tổ chức giải đấu đó.";

    public GeminiChatService(IConfiguration configuration, ILogger<GeminiChatService> logger)
    {
        _apiKey = configuration["Gemini:ApiKey"];
        _httpClient = new HttpClient();
        _logger = logger;
    }

    public async Task<string> GetChatResponse(string userQuestion)
    {
        try
        {
            var requestBody = new
            {
                contents = new[]
                {
                    new
                    {
                        role = "user",
                        parts = new[] { new { text = $"{SYSTEM_PROMPT}\n\nUser: {userQuestion}" } }
                    }
                }
            };

            var response = await _httpClient.PostAsJsonAsync(
                $"{API_URL}?key={_apiKey}", 
                requestBody);

            response.EnsureSuccessStatusCode();
            var result = await response.Content.ReadFromJsonAsync<GeminiResponse>();
            return result.Candidates[0].Content.Parts[0].Text;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting chat response for question: {Question}", userQuestion);
            throw;
        }
    }
}

 