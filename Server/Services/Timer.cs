using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;

namespace Fishbowl.Net.Server.Services
{
    public class Timer : IAsyncDisposable
    {
        private readonly Task task;

        private readonly CancellationTokenSource source = new();

        private TaskCompletionSource restartTaskSource = new();

        public Timer(TimeSpan timeout, Func<Task> action, ILogger<Timer> logger) =>
            this.task = this.Run(timeout, action, this.source.Token, logger);

        public void Restart() => this.restartTaskSource.SetResult();

        private async Task Run(TimeSpan timeout, Func<Task> action, CancellationToken token, ILogger<Timer> logger)
        {
            logger.LogInformation("Run: {Timeout}", timeout);

            try
            {            
                while (await this.RestartRequested(timeout, token))
                {
                    logger.LogInformation("RestartRequested");
                    this.restartTaskSource = new();
                }
            }
            catch (TaskCanceledException)
            {
                logger.LogInformation("Canceled");
                return;
            }

            logger.LogInformation("Expired");
            await action();
        }

        private async Task<bool> RestartRequested(TimeSpan timeout, CancellationToken cancellationToken)
        {
            var completed = await Task.WhenAny(this.restartTaskSource.Task, Task.Delay(timeout, cancellationToken));
            
            await completed;
            
            return completed == this.restartTaskSource.Task;
        }

        public async ValueTask DisposeAsync()
        {
            if (this.task.IsCompleted) return;

            this.source.Cancel();

            await this.task;
        }
    }
}