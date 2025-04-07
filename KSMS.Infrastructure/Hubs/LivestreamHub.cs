using System.Collections.Concurrent;
using Microsoft.AspNetCore.SignalR;

namespace KSMS.Infrastructure.Hubs;

public class LivestreamHub : Hub
{
    private static readonly ConcurrentDictionary<string, int> ViewerCounts = new();
    public static int GetCurrentViewerCount(string streamId)
    {
        return ViewerCounts.GetValueOrDefault(streamId, 0);
    }
    public override async Task OnConnectedAsync()
    {
        await base.OnConnectedAsync();
    }
    public override async Task OnDisconnectedAsync(Exception? exception)
    {
        var streamId = Context.Items["StreamId"] as string;
        if (!string.IsNullOrEmpty(streamId))
        {
            DecrementViewerCount(streamId);
            await UpdateViewerCount(streamId);
        }
        await base.OnDisconnectedAsync(exception);
    }

    public async Task JoinStream(string streamId)
    {
        Context.Items["StreamId"] = streamId;
        await Groups.AddToGroupAsync(Context.ConnectionId, $"stream_{streamId}");
        IncrementViewerCount(streamId);
        await UpdateViewerCount(streamId);
    }
    public async Task LeaveStream(string streamId)
    {
        Context.Items.Remove("StreamId");
        await Groups.RemoveFromGroupAsync(Context.ConnectionId, $"stream_{streamId}");
        DecrementViewerCount(streamId);
        await UpdateViewerCount(streamId);
    }
    private void IncrementViewerCount(string streamId)
    {
        ViewerCounts.AddOrUpdate(streamId, 1, (key, count) => count + 1);
    }
    private void DecrementViewerCount(string streamId)
    {
        ViewerCounts.AddOrUpdate(streamId, 0, (key, count) => Math.Max(0, count - 1));
    }
    private async Task UpdateViewerCount(string streamId)
    {
        var count = ViewerCounts.GetValueOrDefault(streamId, 0);
        await Clients.Group($"stream_{streamId}").SendAsync("ViewerCountUpdated", new
        {
            StreamId = streamId,
            ViewerCount = count
        });
    }
}