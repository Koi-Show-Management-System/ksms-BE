using System;

namespace KSMS.Domain.Dtos.Responses.Livestream;

public class GetLiveStreamResponse
{
    public Guid Id { get; set; }
    public string? CallId { get; set; }
    public DateTime? StartTime { get; set; }
    public DateTime? EndTime { get; set; }
    public string? ShowName { get; set; }
    public int ViewerCount { get; set; }
    public string? Status { get; set; }
}