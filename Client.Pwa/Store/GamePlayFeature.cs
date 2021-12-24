using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Fishbowl.Net.Client.Pwa.Common;
using Fishbowl.Net.Client.Pwa.Components.States;
using Fishbowl.Net.Client.Shared.Components.States;
using Fishbowl.Net.Client.Shared.I18n;
using Fishbowl.Net.Client.Shared.Store;
using Fishbowl.Net.Shared;
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

    public record InitGamePlayAction();

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

            teams[action.Id].Name = action.TeamName;

            return state with { Teams = teams };
        }

        [ReducerMethod(typeof(StartGameAction))]
        public static GamePlayState OnStartGame(GamePlayState state) =>
            state with { Game = new(new(), state.Teams, state.GameSetup.RoundTypes) };
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

        [EffectMethod(typeof(InitGamePlayAction))]
        public Task OnInitGamePlay(IDispatcher dispatcher)
        {
            var nextState = this.persistedGame.Value is null ? typeof(GameSetup) : typeof(Restore);
            dispatcher.Dispatch(new StartStateManagerTransitionAction(nextState));
            return Task.CompletedTask;
        }

        [EffectMethod]
        public Task OnSubmitGameSetup(SubmitGameSetupAction action, IDispatcher dispatcher)
        {
            dispatcher.Dispatch(new SetPlayerSetupAction(
                action.GameSetup.WordCount,
                string.Format(this.localizer["Components.States.PlayerSetup.Title"], 1)));

            dispatcher.Dispatch(new StartStateManagerTransitionAction(typeof(PlayerSetup)));

            return Task.CompletedTask;
        }

        [EffectMethod]
        public Task OnSubmitPlayerSetup(SubmitPlayerSetupAction action, IDispatcher dispatcher)
        {
            if (this.state.Value.Players.Count < this.state.Value.GameSetup.PlayerCount)
            {
                dispatcher.Dispatch(new SetPlayerSetupAction(
                    this.state.Value.GameSetup.WordCount,
                    string.Format(this.localizer["Components.States.PlayerSetup.Title"], this.state.Value.Players.Count + 1)));

                dispatcher.Dispatch(new StartStateManagerTransitionAction(typeof(PlayerSetup)));
            }
            else
            {
                dispatcher.Dispatch(new CreateTeamsAction());
            }

            return Task.CompletedTask;
        }

        [EffectMethod(typeof(CreateTeamsAction))]
        public Task OnCreateTeams(IDispatcher dispatcher)
        {
            dispatcher.Dispatch(new SetTeamNameAction(
                this.state.Value.Teams[0].Map(),
                string.Format(this.localizer["Components.States.TeamName.Title.Pwa"], 1)));

            dispatcher.Dispatch(new StartStateManagerTransitionAction(typeof(TeamName)));

            return Task.CompletedTask;
        }

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
                dispatcher.Dispatch(new SetTeamNameAction(
                    nextTeam.Map(),
                    string.Format(this.localizer["Components.States.TeamName.Title.Pwa"], nextTeamIndex)));

                dispatcher.Dispatch(new StartStateManagerTransitionAction(typeof(TeamName)));
            }

            return Task.CompletedTask;
        }

        [EffectMethod(typeof(StartGameAction))]
        public Task OnStartGame(IDispatcher dispatcher)
        {
            this.Game.GameStarted += game => Dispatch<SetInfoAction, Info>(
                new(Message: this.localizer["Pages.Play.GameStartedTitle"], Loading: true));

            this.Game.GameFinished += game => Dispatch<SetGameFinishedAction, GameFinished>(new(this.Game.Map()));

            this.Game.RoundStarted += round => Dispatch<SetInfoAction, Info>(new(
                Title: $"{this.localizer["Pages.Play.RoundStartedTitle"]}: {round.Type}",
                Message: this.localizer[$"Components.States.Common.RoundTypes.{round.Type}.Description"],
                Loading: true));

            this.Game.RoundFinished += round => Dispatch<SetRoundFinishedAction, RoundFinished>(new(round.MapSummary()));

            this.Game.PeriodSetup += period => Dispatch<SetPeriodSetupPlayAction, PeriodSetupPlay>(
                new(period.Map(this.Game.Game.CurrentRound)));

            this.Game.PeriodStarted += period => Dispatch<SetPeriodPlayPeriodAction, PeriodPlay>(
                new(period.MapRunning(this.Game.Game.CurrentRound)));

            this.Game.PeriodFinished += period => Dispatch<SetPeriodFinishedAction, PeriodFinished>(new(period.Map()));

            this.Game.WordSetup += (player, word) => dispatcher.Dispatch(
                new SetPeriodPlayWordAction(word.Map()));

            this.Game.Run();

            return Task.CompletedTask;

            void Dispatch<TStateAction, TTransition>(TStateAction action)
            {
                dispatcher.Dispatch(action);
                dispatcher.Dispatch(new StartStateManagerTransitionAction(typeof(TTransition)));
            }
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

            dispatcher.Dispatch(new SetPeriodPlayScoreCountAction(
                this.Game.Game.CurrentRound.CurrentPeriod.Scores.Count));

            this.Game.NextWord(DateTimeOffset.UtcNow);

            return Task.CompletedTask;
        }
    }
}