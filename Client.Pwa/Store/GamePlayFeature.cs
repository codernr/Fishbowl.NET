using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Fishbowl.Net.Client.Pwa.Common;
using Fishbowl.Net.Client.Pwa.Components.Screens;
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

        public AsyncGame Game { get; init; } = default!;
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
            state with { Game = new(new(), state.Teams, state.GameSetup.RoundTypes) };

        [ReducerMethod]
        public static GamePlayState OnSetRestoredGame(GamePlayState state, SetRestoredGameAction action) =>
            state with { Game = new(action.Game) };
    }

    public class GamePlayEffects
    {
        private readonly IState<GamePlayState> state;

        private readonly GameProperty persistedGame;

        private readonly IStringLocalizer<Resources> localizer;

        private AsyncGame Game => this.state.Value.Game;

        public GamePlayEffects(
            IState<GamePlayState> state,
            GameProperty persistedGame,
            IStringLocalizer<Resources> localizer) =>
            (this.state, this.persistedGame, this.localizer) =
            (state, persistedGame, localizer);

        [EffectMethod(typeof(StoreInitializedAction))]
        public Task OnInitGamePlay(IDispatcher dispatcher)
        {
            dispatcher.Dispatch(this.persistedGame.Value is null ?
                new StartNewGameAction() :
                new ScreenManagerTransitionAction(typeof(Restore)));
            return Task.CompletedTask;
        }

        [EffectMethod(typeof(RestoreGameAction))]
        public Task OnRestoreGame(IDispatcher dispatcher)
        {
            dispatcher.Dispatch(new SetRestoredGameAction(
                this.persistedGame.Value ?? throw new NullReferenceException("Persisted game is null.")));
            
            return Task.CompletedTask;
        }

        [EffectMethod(typeof(SetRestoredGameAction))]
        public Task OnSetRestoredGameAction(IDispatcher dispatcher)
        {
            this.SetEventHandlers(dispatcher);
            this.Game.Restore();
            return Task.CompletedTask;
        }

        [EffectMethod(typeof(StartNewGameAction))]
        public Task OnStartNewGame(IDispatcher dispatcher)
        {
            dispatcher.Dispatch(new ScreenManagerTransitionAction(typeof(GameSetup)));
            return Task.CompletedTask;
        }

        [EffectMethod]
        public Task OnSubmitGameSetup(SubmitGameSetupAction action, IDispatcher dispatcher)
        {
            dispatcher.Dispatch(new ScreenManagerTransitionAction(
                typeof(PlayerSetup),
                new SetPlayerSetupAction(0, action.GameSetup.WordCount)));

            return Task.CompletedTask;
        }

        [EffectMethod]
        public Task OnSubmitPlayerSetup(SubmitPlayerSetupAction action, IDispatcher dispatcher)
        {
            if (this.state.Value.Players.Count < this.state.Value.GameSetup.PlayerCount)
            {
                dispatcher.Dispatch(new ScreenManagerTransitionAction(
                    typeof(PlayerSetup),
                    new SetPlayerSetupAction(this.state.Value.Players.Count, this.state.Value.GameSetup.WordCount)));
            }
            else
            {
                dispatcher.Dispatch(new CreateTeamsAction());
            }

            return Task.CompletedTask;
        }

        [EffectMethod(typeof(CreateTeamsAction))]
        public Task OnCreateTeams(IDispatcher dispatcher) => OnSubmitTeamName(dispatcher);

        [EffectMethod(typeof(SubmitTeamNameAction))]
        public Task OnSubmitTeamName(IDispatcher dispatcher)
        {
            var teams = this.state.Value.Teams;

            var nextTeam = teams.FirstOrDefault(team => team.Name is null);
            var nextTeamIndex = teams.Where(team => team.Name is not null).Count() + 1;

            if (nextTeam is null)
            {
                dispatcher.Dispatch(new StartGameAction());
            }
            else
            {
                dispatcher.Dispatch(new ScreenManagerTransitionAction(
                    typeof(TeamName),
                    new SetTeamNameAction(
                        nextTeam.Map(),
                        string.Format(this.localizer["Components.States.TeamName.Title.Pwa"], nextTeamIndex))));
            }

            return Task.CompletedTask;
        }

        [EffectMethod(typeof(StartGameAction))]
        public Task OnStartGame(IDispatcher dispatcher)
        {
            this.SetEventHandlers(dispatcher);

            this.Game.Run();

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

            dispatcher.Dispatch(new SetPeriodScoreCountAction(
                this.Game.Game.CurrentRound.CurrentPeriod.Scores.Count));

            this.Game.NextWord(DateTimeOffset.UtcNow);

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
            var score = this.Game.Game.CurrentRound.CurrentPeriod.Scores.Last();
            
            this.Game.RevokeLastScore();

            dispatcher.Dispatch(new SetPeriodScoreCountAction(
                this.Game.Game.CurrentRound.CurrentPeriod.Scores.Count));

            dispatcher.Dispatch(
                new SetPeriodWordAction(score.Word.Map()));

            return Task.CompletedTask;
        }

        private void SetEventHandlers(IDispatcher dispatcher)
        {
            this.Game.GameStarted += game => Dispatch<SetInfoAction, Info>(
                new(Title: this.localizer["Pages.Play.GameStartedTitle"], Loading: true), TimeSpan.FromSeconds(2));

            this.Game.GameFinished += game => Dispatch<ReceiveGameFinishedAction, GameFinished>(game.Map());

            this.Game.RoundStarted += round => Dispatch<SetInfoAction, Info>(new(
                Title: $"{this.localizer["Pages.Play.RoundStartedTitle"]}: {round.Type}",
                Message: this.localizer[$"Components.States.Common.RoundTypes.{round.Type}.Description"],
                Loading: true), TimeSpan.FromSeconds(2));

            this.Game.RoundFinished += round => Dispatch<ReceiveRoundFinishedAction, RoundFinished>(
                round.MapSummary(), TimeSpan.FromSeconds(5));

            this.Game.PeriodSetup += period => Dispatch<ReceivePeriodSetupAction, PeriodSetupPlay>(
                period.Map(this.Game.Game.CurrentRound));

            this.Game.PeriodStarted += period => Dispatch<ReceivePeriodStartedAction, PeriodPlay>(
                period.MapRunning(this.Game.Game.CurrentRound));

            this.Game.PeriodFinished += period => Dispatch<ReceivePeriodFinishedAction, PeriodFinished>(
                period.Map(), TimeSpan.FromSeconds(5));

            this.Game.WordSetup += (player, word) => dispatcher.Dispatch(
                word.Map());

            void Dispatch<TStateAction, TTransition>(TStateAction action, TimeSpan delay = default) =>
                dispatcher.Dispatch(new ScreenManagerTransitionAction(typeof(TTransition), action, delay));
        }
    }
}