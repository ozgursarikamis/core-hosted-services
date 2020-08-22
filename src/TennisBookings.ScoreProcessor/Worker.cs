using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace TennisBookings.ScoreProcessor
{
    public class Worker : BackgroundService
    {
        private readonly ILogger<Worker> _logger;
        private readonly IHostApplicationLifetime _hostApplication;

        public Worker(ILogger<Worker> logger, IHostApplicationLifetime hostApplication)
        {
            _logger = logger;
            _hostApplication = hostApplication;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            stoppingToken.Register(() =>
            {
                _logger.LogInformation("Ending score processing service");
            });
            try
            {
                throw new InvalidOperationException("IOE");
                while (!stoppingToken.IsCancellationRequested)
                {
                    _logger.LogInformation("Worker running at: {time}", DateTimeOffset.Now);
                    await Task.Delay(1000, stoppingToken);
                }
            }
            catch (OperationCanceledException e)
            {
                _logger.LogError(e, "OperationCanceledException");
            }
            catch (Exception e)
            {
                _logger.LogCritical(e, "unhandled exception was thrown");
            }
            finally
            {
                // do something
                _hostApplication.StopApplication();
            }
        }
    }
}
