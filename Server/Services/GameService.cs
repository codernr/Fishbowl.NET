using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Fishbowl.Net.Shared.Data;
using Fishbowl.Net.Shared.Data.ViewModels;
using Microsoft.Extensions.Logging;

namespace Fishbowl.Net.Server.Services
{
    public class GameService
    {
        private Dictionary<string, GameContext> contexts = new();

        private readonly Dictionary<string, GameContext> connectionContextMap = new();

        private readonly Func<string, GameSetupViewModel, GameContext> gameContextFactory;

        private readonly ILogger<GameService> logger;

        public GameService(
            Func<string, GameSetupViewModel, GameContext> gameContextFactory,
            ILogger<GameService> logger) =>
            (this.gameContextFactory, this.logger) = (gameContextFactory, logger);

        public bool GameContextExists(string password) => this.contexts.ContainsKey(password);

        public Task CreateGameContext(string connectionId, GameContextSetupViewModel request)
        {
            var password = request.GameContextJoin.Password;

            this.logger.LogInformation(
                "CreateGameContext: {{ConnectionId: {ConnectionId}, Password: {Password}}}",
                connectionId, password);
            
            if (this.connectionContextMap.ContainsKey(connectionId))
            {
                this.logger.LogError("Connection is already assigned to a GameContext");
                throw new InvalidOperationException();
            }

            if (this.contexts.ContainsKey(password))
            {
                this.logger.LogError("GameContext already exists");
                throw new InvalidOperationException();
            }

            var context = gameContextFactory(password, request.GameSetup);
            context.GameFinished += context => this.RemoveGameContext(password);

            this.contexts.Add(password, context);
            this.connectionContextMap.Add(connectionId, context);

            return context.RegisterConnection(request.GameContextJoin.UserId, connectionId);
        }

        public async Task JoinGameContext(string connectionId, GameContextJoinViewModel request)
        {
            this.logger.LogInformation(
                "JoinGameContext: {{ConnectionId: {ConnectionId}, Password: {Password}}}",
                connectionId, request.Password);
            
            if (this.connectionContextMap.ContainsKey(connectionId))
            {
                this.logger.LogError("Connection is already assigned to a GameContext");
                throw new InvalidOperationException();
            }

            if (!this.contexts.ContainsKey(request.Password))
            {
                this.logger.LogError("GameContext doesn't exist");
                throw new InvalidOperationException();
            }

            var context = this.contexts[request.Password];

            await context.RegisterConnection(request.UserId, connectionId);

            this.connectionContextMap.Add(connectionId, context);
        }

        public async Task RemoveConnection(string connectionId)
        {
            this.logger.LogInformation("RemoveConnection: {ConnectionId}", connectionId);

            if (!this.connectionContextMap.ContainsKey(connectionId))
            {
                this.logger.LogInformation("Connection not found");
                return;
            }

            var context = this.connectionContextMap[connectionId];
            
            await context.RemoveConnection(connectionId);

            this.connectionContextMap.Remove(connectionId);
        }

        public GameContext GetContext(string connectionId) => this.connectionContextMap[connectionId];

        private async void RemoveGameContext(string password)
        {
            var context = this.contexts[password];

            var connections = this.connectionContextMap
                .Where(item => item.Value == context)
                .Select(item => item.Key)
                .ToList();

            foreach (var connectionId in connections)
            {
                this.connectionContextMap.Remove(connectionId);
            }

            this.contexts.Remove(password);

            await context.DisposeAsync();
        }
    }
}