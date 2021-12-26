using System;
using System.Collections.Concurrent;
using System.Linq;
using System.Threading.Tasks;
using Fishbowl.Net.Shared;
using Fishbowl.Net.Shared.Actions;
using Fishbowl.Net.Shared.ViewModels;
using Microsoft.Extensions.Logging;

namespace Fishbowl.Net.Server.Services
{
    public class GameService
    {
        private readonly ConcurrentDictionary<string, GameContext> contexts = new();

        private readonly ConcurrentDictionary<string, GameContext> connectionContextMap = new();

        private readonly Func<string, GameSetupViewModel, GameContext> gameContextFactory;

        private readonly ILogger<GameService> logger;

        public GameService(
            Func<string, GameSetupViewModel, GameContext> gameContextFactory,
            ILogger<GameService> logger) =>
            (this.gameContextFactory, this.logger) = (gameContextFactory, logger);

        public async Task<StatusResponse> CreateGameContext(string connectionId, CreateGameContextAction request)
        {
            this.Log(nameof(this.CreateGameContext), connectionId, request);

            var password = request.GameContextJoin.Password;
            
            if (this.connectionContextMap.ContainsKey(connectionId))
            {
                this.logger.LogWarning(StatusCode.ConnectionAlreadyAssigned.ToString());
                return new(StatusCode.ConnectionAlreadyAssigned);
            }

            var context = gameContextFactory(password, request.GameSetup);
            context.GameFinished += context => this.RemoveGameContext(password);

            if (!this.contexts.TryAdd(password, context))
            {
                this.logger.LogWarning(StatusCode.GameContextExists.ToString());
                return new(StatusCode.GameContextExists);
            }

            if (await context.TryRegisterConnection(request.GameContextJoin.Username, connectionId) &&
                this.connectionContextMap.TryAdd(connectionId, context))
            {
                this.logger.LogInformation(StatusCode.Ok.ToString());
                return new(StatusCode.Ok);
            }

            this.logger.LogError(StatusCode.ConcurrencyError.ToString());
            return new(StatusCode.ConcurrencyError);
        }

        public async Task<StatusResponse> JoinGameContext(string connectionId, JoinGameContextAction request)
        {
            this.Log(nameof(this.JoinGameContext), connectionId, request);
            
            if (this.connectionContextMap.ContainsKey(connectionId))
            {
                this.logger.LogWarning(StatusCode.ConnectionAlreadyAssigned.ToString());
                return new(StatusCode.ConnectionAlreadyAssigned);
            }

            if (!this.contexts.TryGetValue(request.Password, out var context))
            {
                this.logger.LogWarning(StatusCode.GameContextNotFound.ToString());
                return new(StatusCode.GameContextNotFound);
            }

            if (context.IsUsernameTaken(request.Username))
            {
                this.logger.LogWarning(StatusCode.UsernameTaken.ToString());
                return new(StatusCode.UsernameTaken);
            }

            if (!context.CanRegister(request.Username))
            {
                this.logger.LogWarning(StatusCode.GameContextFull.ToString());
                return new(StatusCode.GameContextFull);
            }

            if (await context.TryRegisterConnection(request.Username, connectionId) &&
                this.connectionContextMap.TryAdd(connectionId, context))
            {
                this.logger.LogInformation(StatusCode.Ok.ToString());
                return new(StatusCode.Ok);
            }

            this.logger.LogError(StatusCode.ConcurrencyError.ToString());
            return new(StatusCode.ConcurrencyError);
        }

        public async Task RemoveConnection(string connectionId)
        {
            this.Log(nameof(this.RemoveConnection), connectionId);

            if (!this.connectionContextMap.TryGetValue(connectionId, out var context))
            {
                this.logger.LogInformation("NotFound");
                return;
            }

            await context.RemoveConnection(connectionId);

            this.connectionContextMap.TryRemove(connectionId, out var connection);
        }

        public GameContext? GetContext(string connectionId)
        {
            this.Log(nameof(this.GetContext), connectionId);
            this.connectionContextMap.TryGetValue(connectionId, out var context);
            return context;
        }

        private async void RemoveGameContext(string password)
        {
            this.Log(nameof(this.RemoveGameContext), password);

            if (!this.contexts.TryRemove(password, out var context))
            {
                this.logger.LogWarning("AlreadyRemoved");
                return;
            }

            var connections = this.connectionContextMap
                .Where(item => item.Value == context)
                .Select(item => item.Key)
                .ToList();

            foreach (var connectionId in connections)
            {
                this.connectionContextMap.TryRemove(connectionId, out var connection);
            }

            await context.DisposeAsync();

            this.logger.LogInformation("ContextRemoved");
        }

        private void Log(string methodName, object arg1) =>
            this.logger.LogInformation("{MethodName}: [{Arg1}]", methodName, arg1);

        private void Log(string methodName, object arg1, object arg2) =>
            this.logger.LogInformation("{MethodName}: [{Arg1}] [{Arg2}]", methodName, arg1, arg2);
    }
}