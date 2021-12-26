using System;
using System.Threading.Tasks;
using Fishbowl.Net.Client.Online.Services;
using Fishbowl.Net.Client.Shared.Store;
using Fishbowl.Net.Shared;
using Fishbowl.Net.Shared.Actions;
using Fishbowl.Net.Shared.ViewModels;
using Fluxor;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.SignalR.Client;
using Microsoft.Extensions.Logging;

namespace Fishbowl.Net.Client.Online.Store
{
    [FeatureState]
    public record ConnetionState
    {
        public HubConnectionState State { get; init; }
    }

    public record SetConnectionStateAction(HubConnectionState State);
    public record ConnectionStartedAction();
    public record CreateGameContextSuccessAction();
    public record StatusErrorAction(StatusCode Status);

    public class ConnectionEffects : IAsyncDisposable
    {
        private readonly NavigationManager navigationManager;

        private readonly ILogger<ConnectionEffects> logger;

        private HubConnection connection = default!;

        private IDispatcher dispatcher = default!;

        public ConnectionEffects(
            NavigationManager navigationManager,
            ILogger<ConnectionEffects> logger) =>
            (this.navigationManager, this.logger) =
            (navigationManager, logger);

        [EffectMethod(typeof(StoreInitializedAction))]
        public Task Initialize(IDispatcher dispatcher)
        {
            this.dispatcher = dispatcher;

            this.connection = new HubConnectionBuilder()
                .WithUrl(this.navigationManager.ToAbsoluteUri("/game"))
                .WithAutomaticReconnect()
                .Build();

            this.connection.Reconnecting += Dispatch;
            this.connection.Reconnected += Dispatch;
            this.connection.Closed += Dispatch;

            this.SetHandlers();

            return this.StartAsync();

            Task Dispatch(object? _)
            {
                this.dispatcher.Dispatch(new SetConnectionStateAction(this.connection.State));
                return Task.CompletedTask;
            }
        }

        private void SetHandlers()
        {
            this
                .On<ReceiveGameSetupAction>(nameof(IGameClient.ReceiveGameSetup))
                .On<ReceivePlayerCountAction>(nameof(IGameClient.ReceivePlayerCount))
                .On<ReceiveWaitForOtherPlayersAction>(nameof(IGameClient.ReceiveWaitForOtherPlayers))
                .On<ReceiveSetTeamNameAction>(nameof(IGameClient.ReceiveSetTeamName))
                .On<ReceiveWaitForTeamSetupAction>(nameof(IGameClient.ReceiveWaitForTeamSetup))
                .On<ReceiveTeamNameAction>(nameof(IGameClient.ReceiveTeamName))
                .On<ReceiveRestoreStateAction>(nameof(IGameClient.ReceiveRestoreState))
                .On<ReceiveGameAbortAction>(nameof(IGameClient.ReceiveGameAborted))
                .On<ReceiveGameStartedAction>(nameof(IGameClient.ReceiveGameStarted))
                .On<ReceiveGameFinishedAction>(nameof(IGameClient.ReceiveGameFinished))
                .On<ReceiveRoundStartedAction>(nameof(IGameClient.ReceiveRoundStarted))
                .On<ReceiveRoundFinishedAction>(nameof(IGameClient.ReceiveRoundFinished))
                .On<ReceivePeriodSetupAction>(nameof(IGameClient.ReceivePeriodSetup))
                .On<ReceivePeriodStartedAction>(nameof(IGameClient.ReceivePeriodStarted))
                .On<ReceivePeriodFinishedAction>(nameof(IGameClient.ReceivePeriodFinished))
                .On<ReceiveWordSetupAction>(nameof(IGameClient.ReceiveWordSetup))
                .On<ReceiveScoreAddedAction>(nameof(IGameClient.ReceiveScoreAdded))
                .On<ReceiveLastScoreRevokedAction>(nameof(IGameClient.ReceiveLastScoreRevoked));
        }

        private async Task StartAsync()
        {
            await this.connection.StartAsync();

            this.dispatcher.Dispatch(new SetConnectionStateAction(this.connection.State));
            this.dispatcher.Dispatch(new ConnectionStartedAction());
        }

        [EffectMethod]
        public async Task OnJoinGameContext(JoinGameContextAction action, IDispatcher dispatcher)
        {
            var response = await this.connection.InvokeAsync<StatusResponse>("JoinGameContext", action);

            if (response.Status == StatusCode.Ok) return;

            dispatcher.Dispatch(new StatusErrorAction(response.Status));
        }

        [EffectMethod]
        public async Task OnCreateGameContext(CreateGameContextAction action, IDispatcher dispatcher)
        {
            var response = await this.connection.InvokeAsync<StatusResponse>("CreateGameContext", action);

            if (response.Status == StatusCode.Ok)
            {
                dispatcher.Dispatch(new CreateGameContextSuccessAction());
                return;
            }

            dispatcher.Dispatch(new StatusErrorAction(response.Status));
        }

        [EffectMethod]
        public async Task OnAddPlayer(AddPlayerAction action, IDispatcher dispatcher)
        {
            await this.connection.InvokeAsync<StatusResponse>("AddPlayer", action);
        }

        [EffectMethod]
        public async Task OnSubmitTeamName(SubmitTeamNameAction action, IDispatcher dispatcher)
        {
            await this.connection.InvokeAsync<StatusResponse>("SubmitTeamName", action);
        }

        [EffectMethod]
        public async Task OnAddScore(AddScoreAction action, IDispatcher dispatcher)
        {
            await this.SendAsync("AddScore", action.Word);
            await this.SendAsync("NextWord");
        }

        [EffectMethod(typeof(RevokeLastScoreAction))]
        public Task OnRevokeLastScore(IDispatcher dispatcher) =>
            this.SendAsync("RevokeLastScore");

        [EffectMethod(typeof(FinishPeriodAction))]
        public Task OnFinishPeriod(IDispatcher dispatcher) =>
            this.SendAsync("FinishPeriod");

        private ConnectionEffects On<T>(string methodName)
        {
            this.connection.On<T>(methodName, (T param) => this.dispatcher.Dispatch(param));
            this.connection.On<T>(methodName, data => this.logger.LogInformation("{MethodName}: {Data}", methodName, data));
            return this;
        }

        private Task SendAsync(string methodName, object? arg = null)
        {
            this.logger.LogInformation("{MethodName}: {Data}", methodName, arg);
            return arg is null ? this.connection.SendAsync(methodName) : this.connection.SendAsync(methodName, arg);
        }

        public ValueTask DisposeAsync() => this.connection.DisposeAsync();
    }
}