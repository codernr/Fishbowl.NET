using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Fishbowl.Net.Client.Online.Components.Screens;
using Fishbowl.Net.Client.Shared;
using Fishbowl.Net.Client.Shared.Components.Screens;
using Fishbowl.Net.Client.Shared.I18n;
using Fishbowl.Net.Client.Shared.Store;
using Fishbowl.Net.Shared.Actions;
using Fishbowl.Net.Shared.ViewModels;
using Fluxor;
using Microsoft.Extensions.Localization;
using MudBlazor;

namespace Fishbowl.Net.Client.Online.Store
{
    [FeatureState]
    public record GamePlayState
    {
        public string? Username { get; init; }

        public string? Password { get; init; }

        public GameSetupViewModel Setup { get; init; } = default!;

        public int ReadyPlayerCount { get; init; }

        public int ConnectedPlayerCount { get; init; }

        public int TotalPlayerCount { get; init; }

        public List<TeamViewModel> Teams { get; init; } = new();

        public TeamViewModel Team => this.Teams.First(team =>
            team.Players.Any(player => player.Username == this.Username));

        public PlayerViewModel? TeamSetupPlayer { get; init; }
    }

    public record SetupCreateGameContextAction(string Username, string Password);

    public static class GamePlayReducers
    {
        [ReducerMethod]
        public static GamePlayState OnReceiveWaitForOtherPlayers(GamePlayState state, ReceiveWaitForOtherPlayersAction action) =>
            state with { Username = action.Username };

        [ReducerMethod]
        public static GamePlayState OnReceiveRestoreState(GamePlayState state, ReceiveRestoreStateAction action) =>
            state with { Username = action.Player.Username, Teams = action.Teams };

        [ReducerMethod]
        public static GamePlayState OnJoinGameContext(GamePlayState state, JoinGameContextAction action) =>
            state with { Username = action.Username, Password = action.Password };

        [ReducerMethod]
        public static GamePlayState OnSetupCreateGameContext(GamePlayState state, SetupCreateGameContextAction action) =>
            state with { Username = action.Username, Password = action.Password };

        [ReducerMethod]
        public static GamePlayState OnReceiveGameSetup(GamePlayState state, ReceiveGameSetupAction action) =>
            state with { Setup = action with { } as GameSetupViewModel };

        [ReducerMethod]
        public static GamePlayState OnReceivePlayerCount(GamePlayState state, ReceivePlayerCountAction action) =>
            state with
            { 
                ReadyPlayerCount = action.SetupCount,
                ConnectedPlayerCount = action.ConnectedCount,
                TotalPlayerCount = action.TotalCount
            };

        [ReducerMethod]
        public static GamePlayState OnReceiveSetTeamName(GamePlayState state, ReceiveSetTeamNameAction action) =>
            state with { Teams = action.Teams };

        [ReducerMethod]
        public static GamePlayState OnReceiveTeamName(GamePlayState state, ReceiveTeamNameAction action)
        {
            state.Teams[action.Id] = state.Teams[action.Id] with { Name = action.Name };
            return state with { Teams = new(state.Teams) };
        }

        [ReducerMethod]
        public static GamePlayState OnReceiveWaitForTeamSetup(GamePlayState state, ReceiveWaitForTeamSetupAction action) =>
            state with { TeamSetupPlayer = action.SetupPlayer, Teams = action.Teams };
    }

    public class GamePlayEffects
    {
        private readonly IState<GamePlayState> state;

        private readonly IStringLocalizer<Resources> localizer;

        private readonly ISnackbar snackbar;

        private string PlayerCountMessage => string.Format(
            this.localizer["Components.States.Common.PlayerCount"],
            this.state.Value.TotalPlayerCount,
            this.state.Value.ConnectedPlayerCount,
            this.state.Value.ReadyPlayerCount);

        public GamePlayEffects(
            IState<GamePlayState> state,
            IStringLocalizer<Resources> localizer,
            ISnackbar snackbar) =>
            (this.state, this.localizer, this.snackbar) = (state, localizer, snackbar);

        [EffectMethod(typeof(ConnectionStartedAction))]
        public Task OnConnectionStarted(IDispatcher dispatcher)
        {
            var username = this.state.Value.Username;
            var password = this.state.Value.Password;

            if (username is not null && password is not null)
            {
                dispatcher.Dispatch(new JoinGameContextAction(password, username));
                return Task.CompletedTask;
            }

            return dispatcher.DispatchTransition<UsernamePassword>();
        }

        [EffectMethod]
        public Task OnConnectionReconnecting(ConnectionReconnectingAction action, IDispatcher dispatcher) =>
            dispatcher.DispatchTransition<Info, SetInfoAction>(new(
                Severity.Error,
                this.localizer["Pages.Play.ErrorTitle"],
                this.localizer["Pages.Play.Reconnecting"],
                true));

        [EffectMethod]
        public Task OnConnectionReconnected(ConnectionReconnectedAction action, IDispatcher dispatcher) =>
            this.state.Value.Username is not null && this.state.Value.Password is not null ?
            dispatcher.Dispatch<JoinGameContextAction>(new(this.state.Value.Password, this.state.Value.Username)) :
            dispatcher.DispatchTransition<UsernamePassword>();

        [EffectMethod]
        public Task OnConnectionClosed(ConnectionClosedAction action, IDispatcher dispatcher) =>
            dispatcher.DispatchTransition<ConnectionClosed>();

