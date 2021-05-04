using System;
using System.Threading.Tasks;
using Fishbowl.Net.Client.Services;
using Fishbowl.Net.Server.Services;
using Fishbowl.Net.Shared.Data;
using Microsoft.AspNetCore.SignalR;

namespace Fishbowl.Net.Server.Hubs
{
    public class GameHub : Hub<IGameClient>
    {
        private readonly GameService service;

        public GameHub(GameService service) => this.service = service;

        private GameContext GameContext => this.service.GetContext(this.Context.ConnectionId);

        public override async Task OnDisconnectedAsync(System.Exception? exception)
        {
            await this.service.RemoveConnection(this.Context.ConnectionId);
            await base.OnDisconnectedAsync(exception);
        }

        public Task CreateGameContext(GameContextSetup request) =>
            this.service.CreateGameContext(this.Context.ConnectionId, request);

        public bool GameContextExists(string password) => this.service.GameContextExists(password);

        public async Task<bool> JoinGameContext(GameContextJoin request)
        {
            try
            {
                await this.service.JoinGameContext(this.Context.ConnectionId, request);
                return true;
            }
            catch (InvalidOperationException)
            {
                return false;
            }
        }

        public int GetWordCount() => this.GameContext.WordCount;

        public void AddPlayer(Player player) =>
            this.GameContext.AddPlayer(player);

        public void StartPeriod(DateTimeOffset timestamp) => this.GameContext.Game.StartPeriod(timestamp);

        public void NextWord(DateTimeOffset timestamp) => this.GameContext.Game.NextWord(timestamp);

        public void AddScore(Score score) => this.GameContext.Game.AddScore(score);

        public void FinishPeriod(DateTimeOffset timestamp) => this.GameContext.Game.FinishPeriod(timestamp);
    }
}