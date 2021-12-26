using System.Text.Json;
using Fluxor;
using Microsoft.Extensions.Logging;

namespace Fishbowl.Net.Client.Shared.Store
{
    public class LoggingMiddleware : Middleware
    {
        private readonly ILogger<LoggingMiddleware> logger;

        public LoggingMiddleware(ILogger<LoggingMiddleware> logger) =>
            this.logger = logger;

        public override void BeforeDispatch(object action) => this.Log("BeforeDispatch", action);

        public override void AfterDispatch(object action) => this.Log("AfterDispatch", action);

        private void Log(string callback, object action) =>
            this.logger.LogInformation("{callback}: {action}", callback, action);
    }
}