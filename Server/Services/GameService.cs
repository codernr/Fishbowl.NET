using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Fishbowl.Net.Client.Services;
using Fishbowl.Net.Server.Hubs;
using Fishbowl.Net.Shared.Data;
using Microsoft.AspNetCore.SignalR;

namespace Fishbowl.Net.Server.Services
{
    public class GameService
    {
        private Dictionary<string, GameContext> contexts = new();

        private readonly Dictionary<string, GameContext> connectionContextMap = new();

        private readonly IHubContext<GameHub, IGameClient> hubContext;

        public GameService(IHubContext<GameHub, IGameClient> hubContext) => this.hubContext = hubContext;

        public bool GameContextExists(string password) => this.contexts.ContainsKey(password);

        public Task CreateGameContext(string connectionId, GameContextSetup request)
        {
            if (this.connectionContextMap.ContainsKey(connectionId) || this.contexts.ContainsKey(request.Password))
            {
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
            var context = this.contexts[request.Password];

            if (this.connectionContextMap.ContainsKey(connectionId) || context is null)
            {
                throw new InvalidOperationException();
            }

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
            if (!this.connectionContextMap.ContainsKey(connectionId))
            {
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