using System;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;

namespace Fishbowl.Net.Server.Services
{
    public class Timer
    {
        private readonly Task task;

        private TaskCompletionSource restartTaskSource = new();

        public Timer(TimeSpan timeout, Func<Task> action, ILogger<Timer> logger) =>
            this.task = this.Run(timeout, action, logger);

        private async Task Run(TimeSpan timeout, Func<Task> action, ILogger<Timer> logger)
        {
            logger.LogInformation($"Run {{ timeout: {timeout.ToString("c")} }}");

            while (await this.RestartRequested(timeout))
            {
                logger.LogInformation("Restart requested");
                this.restartTaskSource = new();
            }

            logger.LogInformation("Expired, firing action");
            await action();
        }

        private async Task<bool> RestartRequested(TimeSpan timeout) =>
            (await Task.WhenAny(this.restartTaskSource.Task, Task.Delay(timeout))) == this.restartTaskSource.Task;
    }
}