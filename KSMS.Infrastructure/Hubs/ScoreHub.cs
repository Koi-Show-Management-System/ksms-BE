using Microsoft.AspNetCore.SignalR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KSMS.Infrastructure.Hubs
{
    public class ScoreHub : Hub
    {
        public async Task SendUpdatedScores()
        {
            await Clients.All.SendAsync("ReceiveUpdatedScores");
        }
    }
}
