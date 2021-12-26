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

        public List<TeamViewModel> Teams { get; init; } = default!;

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
        public static GamePlayState OnSubmitGameSetup(GamePlayState state, SubmitGameSetupAction action) =>
            state with { Setup = action.GameSetup };

        [ReducerMethod]
        public static GamePlayState OnReceivePlayerCount(GamePlayState state, ReceivePlayerCountAction action) =>
            state with { ReadyPlayerCount = action.SetupCount, ConnectedPlayerCount = action.ConnectedCount };

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

        private string PlayerCountMessage => string.Format(
            this.localizer["Components.States.Common.PlayerCount"],
            this.state.Value.Setup.PlayerCount,
            this.state.Value.ConnectedPlayerCount,
            this.state.Value.ReadyPlayerCount);

        public GamePlayEffects(
            IState<GamePlayState> state,
            IStringLocalizer<Resources> localizer) =>
            (this.state, this.localizer) = (state, localizer);

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
        public Task OnJoinGameContextError(JoinGameContextErrorAction action, IDispatcher dispatcher) =>
            dispatcher.DispatchTransition<UsernamePassword>();

        [EffectMethod]
        public Task OnReceiveGameSetup(ReceiveGameSetupAction action, IDispatcher dispatcher) =>
            dispatcher.DispatchTransition<PlayerWords, SetPlayerWordsAction>(
                new(this.PlayerCountMessage, action.WordCount));

        [EffectMethod]
        public Task OnReceivePlayerCount(ReceivePlayerCountAction action, IDispatcher dispatcher)
        {
            dispatcher.Dispatch(new SetInfoMessageAction(this.PlayerCountMessage));
            dispatcher.Dispatch(new SetPlayerWordsMessageAction(this.PlayerCountMessage));
            return Task.CompletedTask;
        }

        [EffectMethod(typeof(ReceiveWaitForOtherPlayersAction))]
        public Task OnReceiveWaitForOtherPlayers(IDispatcher dispatcher) =>
            dispatcher.DispatchTransition<Info, SetInfoAction>(new(
                Severity.Info, this.localizer["Components.States.WaitingForPlayers.Title"], this.PlayerCountMessage, true));

        [EffectMethod(typeof(ReceiveGameStartedAction))]
        public Task OnReceiveGameStarted(IDispatcher dispatcher) =>
            dispatcher.DispatchTransition<Info, SetInfoAction>(new(
                Title: this.localizer["Pages.Play.GameStartedTitle"],
                Loading: true));

        [EffectMethod]
        public Task OnReceiveGameFinished(ReceiveGameFinishedAction action, IDispatcher dispatcher) =>
            dispatcher.DispatchTransition<GameFinished>();

        [EffectMethod]
        public Task OnReceiveRoundFinished(ReceiveRoundFinishedAction action, IDispatcher dispatcher) =>
            dispatcher.DispatchTransition<RoundFinished>();

        [EffectMethod]
        public Task OnReceivePeriodSetup(ReceivePeriodSetupAction action, IDispatcher dispatcher) =>
            action.Player.Username == this.state.Value.Username ?
            dispatcher.DispatchTransition<PeriodSetupPlay>() :
            dispatcher.DispatchTransition<PeriodSetupWatch>();
    }
}