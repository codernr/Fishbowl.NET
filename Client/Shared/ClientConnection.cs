using System;
using System.Threading.Tasks;
using Fishbowl.Net.Client.Services;
using Fishbowl.Net.Shared.GameEntities;
using Fishbowl.Net.Shared.ViewModels;
using Microsoft.AspNetCore.SignalR.Client;
using Microsoft.Extensions.Logging;

namespace Fishbowl.Net.Client.Shared
{
    public class ClientConnection : IAsyncDisposable
    {
        private readonly HubConnection connection;

        private readonly IGameClient client;

        private readonly ILogger<ClientConnection> logger;

        public ClientConnection(Uri uri, IGameClient client, ILogger<ClientConnection> logger)
        {
            this.client = client;
            this.logger = logger;

            this.connection = new HubConnectionBuilder()
                .WithUrl(uri)
                .WithAutomaticReconnect()
                .Build();

            this.Configure();
        }

        private void Configure()
        {
            this.connection.Reconnecting += this.client.Reconnecting;
            this.connection.Reconnected += this.client.Reconnected;
            this.connection.Closed += this.client.Closed;

            this
                .On<GameSetupViewModel>(nameof(client.ReceiveSetupPlayer), client.ReceiveSetupPlayer)
                .On<PlayerCountViewModel>(nameof(client.ReceivePlayerCount), client.ReceivePlayerCount)
                .On<PlayerViewModel>(nameof(client.ReceiveWaitForOtherPlayers), client.ReceiveWaitForOtherPlayers)
                .On<TeamSetupViewModel>(nameof(client.ReceiveSetTeamName), client.ReceiveSetTeamName)
                .On<TeamSetupViewModel>(nameof(client.ReceiveWaitForTeamSetup), client.ReceiveWaitForTeamSetup)
                .On<TeamNameViewModel>(nameof(client.ReceiveTeamName), client.ReceiveTeamName)
                .On<PlayerViewModel>(nameof(client.ReceiveRestoreState), client.ReceiveRestoreState)
                .On<GameAbortViewModel>(nameof(client.ReceiveGameAborted), client.ReceiveGameAborted)
                .On(nameof(client.ReceiveGameStarted), client.ReceiveGameStarted)
                .On<GameSummaryViewModel>(nameof(client.ReceiveGameFinished), client.ReceiveGameFinished)
                .On<RoundViewModel>(nameof(client.ReceiveRoundStarted), client.ReceiveRoundStarted)
                .On<RoundSummaryViewModel>(nameof(client.ReceiveRoundFinished), client.ReceiveRoundFinished)
                .On<PeriodSetupViewModel>(nameof(client.ReceivePeriodSetup), client.ReceivePeriodSetup)
                .On<PeriodRunningViewModel>(nameof(client.ReceivePeriodStarted), client.ReceivePeriodStarted)
                .On<PeriodSummaryViewModel>(nameof(client.ReceivePeriodFinished), client.ReceivePeriodFinished)
                .On<WordViewModel>(nameof(client.ReceiveWordSetup), client.ReceiveWordSetup)
                .On<ScoreViewModel>(nameof(client.ReceiveScoreAdded), client.ReceiveScoreAdded)
                .On<ScoreViewModel>(nameof(client.ReceiveLastScoreRevoked), client.ReceiveLastScoreRevoked);
        }

        public async Task StartAsync()
        {
            await this.connection.StartAsync();

            if (this.connection.State == HubConnectionState.Connected)
            {
                await this.client.Connected();
            }
        }

        public Task StopAsync() => this.connection.StopAsync();

        public Task<StatusResponse<bool>> GameContextExists(string password) =>
            this.connection.InvokeAsync<StatusResponse<bool>>(nameof(this.GameContextExists), password);

        public Task<StatusResponse> JoinGameContext(GameContextJoinViewModel gameContextJoin) =>
            this.connection.InvokeAsync<StatusResponse>(nameof(this.JoinGameContext), gameContextJoin);

        public Task<StatusResponse> CreateGameContext(GameContextSetupViewModel gameContextSetup) =>
            this.connection.InvokeAsync<StatusResponse>(nameof(this.CreateGameContext), gameContextSetup);

        public Task<StatusResponse> AddPlayer(Player player) =>
            this.connection.InvokeAsync<StatusResponse>(nameof(this.AddPlayer), player);

        public Task<StatusResponse> SetTeamName(TeamNameViewModel teamName) =>
            this.connection.InvokeAsync<StatusResponse>(nameof(this.SetTeamName), teamName);

        public Task StartPeriod(DateTimeOffset timestamp) => this.connection.SendAsync(nameof(this.StartPeriod), timestamp);

        public Task AddScore(ScoreViewModel score) => this.connection.SendAsync(nameof(this.AddScore), score);

        public Task NextWord(DateTimeOffset timestamp) => this.connection.SendAsync(nameof(this.NextWord), timestamp);

        public Task RevokeLastScore() => this.connection.SendAsync(nameof(this.RevokeLastScore));

        public Task FinishPeriod(DateTimeOffset timestamp) => this.connection.SendAsync(nameof(this.FinishPeriod), timestamp);

        private ClientConnection On(string methodName, Func<Task> handler)
        {
            this.connection.On(methodName, handler);
            this.connection.On(methodName, () => this.logger.LogInformation("{MethodName}", methodName));
            return this;
        }

        private ClientConnection On<T>(string methodName, Func<T, Task> handler)
        {
            this.connection.On<T>(methodName, handler);
            this.connection.On<T>(methodName, data => this.logger.LogInformation("{MethodName}: {Data}", methodName, data));
            return this;
        }

        public ValueTask DisposeAsync() => this.connection.DisposeAsync();
    }
}