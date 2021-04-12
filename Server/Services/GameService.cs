using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Fishbowl.Net.Client.Services;
using Fishbowl.Net.Server.Hubs;
using Microsoft.AspNetCore.SignalR;

namespace Fishbowl.Net.Server.Services
{
    public class GameService
    {
        private List<GameContext> contexts = new();

        private readonly Dictionary<string, GameContext> connectionMap = new();

        private readonly IHubContext<GameHub, IGameClient> hubContext;

        public GameService(IHubContext<GameHub, IGameClient> hubContext) => this.hubContext = hubContext;

        public async Task CreateGame(string connectionId, string password)
        {
            if (this.connectionMap.ContainsKey(connectionId) || this.contexts.Any(context => context.Password == password))
            {
                throw new InvalidOperationException();
            }

            await this.hubContext.Groups.AddToGroupAsync(connectionId, password);
            var context = new GameContext(password, this.hubContext);

            this.contexts.Add(context);
            this.connectionMap.Add(connectionId, context);

            context.RegisterConnection(connectionId);
        }

        public async Task JoinGame(string connectionId, string password)
        {
            var context = this.contexts.FirstOrDefault(game => game.Password == password);

            if (this.connectionMap.ContainsKey(connectionId) || context is null)
            {
                throw new InvalidOperationException();
            }

            await this.hubContext.Groups.AddToGroupAsync(connectionId, password);
            context.RegisterConnection(connectionId);
        }

        public async Task RemoveConnection(string connectionId)
        {
            if (this.connectionMap.ContainsKey(connectionId))
            {
                var context = this.connectionMap[connectionId];
                context.RemoveConnection(connectionId);
                this.connectionMap.Remove(connectionId);

                await this.hubContext.Groups.RemoveFromGroupAsync(context.Password, connectionId);
            }
        }

        public GameContext GetContext(string connectionId) => this.connectionMap[connectionId];
    }
}