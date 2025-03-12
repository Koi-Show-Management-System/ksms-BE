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
using KSMS.Domain.Enums;
using KSMS.Infrastructure.Database;
using KSMS.Infrastructure.Utils;
using ShowStatus = KSMS.Domain.Entities.ShowStatus;

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
            _logger.LogInformation("ShowStatusBackgroundService bắt đầu chạy.");

            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    using var scope = _scopeFactory.CreateScope();
                    var unitOfWork = scope.ServiceProvider.GetRequiredService<IUnitOfWork<KoiShowManagementSystemContext>>();
                    var showStatusRepository = unitOfWork.GetRepository<ShowStatus>();
                    var koiShowRepository = unitOfWork.GetRepository<KoiShow>();
                    var currentTime = VietNamTimeUtil.GetVietnamTime();
                    
                    var showStatuses = await showStatusRepository.GetListAsync();
                    foreach (var status in showStatuses)
                    {
                        var isNowActive = currentTime >= status.StartDate && currentTime <= status.EndDate;

                        if (status.IsActive != isNowActive)
                        {
                            status.IsActive = isNowActive; 
                            showStatusRepository.UpdateAsync(status);
                            if (isNowActive)
                            {
                                var koiShow = await koiShowRepository.SingleOrDefaultAsync(predicate: x => x.Id == status.KoiShowId);
                                var showProgress = Enum.Parse<ShowProgress>(status.StatusName);
                            
                                var newStatus = showProgress switch
                                {
                                    ShowProgress.RegistrationOpen or ShowProgress.RegistrationClosed => "upcoming",
                                    ShowProgress.Finished => "finished",
                                    _ => "inprogress"
                                };

                                if (koiShow.Status != newStatus)
                                {
                                    koiShow.Status = newStatus; 
                                    koiShowRepository.UpdateAsync(koiShow);
                                }
                            }
                        }
                    }

                    if (showStatuses.Any())
                    {
                        await unitOfWork.CommitAsync();
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError($"Lỗi trong ShowStatusBackgroundService: {ex.Message}");
                }

                await Task.Delay(TimeSpan.FromDays(1), stoppingToken);
            }
        }
    }

}

