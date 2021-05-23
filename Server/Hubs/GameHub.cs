using System;
using System.Collections.Generic;
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

        private GameContext GameContext => this.service.GetContext(this.Context.ConnectionId);

        public override async Task OnDisconnectedAsync(System.Exception? exception)
        {
            await this.service.RemoveConnection(this.Context.ConnectionId);
            await base.OnDisconnectedAsync(exception);
        }

        public Task<StatusResponse> CreateGameContext(GameContextSetupViewModel request) =>
            Catch<KeyNotFoundException>(() =>
                this.service.CreateGameContext(this.Context.ConnectionId, request), StatusCode.ConcurrencyError);

        public bool GameContextExists(string password) => this.service.GameContextExists(password);

        public Task<StatusResponse> JoinGameContext(GameContextJoinViewModel request) =>
            Catch<KeyNotFoundException>(() =>
                this.service.JoinGameContext(this.Context.ConnectionId, request), StatusCode.ConcurrencyError);

        public StatusResponse<int> GetWordCount() =>
            Catch<KeyNotFoundException, int>(() => this.GameContext.WordCount, StatusCode.ConcurrencyError);

        public Task<StatusResponse> AddPlayer(Player player) =>
            Catch<KeyNotFoundException>(() => this.GameContext.AddPlayer(player), StatusCode.ConcurrencyError);

        public Task<StatusResponse> SetTeamName(TeamNameViewModel teamName) =>
            Catch<KeyNotFoundException>(() =>this.GameContext.SetTeamName(teamName), StatusCode.ConcurrencyError);

        public StatusResponse StartPeriod(DateTimeOffset timestamp) =>
            Catch<KeyNotFoundException>(() => this.GameContext.Game.StartPeriod(timestamp), StatusCode.ConcurrencyError);

        public StatusResponse NextWord(DateTimeOffset timestamp) =>
            Catch<KeyNotFoundException>(() => this.GameContext.Game.NextWord(timestamp), StatusCode.ConcurrencyError);

        public StatusResponse AddScore(ScoreViewModel score) =>
            Catch<KeyNotFoundException>(() => this.GameContext.Game.AddScore(score.Map()), StatusCode.ConcurrencyError);

        public StatusResponse RevokeLastScore() =>
            Catch<KeyNotFoundException>(() => this.GameContext.Game.RevokeLastScore(), StatusCode.ConcurrencyError);

        public StatusResponse FinishPeriod(DateTimeOffset timestamp) =>
            Catch<KeyNotFoundException>(() => this.GameContext.Game.FinishPeriod(timestamp), StatusCode.ConcurrencyError);

        private static StatusResponse<TResult> Catch<TException, TResult>(Func<TResult> action, StatusCode catchCode)
            where TResult : notnull where TException : Exception
        {
            try
            {
                return new(StatusCode.Ok, action());
            }
            catch (TException)
            {
                return new(catchCode);
            }
        }

        private static async Task<StatusResponse> Catch<TException>(Func<Task<StatusCode>> action, StatusCode catchCode)
            where TException : Exception
        {
            try
            {
                return new(await action());
            }
            catch (TException)
            {
                return new(catchCode);
            }
        }

        private static async Task<StatusResponse> Catch<TException>(Func<Task> action, StatusCode catchCode)
            where TException : Exception
        {
            try
            {
                await action();
                return new(StatusCode.Ok);
            }
            catch (TException)
            {
                return new(catchCode);
            }
        }

        private static StatusResponse Catch<TException>(Action action, StatusCode catchCode)
            where TException : Exception
        {
            try
            {
                action();
                return new(StatusCode.Ok);
            }
            catch (TException)
            {
                return new(catchCode);
            }
        }
    }
}