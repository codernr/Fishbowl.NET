using System;
using System.Collections.Concurrent;
using System.Linq;
using System.Threading.Tasks;
using Fishbowl.Net.Shared;
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

        public StatusResponse<bool> GameContextExists(string password) => new(StatusCode.Ok, this.contexts.ContainsKey(password));

        public async Task<StatusResponse> CreateGameContext(string connectionId, GameContextSetupViewModel request)
        {
            var password = request.GameContextJoin.Password;

            this.logger.LogInformation(
                "CreateGameContext: {{ConnectionId: {ConnectionId}, Password: {Password}}}",
                connectionId, password);
            
            if (this.connectionContextMap.ContainsKey(connectionId))
            {
                this.logger.LogError("Connection is already assigned to a GameContext");
                return new(StatusCode.ConnectionAlreadyAssigned);
            }

            var context = gameContextFactory(password, request.GameSetup);
            context.GameFinished += context => this.RemoveGameContext(password);

            if (!this.contexts.TryAdd(password, context))
            {
                this.logger.LogError("GameContext already exists");
                return new(StatusCode.GameContextExists);
            }

            if (await context.TryRegisterConnection(request.GameContextJoin.UserId, connectionId) &&
                this.connectionContextMap.TryAdd(connectionId, context))
            {
                return new(StatusCode.Ok);
            }

            return new(StatusCode.ConcurrencyError);
        }

        public async Task<StatusResponse> JoinGameContext(string connectionId, GameContextJoinViewModel request)
        {
            this.logger.LogInformation(
                "JoinGameContext: {{ConnectionId: {ConnectionId}, Request: {Request}}}",
                connectionId, request);
            
            if (this.connectionContextMap.ContainsKey(connectionId))
            {
                this.logger.LogError("Connection is already assigned to a GameContext");
                return new(StatusCode.ConnectionAlreadyAssigned);
            }

            if (!this.contexts.TryGetValue(request.Password, out var context))
            {
                this.logger.LogError("GameContext doesn't exist");
                return new(StatusCode.GameContextNotFound);
            }

            if (!context.CanRegister(request.UserId))
            {
                this.logger.LogError("GameContext is full and player Id is not registered connection.");
                return new(StatusCode.GameContextFull);
            }

            if (await context.TryRegisterConnection(request.UserId, connectionId) &&
                this.connectionContextMap.TryAdd(connectionId, context))
            {
                return new(StatusCode.Ok);
            }

            return new(StatusCode.ConcurrencyError);
        }

        public async Task RemoveConnection(string connectionId)
        {
            this.logger.LogInformation("RemoveConnection: {ConnectionId}", connectionId);

            if (!this.connectionContextMap.TryGetValue(connectionId, out var context))
            {
                this.logger.LogInformation("Connection not found");
                return;
            }

            await context.RemoveConnection(connectionId);

            this.connectionContextMap.TryRemove(connectionId, out var connection);
        }

        public GameContext? GetContext(string connectionId)
        {
            this.connectionContextMap.TryGetValue(connectionId, out var context);
            return context;
        }

        private async void RemoveGameContext(string password)
        {
            this.logger.LogInformation("RemoveGameContext: {Password}", password);

            if (!this.contexts.TryRemove(password, out var context))
            {
                this.logger.LogWarning("Context already removed");
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
        }
    }
}