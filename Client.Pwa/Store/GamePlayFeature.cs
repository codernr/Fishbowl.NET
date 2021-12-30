using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Fishbowl.Net.Client.Pwa.Common;
using Fishbowl.Net.Client.Pwa.Components.Screens;
using Fishbowl.Net.Client.Shared;
using Fishbowl.Net.Client.Shared.Components.Screens;
using Fishbowl.Net.Client.Shared.I18n;
using Fishbowl.Net.Client.Shared.Store;
using Fishbowl.Net.Shared;
using Fishbowl.Net.Shared.Actions;
using Fishbowl.Net.Shared.GameEntities;
using Fishbowl.Net.Shared.ViewModels;
using Fluxor;
using Microsoft.Extensions.Localization;

namespace Fishbowl.Net.Client.Pwa.Store
{
    [FeatureState]
    public record GamePlayState
    {
        public GameSetupViewModel GameSetup { get; init; } = default!;

        public List<Player> Players { get; init; } = new();
        
        public List<Team> Teams { get; init; } = new();

        public Game Game { get; init; } = default!;
    }

    public record StartNewGameAction();

    public record RestoreGameAction();

    public record SetRestoredGameAction(Game Game);

    public record CreateTeamsAction();

    public record StartGameAction();

    public static class GamePlayReducers
    {
        [ReducerMethod]
        public static GamePlayState OnSubmitGameSetup(GamePlayState state, SubmitGameSetupAction action) =>
            state with { GameSetup = action.GameSetup };

        [ReducerMethod]
        public static GamePlayState OnSubmitPlayerSetup(GamePlayState state, SubmitPlayerSetupAction action)
        {
            var players = new List<Player>(state.Players);

            players.Add(new Player(
                action.PlayerName,
                action.Words.Select(word => new Word(Guid.NewGuid(), word)).ToList()));

            return state with { Players = players };
        }

        [ReducerMethod(typeof(CreateTeamsAction))]
        public static GamePlayState OnCreateTeams(GamePlayState state) =>
            state with { Teams = state.Players.Randomize().ToList().CreateTeams(state.GameSetup.TeamCount).ToList() };

        [ReducerMethod]
        public static GamePlayState OnSubmitTeamName(GamePlayState state, SubmitTeamNameAction action)
        {
            var teams = state.Teams;

            teams[action.Id].Name = action.Name;

            return state with { Teams = teams };
        }

        [ReducerMethod(typeof(StartGameAction))]
        public static GamePlayState OnStartGame(GamePlayState state) =>
            state with { Game = new(
                Guid.NewGuid(), new(), state.Teams, state.GameSetup.RoundTypes, words => new ShuffleList<Word>(words)) };

        [ReducerMethod]
        public static GamePlayState OnSetRestoredGame(GamePlayState state, SetRestoredGameAction action) =>
            state with { Game = action.Game };
    }

    public class GamePlayEffects
    {
        private readonly IState<GamePlayState> state;

        private readonly GameProperty persistedGame;

        private readonly IStringLocalizer<Resources> localizer;

        private Game Game => this.state.Value.Game;

        public GamePlayEffects(
            IState<GamePlayState> state,
            GameProperty persistedGame,
            IStringLocalizer<Resources> localizer) =>
            (this.state, this.persistedGame, this.localizer) =
            (state, persistedGame, localizer);

        [EffectMethod(typeof(StoreInitializedAction))]
        public Task OnInitGamePlay(IDispatcher dispatcher) =>
            this.persistedGame.Value is null ?
            dispatcher.Dispatch<StartNewGameAction>() :
            dispatcher.DispatchTransition<Restore>();

        [EffectMethod(typeof(RestoreGameAction))]
        public Task OnRestoreGame(IDispatcher dispatcher) =>
            dispatcher.Dispatch<SetRestoredGameAction>(new(
                this.persistedGame.Value ?? throw new NullReferenceException("Persisted game is null.")));

        [EffectMethod(typeof(SetRestoredGameAction))]
        public Task OnSetRestoredGameAction(IDispatcher dispatcher)
        {
            this.SetEventHandlers(dispatcher);
            this.Game.Restore();
            return Task.CompletedTask;
        }

        [EffectMethod(typeof(StartNewGameAction))]
        public Task OnStartNewGame(IDispatcher dispatcher) =>
            dispatcher.DispatchTransition<GameSetup>();

        [EffectMethod]
        public Task OnSubmitGameSetup(SubmitGameSetupAction action, IDispatcher dispatcher) =>
            dispatcher.DispatchTransition<PlayerSetup, SetPlayerSetupAction>(
                new(0, action.GameSetup.WordCount));

        [EffectMethod]
        public Task OnSubmitPlayerSetup(SubmitPlayerSetupAction action, IDispatcher dispatcher) =>
            this.state.Value.Players.Count < this.state.Value.GameSetup.PlayerCount ?
                dispatcher.DispatchTransition<PlayerSetup, SetPlayerSetupAction>(
                    new(this.state.Value.Players.Count, this.state.Value.GameSetup.WordCount)) :
                dispatcher.Dispatch<CreateTeamsAction>();

        [EffectMethod(typeof(CreateTeamsAction))]
        public Task OnCreateTeams(IDispatcher dispatcher) => OnSubmitTeamName(dispatcher);

