using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using NCrontab;

namespace ScheduledBackgroundService
{
    public abstract class ScheduledBackgroundService : BackgroundService
    {
        private readonly ILogger _logger;
        private CrontabSchedule _crontabSchedule;

        public ScheduledBackgroundService(ILogger logger, string cronExpression)
        {
            _logger = logger;
            var includingSeconds = new CrontabSchedule.ParseOptions { IncludingSeconds = true };
            _crontabSchedule = CrontabSchedule.Parse(cronExpression, includingSeconds);

            _logger.LogDebug($"cronExpression: [{cronExpression}]");
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested) 
            {
                var utcNow = DateTime.UtcNow;
                var nextOccurrenceDateTime = _crontabSchedule.GetNextOccurrence(utcNow);
                var durationToNextOccurrence = (nextOccurrenceDateTime - utcNow);

                _logger.LogDebug($"Scheduled execution for, {nextOccurrenceDateTime.ToString("o")}");
                await Task.Delay(durationToNextOccurrence, stoppingToken);
                _logger.LogDebug($"Started execution at [{DateTime.UtcNow.ToString("o")}]");
                
                await ExecuteAtScheduledTimeAsync(stoppingToken);
            }
        }

        protected abstract Task ExecuteAtScheduledTimeAsync(CancellationToken stoppingToken);
    }
}
