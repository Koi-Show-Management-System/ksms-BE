namespace KSMS.Infrastructure.Utils;

public static class VietNamTimeUtil
{
    private const string VIETNAM_TIMEZONE = "SE Asia Standard Time";

    public static DateTime GetVietnamTime()
    {
        var vietnamZone = TimeZoneInfo.FindSystemTimeZoneById(VIETNAM_TIMEZONE);
        return TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, vietnamZone);
    }

    public static DateTime ConvertToVietnamTime(DateTime utcTime)
    {
        var vietnamZone = TimeZoneInfo.FindSystemTimeZoneById(VIETNAM_TIMEZONE);
        return TimeZoneInfo.ConvertTimeFromUtc(utcTime, vietnamZone);
    }
}