        [EffectMethod]
        public Task OnStatusError(StatusErrorAction action, IDispatcher dispatcher)
        {
            this.snackbar.Add(this.localizer[$"Pages.Play.StatusCode.{action.Status}"]);
            return dispatcher.DispatchTransition<UsernamePassword>();
        }

        [EffectMethod]
        public Task OnReceiveGameSetup(ReceiveGameSetupAction action, IDispatcher dispatcher) =>
            dispatcher.DispatchTransition<PlayerWords>();

        [EffectMethod(typeof(ReceiveWaitForOtherPlayersAction))]
        public Task OnReceiveWaitForOtherPlayers(IDispatcher dispatcher) =>
            dispatcher.DispatchTransition<Info, SetInfoAction>(new(
                Severity.Info, this.localizer["Components.States.WaitingForPlayers.Title"], this.PlayerCountMessage, true));

        [EffectMethod]
        public Task OnReceiveSetTeamName(ReceiveSetTeamNameAction action, IDispatcher dispatcher) =>
            dispatcher.DispatchTransition<TeamName, SetTeamNameAction>(
                new(this.state.Value.Team, this.localizer["Components.States.TeamName.Title.Online"]));

        [EffectMethod]
        public Task OnReceiveWaitForTeamSetup(ReceiveWaitForTeamSetupAction action, IDispatcher dispatcher) =>
            dispatcher.DispatchTransition<WaitingForTeamNames>();

        [EffectMethod(typeof(ReceiveGameStartedAction))]
        public Task OnReceiveGameStarted(IDispatcher dispatcher) =>
            dispatcher.DispatchTransition<Info, SetInfoAction>(new(
                Title: this.localizer["Pages.Play.GameStartedTitle"],
                Loading: true), TimeSpan.FromSeconds(2));

        [EffectMethod(typeof(ReceiveGameFinishedAction))]
        public Task OnReceiveGameFinished(IDispatcher dispatcher) =>
            dispatcher.DispatchTransition<GameFinished>();

        [EffectMethod]
        public Task OnReceiveRoundStarted(ReceiveRoundStartedAction action, IDispatcher dispatcher) =>
            dispatcher.DispatchTransition<Info, SetInfoAction>(new(
                Severity.Info,
                $"{this.localizer["Pages.Play.RoundStartedTitle"]}: {action.Type}",
                this.localizer[$"Components.States.Common.RoundTypes.{action.Type}.Description"],
                true), TimeSpan.FromSeconds(2));

        [EffectMethod(typeof(ReceiveRoundFinishedAction))]
        public Task OnReceiveRoundFinished(IDispatcher dispatcher) =>
            dispatcher.DispatchTransition<RoundFinished>();

        [EffectMethod]
        public Task OnReceivePeriodSetup(ReceivePeriodSetupAction action, IDispatcher dispatcher) =>
            action.Player.Username == this.state.Value.Username ?
            dispatcher.DispatchTransition<PeriodSetupPlay>() :
            dispatcher.DispatchTransition<PeriodSetupWatch>();

        [EffectMethod]
        public Task OnReceivePeriodStarted(ReceivePeriodStartedAction action, IDispatcher dispatcher) =>
            action.Player.Username == this.state.Value.Username ?
            dispatcher.DispatchTransition<PeriodPlay>() :
            dispatcher.DispatchTransition<PeriodWatch>();

        [EffectMethod(typeof(ReceivePeriodFinishedAction))]
        public Task OnReceivePeriodFinished(IDispatcher dispatcher) =>
            dispatcher.DispatchTransition<PeriodFinished>(TimeSpan.FromSeconds(5));

        [EffectMethod(typeof(ReceiveScoreAddedAction))]
        public Task OnReceiveScoreAdded(IDispatcher dispatcher)
        {
            this.snackbar.Add($"{this.localizer["Pages.Play.Scored"]}", Severity.Success);
            return Task.CompletedTask;
        }

        [EffectMethod(typeof(ReceiveLastScoreRevokedAction))]
        public Task OnReceiveLastScoreRevoked(IDispatcher dispatcher)
        {
            this.snackbar.Add($"{this.localizer["Pages.Play.Scored"]}", Severity.Success);
            return Task.CompletedTask;
        }

        [EffectMethod(typeof(SetupCreateGameContextAction))]
        public Task OnSetupCreateGameContext(IDispatcher dispatcher) =>
            dispatcher.DispatchTransition<GameSetup, SetGameInfoAction>(
                new(this.localizer["Components.States.GameSetup.Info"]));

        [EffectMethod]
        public Task OnSubmitGameSetup(SubmitGameSetupAction action, IDispatcher dispatcher) =>
            dispatcher.Dispatch<CreateGameContextAction>(new(
                new(this.state.Value.Password!, this.state.Value.Username!), action.GameSetup));

        [EffectMethod(typeof(CreateGameContextSuccessAction))]
        public Task OnCreateGameContextSuccess(IDispatcher dispatcher)
        {
            this.snackbar.Add(this.localizer["Common.GameCreated"], Severity.Success);
            return Task.CompletedTask;
        }

        [EffectMethod]
        public Task OnReceiveGameAbort(ReceiveGameAbortAction action, IDispatcher dispatcher) => Task.WhenAll(
            dispatcher.DispatchTransition<Info, SetInfoAction>(new(
                Severity.Error,
                this.localizer["Pages.Play.ErrorTitle"],
                this.localizer[action.MessageKey])),
            dispatcher.Dispatch<ReloadAction>());
    }
}