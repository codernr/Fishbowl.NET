using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Fishbowl.Net.Client.Online.Components.Screens;
using Fishbowl.Net.Client.Shared.I18n;
using Fishbowl.Net.Client.Shared.Store;
using Fishbowl.Net.Shared.Actions;
using Fishbowl.Net.Shared.ViewModels;
using Fluxor;
using Microsoft.Extensions.Localization;

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

        public List<ScoreViewModel> PeriodScores { get; init; } = default!;
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
        public static GamePlayState OnReceiveWaitForTeamSetup(GamePlayState state, ReceiveWaitForTeamSetupAction action) =>
            state with { Teams = action.Teams };

        [ReducerMethod]
        public static GamePlayState OnReceivePeriodStarted(GamePlayState state, ReceivePeriodStartedAction action) =>
            state with { PeriodScores = new() };

        [ReducerMethod]
        public static GamePlayState OnReceiveScoreAdded(GamePlayState state, ReceiveScoreAddedAction action)
        {
            state.PeriodScores.Add(action with { } as ScoreViewModel);
            return state with { PeriodScores = new(state.PeriodScores) };
        }

        [ReducerMethod]
        public static GamePlayState OnReceiveLastScoreRevoked(GamePlayState state, ReceiveLastScoreRevokedAction action)
        {
            state.PeriodScores.Remove(action as ScoreViewModel);
            return state with { PeriodScores = new(state.PeriodScores) };
        }
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

            dispatcher.Dispatch(new ScreenManagerTransitionAction(typeof(UsernamePassword)));
            return Task.CompletedTask;
        }

        [EffectMethod]
        public Task OnJoinGameContextError(JoinGameContextErrorAction action, IDispatcher dispatcher)
        {
            dispatcher.Dispatch(new ScreenManagerTransitionAction(typeof(UsernamePassword)));
            return Task.CompletedTask;
        }

        [EffectMethod]
        public Task OnReceiveGameSetup(ReceiveGameSetupAction action, IDispatcher dispatcher)
        {
            dispatcher.Dispatch(new ScreenManagerTransitionAction(
                typeof(PlayerWords), new SetPlayerWordsAction(this.PlayerCountMessage, action.WordCount)));

            return Task.CompletedTask;
        }

        [EffectMethod]
        public Task OnReceivePlayerCount(ReceivePlayerCountAction action, IDispatcher dispatcher)
        {
            dispatcher.Dispatch(new SetInfoMessageAction(this.PlayerCountMessage));
            dispatcher.Dispatch(new SetPlayerWordsMessageAction(this.PlayerCountMessage));
            return Task.CompletedTask;
        }
    }
}