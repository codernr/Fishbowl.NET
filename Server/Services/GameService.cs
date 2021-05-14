using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Fishbowl.Net.Shared;
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

        public async Task<StatusCode> CreateGameContext(string connectionId, GameContextSetupViewModel request)
        {
            var password = request.GameContextJoin.Password;

            this.logger.LogInformation(
                "CreateGameContext: {{ConnectionId: {ConnectionId}, Password: {Password}}}",
                connectionId, password);
            
            if (this.connectionContextMap.ContainsKey(connectionId))
            {
                this.logger.LogError("Connection is already assigned to a GameContext");
                return StatusCode.ConnectionAlreadyAssigned;
            }

            if (this.contexts.ContainsKey(password))
            {
                this.logger.LogError("GameContext already exists");
                return StatusCode.GameContextExists;
            }

            var context = gameContextFactory(password, request.GameSetup);
            context.GameFinished += context => this.RemoveGameContext(password);

            this.contexts.Add(password, context);
            this.connectionContextMap.Add(connectionId, context);

            await context.RegisterConnection(request.GameContextJoin.UserId, connectionId);
            
            return StatusCode.Ok;
        }

        public async Task<StatusCode> JoinGameContext(string connectionId, GameContextJoinViewModel request)
        {
            this.logger.LogInformation(
                "JoinGameContext: {{ConnectionId: {ConnectionId}, Request: {Request}}}",
                connectionId, request);
            
            if (this.connectionContextMap.ContainsKey(connectionId))
            {
                this.logger.LogError("Connection is already assigned to a GameContext");
                return StatusCode.ConnectionAlreadyAssigned;
            }

            if (!this.contexts.ContainsKey(request.Password))
            {
                this.logger.LogError("GameContext doesn't exist");
                return StatusCode.GameContextNotFound;
            }

            var context = this.contexts[request.Password];

            if (!context.CanRegister(request.UserId))
            {
                this.logger.LogError("GameContext is full and player Id is not registered connection.");
                return StatusCode.GameContextFull;
            }

            await context.RegisterConnection(request.UserId, connectionId);

            this.connectionContextMap.Add(connectionId, context);
            return StatusCode.Ok;
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