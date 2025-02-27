using KSMS.Application.Repositories;
using KSMS.Domain.Entities;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
 
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using KSMS.Infrastructure.Database;
namespace KSMS.Infrastructure.Services
{
    public class ShowStatusBackgroundService : BackgroundService
    {
        private readonly IServiceScopeFactory _scopeFactory;
        private readonly ILogger<ShowStatusBackgroundService> _logger;

        public ShowStatusBackgroundService(IServiceScopeFactory scopeFactory, ILogger<ShowStatusBackgroundService> logger)
        {
            _scopeFactory = scopeFactory;
            _logger = logger;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("ShowStatusBackgroundService started.");

            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    using (var scope = _scopeFactory.CreateScope())
                    {
                     
                        var unitOfWork = scope.ServiceProvider.GetRequiredService<IUnitOfWork<KoiShowManagementSystemContext>>();
                        var showStatusRepository = unitOfWork.GetRepository<ShowStatus>();

                        var currentTime = DateTime.UtcNow;

                        var showStatuses = await showStatusRepository.GetListAsync(
                            predicate: s => s.IsActive && (s.StartDate <= currentTime || s.EndDate <= currentTime)
                        );

                        foreach (var status in showStatuses)
                        {
                            if (status.StartDate <= currentTime && status.EndDate > currentTime)
                            {
                                status.StatusName = "Ongoing";
                            }
                            else if (status.EndDate <= currentTime)
                            {
                                status.StatusName = "Completed";
                                status.IsActive = false;
                            }
                             showStatusRepository.UpdateAsync(status);
                        }

                        if (showStatuses.Any())
                        {
                            await unitOfWork.CommitAsync();
                            _logger.LogInformation($"Updated {showStatuses.Count} show statuses.");
                        }
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError($"Error in ShowStatusBackgroundService: {ex.Message}");
                }

                
                await Task.Delay(TimeSpan.FromDays(1), stoppingToken);
            }
        }
    }

}

