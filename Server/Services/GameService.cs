using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Fishbowl.Net.Client.Services;
using Fishbowl.Net.Server.Hubs;
using Fishbowl.Net.Shared.Data;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Logging;

namespace Fishbowl.Net.Server.Services
{
    public class GameService
    {
        private Dictionary<string, GameContext> contexts = new();

        private readonly Dictionary<string, GameContext> connectionContextMap = new();

        private readonly IHubContext<GameHub, IGameClient> hubContext;

        private readonly ILogger<GameService> logger;

        public GameService(
            IHubContext<GameHub, IGameClient> hubContext,
            ILogger<GameService> logger) =>
            (this.hubContext, this.logger) = (hubContext, logger);

        public bool GameContextExists(string password) => this.contexts.ContainsKey(password);

        public Task CreateGameContext(string connectionId, GameContextSetup request)
        {
            this.logger.LogInformation(
                "CreateGameContext: {{ConnectionId: {ConnectionId}, Password: {Password}}}",
                connectionId, request.Password);
            
            if (this.connectionContextMap.ContainsKey(connectionId))
            {
                this.logger.LogError("Connection is already assigned to a GameContext");
                throw new InvalidOperationException();
            }

            if (this.contexts.ContainsKey(request.Password))
            {
                this.logger.LogError("GameContext already exists");
                throw new InvalidOperationException();
            }

            var groupHubContext = new GroupHubContext(this.hubContext, request.Password);
            var context = new GameContext(request.WordCount, groupHubContext);
            context.GameFinished += context => this.RemoveGameContext(request.Password);

            this.contexts.Add(request.Password, context);
            this.connectionContextMap.Add(connectionId, context);

            return context.RegisterConnection(request.UserId, connectionId);
        }

        public async Task JoinGameContext(string connectionId, GameContextJoin request)
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

        public async Task<Game> ReconnectGameContext(string connectionId, GameContextJoin request)
        {
            await this.JoinGameContext(connectionId, request);

            return this.contexts[request.Password].GetGameData();
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