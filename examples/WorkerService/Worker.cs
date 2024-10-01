using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace WorkerService
{
	public class Worker : BackgroundService
    {
        private readonly ILogger<Worker> _logger;

        public Worker(ILogger<Worker> logger)
        {
            _logger = logger;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            try
            {
                // Pretend `13` is a correlation GUID, user id, request url, or other Very Useful Information.
                using (_logger.BeginScope("Work item {workItemId}", 13))
                using (_logger.BeginScope("Next {workItemId}", 7))
                {
                    _logger.LogInformation("Worker running at: {time}", DateTimeOffset.Now);
                    await Task.Delay(1000, stoppingToken);
                    _logger.LogInformation("Worker running at: {time}", DateTimeOffset.Now);
                    await Task.Delay(1000, stoppingToken);

                    throw new InvalidOperationException("Test exception.");
                }
            }
            catch (Exception e)
            {
                // Example code representing a high-level catch (e.g., an early exception handling middleware).
                _logger.LogError(e, "Unexpected error.");
                throw;
            }
        }
    }
}
