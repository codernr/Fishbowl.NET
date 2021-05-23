using System;
using System.Threading.Tasks;
using Fishbowl.Net.Client.Services;
using Fishbowl.Net.Server.Services;
using Fishbowl.Net.Shared;
using Fishbowl.Net.Shared.GameEntities;
using Fishbowl.Net.Shared.ViewModels;
using Microsoft.AspNetCore.SignalR;

namespace Fishbowl.Net.Server.Hubs
{
    public class GameHub : Hub<IGameClient>
    {
        private readonly GameService service;

        public GameHub(GameService service) => this.service = service;

        public override async Task OnDisconnectedAsync(System.Exception? exception)
        {
            await this.service.RemoveConnection(this.Context.ConnectionId);
            await base.OnDisconnectedAsync(exception);
        }

        public Task<StatusResponse> CreateGameContext(GameContextSetupViewModel request) =>
            this.service.CreateGameContext(this.Context.ConnectionId, request);

        public StatusResponse<bool> GameContextExists(string password) => this.service.GameContextExists(password);

        public Task<StatusResponse> JoinGameContext(GameContextJoinViewModel request) =>
            this.service.JoinGameContext(this.Context.ConnectionId, request);

        public StatusResponse<int> GetWordCount() =>
            this.CallContext(context => context.WordCount);

        public Task<StatusResponse> AddPlayer(Player player) =>
            this.CallContext(context => context.AddPlayer(player));

        public Task<StatusResponse> SetTeamName(TeamNameViewModel teamName) =>
            this.CallContext(context => context.SetTeamName(teamName));

        public StatusResponse StartPeriod(DateTimeOffset timestamp) =>
            this.CallContext(context => context.Game.StartPeriod(timestamp));

        public StatusResponse NextWord(DateTimeOffset timestamp) =>
            this.CallContext(context => context.Game.NextWord(timestamp));

        public StatusResponse AddScore(ScoreViewModel score) =>
            this.CallContext(context => context.Game.AddScore(score.Map()));

        public StatusResponse RevokeLastScore() =>
            this.CallContext(context => context.Game.RevokeLastScore());

        public StatusResponse FinishPeriod(DateTimeOffset timestamp) =>
            this.CallContext(context => context.Game.FinishPeriod(timestamp));

        private StatusResponse CallContext(Action<GameContext> action)
        {
             var context = this.service.GetContext(this.Context.ConnectionId);

            if (context is null) return new(StatusCode.ConcurrencyError);

            action(context);

            return new(StatusCode.Ok);
        }

        private async Task<StatusResponse> CallContext(Func<GameContext, Task> action)
        {
            var context = this.service.GetContext(this.Context.ConnectionId);

            if (context is null) return new(StatusCode.ConcurrencyError);

            await action(context);

            return new(StatusCode.Ok);
        }

        private StatusResponse<T> CallContext<T>(Func<GameContext, T> action) where T : notnull
        {
            var context = this.service.GetContext(this.Context.ConnectionId);

            if (context is null) return new(StatusCode.ConcurrencyError);

            return new(StatusCode.Ok, action(context));
        }
    }
}