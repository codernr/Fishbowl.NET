using System;
using System.Threading.Tasks;
using Fishbowl.Net.Client.Services;
using Fishbowl.Net.Server.Services;
using Fishbowl.Net.Shared;
using Fishbowl.Net.Shared.Data;
using Fishbowl.Net.Shared.Data.ViewModels;
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

        public Task<StatusCode> CreateGameContext(GameContextSetupViewModel request) =>
            this.service.CreateGameContext(this.Context.ConnectionId, request);

        public bool GameContextExists(string password) => this.service.GameContextExists(password);

        public Task<StatusCode> JoinGameContext(GameContextJoinViewModel request) =>
            this.service.JoinGameContext(this.Context.ConnectionId, request);

        public int GetWordCount() => this.GameContext.WordCount;

        public Task AddPlayer(Player player) =>
            this.GameContext.AddPlayer(player);

        public Task SetTeamName(TeamNameViewModel teamName) => this.GameContext.SetTeamName(teamName);

        public void StartPeriod(DateTimeOffset timestamp) => this.GameContext.Game.StartPeriod(timestamp);

        public void NextWord(DateTimeOffset timestamp) => this.GameContext.Game.NextWord(timestamp);

        public void AddScore(ScoreViewModel score) => this.GameContext.Game.AddScore(score.Map());

        public void RevokeLastScore() => this.GameContext.Game.RevokeLastScore();

        public void FinishPeriod(DateTimeOffset timestamp) => this.GameContext.Game.FinishPeriod(timestamp);
    }
}