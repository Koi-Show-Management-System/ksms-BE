using Microsoft.AspNetCore.SignalR;
using System;
using System.Threading.Tasks;

namespace KSMS.Infrastructure.Hubs
{
    public interface IShowClient
    {
        Task ReceiveShowStatusUpdate(Guid showId, string status);
    }
    
    public class ShowHub : Hub
    {
        // Không cần định nghĩa phương thức ở đây
    }
    
    public interface IShowHubService
    {
        Task SendShowStatusUpdate(Guid showId, string status);
    }
    
    public class ShowHubService : IShowHubService
    {
        private readonly IHubContext<ShowHub> _hubContext;
        
        public ShowHubService(IHubContext<ShowHub> hubContext)
        {
            _hubContext = hubContext;
        }
        
        public async Task SendShowStatusUpdate(Guid showId, string status)
        {
            await _hubContext.Clients.All.SendAsync("ReceiveShowStatusUpdate", showId, status);
        }
    }
} 