        [EffectMethod(typeof(SubmitTeamNameAction))]
        public Task OnSubmitTeamName(IDispatcher dispatcher)
        {
            var teams = this.state.Value.Teams;

            var nextTeam = teams.FirstOrDefault(team => team.Name is null);
            var nextTeamIndex = teams.Where(team => team.Name is not null).Count() + 1;

            return nextTeam is null ?
                dispatcher.Dispatch<StartGameAction>() :
                dispatcher.DispatchTransition<TeamName, SetTeamNameAction>(new(
                    nextTeam.Map(), string.Format(this.localizer["Components.States.TeamName.Title.Pwa"], nextTeamIndex)));
        }

        [EffectMethod(typeof(StartGameAction))]
        public Task OnStartGame(IDispatcher dispatcher)
        {
            this.SetEventHandlers(dispatcher);

            this.Game.Start();

            return Task.CompletedTask;
        }

        [EffectMethod(typeof(StartPeriodAction))]
        public Task OnStartPeriod(IDispatcher dispatcher)
        {
            this.state.Value.Game.StartPeriod(DateTimeOffset.UtcNow);
            return Task.CompletedTask;
        }

        [EffectMethod]
        public Task OnAddScore(AddScoreAction action, IDispatcher dispatcher)
        {
            this.Game.AddScore(new(action.Word.Map(), DateTimeOffset.UtcNow));

            return Task.CompletedTask;
        }

        [EffectMethod(typeof(FinishPeriodAction))]
        public Task OnFinishPeriod(IDispatcher dispatcher)
        {
            this.Game.FinishPeriod(DateTimeOffset.UtcNow);
            return Task.CompletedTask;
        }

        [EffectMethod(typeof(RevokeLastScoreAction))]
        public Task OnRevokeLastScore(IDispatcher dispatcher)
        {
            this.Game.RevokeLastScore();

            return Task.CompletedTask;
        }

        [EffectMethod(typeof(ReceiveGameStartedAction))]
        public Task OnReceiveGameStarted(IDispatcher dispatcher) => dispatcher.DispatchTransition<Info, SetInfoAction>(
                new(Title: this.localizer["Pages.Play.GameStartedTitle"], Loading: true), TimeSpan.FromSeconds(2));

        [EffectMethod(typeof(ReceiveGameFinishedAction))]
        public Task OnReceiveGameFinished(IDispatcher dispatcher) =>
            dispatcher.DispatchTransition<GameFinished>();

        [EffectMethod]
        public Task OnReceiveRoundStarted(ReceiveRoundStartedAction action, IDispatcher dispatcher) =>
            dispatcher.DispatchTransition<Info, SetInfoAction>(new(
                Title: $"{this.localizer["Pages.Play.RoundStartedTitle"]}: {action.Type}",
                Message: this.localizer[$"Components.States.Common.RoundTypes.{action.Type}.Description"],
                Loading: true), TimeSpan.FromSeconds(2));

        [EffectMethod(typeof(ReceiveRoundFinishedAction))]
        public Task OnReceiveRoundFinished(IDispatcher dispatcher) =>
            dispatcher.DispatchTransition<RoundFinished>(TimeSpan.FromSeconds(5));

        [EffectMethod]
        public Task OnReceivePeriodSetup(ReceivePeriodSetupAction action, IDispatcher dispatcher) =>
            dispatcher.DispatchTransition<PeriodSetupPlay, IncrementPeriodCurrentIdAction>(new());

        [EffectMethod]
        public Task OnReceivePeriodStarted(ReceivePeriodStartedAction action, IDispatcher dispatcher) =>
            dispatcher.DispatchTransition<PeriodPlay>();

        [EffectMethod(typeof(ReceivePeriodFinishedAction))]
        public Task OnReceivePeriodFinished(IDispatcher dispatcher) =>
            dispatcher.DispatchTransition<PeriodFinished>(TimeSpan.FromSeconds(5));

        private void SetEventHandlers(IDispatcher dispatcher)
        {
            this.Game.GameStarted += game => dispatcher.Dispatch<ReceiveGameStartedAction>(new());

            this.Game.GameFinished += game => dispatcher.Dispatch<ReceiveGameFinishedAction>(new(game.Map()));

            this.Game.RoundStarted += round => dispatcher.Dispatch<ReceiveRoundStartedAction>(new(round.Type));

            this.Game.RoundFinished += round => dispatcher.Dispatch<ReceiveRoundFinishedAction>(round.MapSummary());

            this.Game.PeriodSetup += period => dispatcher.Dispatch<ReceivePeriodSetupAction>(
                    new(period.Map(this.Game.Rounds.Current)));

            this.Game.PeriodStarted += period => dispatcher.Dispatch<ReceivePeriodStartedAction>(
                    new(period.MapRunning(this.Game.Rounds.Current)));

            this.Game.PeriodFinished += period => dispatcher.Dispatch<ReceivePeriodFinishedAction>(
                    new(period.Map()));

            this.Game.WordSetup += (player, word) => dispatcher.Dispatch<ReceiveWordSetupAction>(
                new(word.Map()));

            this.Game.ScoreAdded += score => dispatcher.Dispatch<ReceiveScoreAddedAction>(new(score.Map()));

            this.Game.LastScoreRevoked += score => dispatcher.Dispatch<ReceiveLastScoreRevokedAction>(new(score.Map()));

            this.Game.TimerUpdate += remainig => dispatcher.Dispatch<ReceiveTimerUpdateAction>(new(remainig));
        }
    }
}