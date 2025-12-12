using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.DependencyInjection;

namespace EngAce.Api.Services
{
    /// <summary>
    /// Background service that checks for expired premium users daily
    /// Runs at 2 AM every day
    /// </summary>
    public class PremiumExpirationBackgroundService : BackgroundService
    {
        private readonly ILogger<PremiumExpirationBackgroundService> _logger;
        private readonly IServiceProvider _serviceProvider;
        private readonly TimeSpan _checkInterval = TimeSpan.FromHours(24); // Check every 24 hours

        public PremiumExpirationBackgroundService(
            ILogger<PremiumExpirationBackgroundService> logger,
            IServiceProvider serviceProvider)
        {
            _logger = logger;
            _serviceProvider = serviceProvider;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("Premium Expiration Background Service started");

            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    // Calculate time until next 2 AM
                    var now = DateTime.Now;
                    var next2AM = DateTime.Today.AddDays(1).AddHours(2); // Next 2 AM
                    
                    if (now.Hour < 2)
                    {
                        // If before 2 AM today, run today at 2 AM
                        next2AM = DateTime.Today.AddHours(2);
                    }

                    var delayUntilNext2AM = next2AM - now;

                    _logger.LogInformation(
                        "Next premium expiration check scheduled at {NextCheckTime} (in {Hours} hours)",
                        next2AM, delayUntilNext2AM.TotalHours);

                    // Wait until 2 AM
                    await Task.Delay(delayUntilNext2AM, stoppingToken);

                    if (stoppingToken.IsCancellationRequested)
                        break;

                    // Run the check
                    await CheckAndDowngradeExpiredUsers();

                    // Wait a bit to avoid running multiple times at 2 AM
                    await Task.Delay(TimeSpan.FromMinutes(5), stoppingToken);
                }
                catch (OperationCanceledException)
                {
                    _logger.LogInformation("Premium Expiration Background Service is stopping");
                    break;
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error in Premium Expiration Background Service");
                    // Wait before retrying
                    await Task.Delay(TimeSpan.FromMinutes(10), stoppingToken);
                }
            }

            _logger.LogInformation("Premium Expiration Background Service stopped");
        }

        private async Task CheckAndDowngradeExpiredUsers()
        {
            try
            {
                _logger.LogInformation("Starting premium expiration check...");

                using (var scope = _serviceProvider.CreateScope())
                {
                    var expirationService = scope.ServiceProvider.GetRequiredService<IPremiumExpirationService>();
                    var result = await expirationService.CheckAndDowngradeExpiredUsersAsync();

                    if (result.Success)
                    {
                        _logger.LogInformation(
                            "Premium expiration check completed successfully: {Message}",
                            result.Message);

                        if (result.TotalDowngraded > 0)
                        {
                            _logger.LogWarning(
                                "⚠️ {Count} users were downgraded from premium to free due to expiration",
                                result.TotalDowngraded);
                        }
                    }
                    else
                    {
                        _logger.LogError("Premium expiration check failed: {Message}", result.Message);
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during premium expiration check");
            }
        }

        public override async Task StopAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("Premium Expiration Background Service is stopping...");
            await base.StopAsync(stoppingToken);
        }
    }
}
