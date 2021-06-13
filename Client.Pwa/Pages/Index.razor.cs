using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Fishbowl.Net.Client.Pwa.Components.States;
using Fishbowl.Net.Client.Shared.Common;
using Fishbowl.Net.Client.Shared.Components;
using Fishbowl.Net.Client.Shared.Components.States;
using Fishbowl.Net.Shared;
using Fishbowl.Net.Shared.Exceptions;
using Fishbowl.Net.Shared.GameEntities;
using Fishbowl.Net.Shared.ViewModels;

namespace Fishbowl.Net.Client.Pwa.Pages
{
    public partial class Index
    {
        private StateManager? stateManager;

        private StateManager StateManager => this.stateManager ?? throw new InvalidOperationException();

        private Toast? playerSetupDisplay;

        private Toast PlayerSetupDisplay => this.playerSetupDisplay ?? throw new InvalidOperationException();

        private int playerCount = 4;

        private int teamCount = 2;

        private int wordCount = 1;

        private string[] roundTypes = new string[0];

        private List<Player> players = new();

        private List<Team> teams = new();

        private string currentPlayerName = string.Empty;

        private AsyncGame Game => this.game ?? throw new InvalidOperationException();

        private AsyncGame? game;

        private Task transition = Task.CompletedTask;

        private string L(string key) => this.StringLocalizer[key] ?? key;

        protected override async Task OnAfterRenderAsync(bool firstRender)
        {
            if (!firstRender) return;
            
            await (this.PersistedGame.Value is not null ?
                this.Transition<Restore>() :
                this.Transition<PlayerCount>());
        }

        private Task SetPlayerCount(int playerCount)
        {
            this.playerCount = playerCount;

            return this.StateManager.SetStateAsync<TeamCount>(teamCount => teamCount.MaxTeamCount = playerCount / 2);
        }

        private Task SetTeamCount(int teamCount)
        {
            this.teamCount = teamCount;

            return this.StateManager.SetStateAsync<WordCount>();
        }

        private Task SetWordCount(int wordCount)
        {
            this.wordCount = wordCount;

            return this.StateManager.SetStateAsync<RoundTypes>();
        }

        private Task SetRoundTypes(string[] roundTypes)
        {
            this.roundTypes = roundTypes;

            return Task.WhenAll(
                this.StateManager.SetStateAsync<PlayerName>(),
                this.PlayerSetupDisplay.Show());
        }

        private Task SetPlayerName(string name)
        {
            this.currentPlayerName = name;

            return this.StateManager.SetStateAsync<PlayerWords>(state => state.WordCount = this.wordCount);
        }

        private Task SetPlayerData(string[] words)
        {
            this.players.Add(new Player(
                Guid.NewGuid(),
                this.currentPlayerName,
                words.Select(word => new Word(Guid.NewGuid(), word)).ToList()));

            this.StateHasChanged();

            return this.players.Count < this.playerCount ?
                this.StateManager.SetStateAsync<PlayerName>(state => state.Value = string.Empty) :
                Task.WhenAll(this.PlayerSetupDisplay.Hide(), this.CreateTeams());
        }

        private Task CreateTeams()
        {
            this.teams = this.players.Randomize().ToList().CreateTeams(this.teamCount).ToList();

            return this.StateManager.SetStateAsync<TeamName>(state => state.Team = this.teams[0].Map());
        }

        private Task SetTeamName(TeamNameViewModel teamName)
        {
            this.teams[teamName.Id].Name = teamName.Name;

            var nextTeam = this.teams.FirstOrDefault(team => team.Name is null);

            return nextTeam is null ? this.StartGame() :
                this.StateManager.SetStateAsync<TeamName>(state =>
                {
                    state.Team = nextTeam.Map();
                    state.Value = string.Empty;
                });
        }

        private async Task StartGame()
        {
            try
            {
                this.game = new(new(), this.teams, this.roundTypes);
                this.SetEventHandlers();
                this.PersistGame();
                this.Game.Run();
            }
            catch (ArgumentException e)
            {
                await this.Abort(e is InvalidReturnValueException ? "Common.Abort.InvalidReturnValue" : "Common.Abort.PlayerCount");
            }
        }

        private async Task RestoreGame()
        {
            var game = this.PersistedGame.Value;

            if (game is null)
            {
                await this.Abort("Common.Abort.InvalidReturnValue");
                return;
            }

            this.game = new(game);
            this.SetEventHandlers();
            this.Game.Restore();
        }

        private void SetEventHandlers()
        {
            this.Game.GameStarted += game => this.Transition(game, this.OnGameStarted);
            this.Game.GameFinished += game => this.Transition(game, this.OnGameFinished);
            this.Game.RoundStarted += round => this.Transition(round, this.OnRoundStarted);
            this.Game.RoundFinished += round => this.Transition(round, this.OnRoundFinished);
            this.Game.PeriodSetup += period => this.Transition(period, this.OnPeriodSetup);
            this.Game.PeriodStarted += period => this.Transition(period, this.OnPeriodStarted);
            this.Game.PeriodFinished += period => this.Transition(period, this.OnPeriodFinished);
            this.Game.WordSetup += this.OnWordSetup;
        }

        private Task Transition<TState>() where TState : State
        {
            this.transition = this.transition
                .ContinueWith(_ => this.StateManager.SetStateAsync<TState>()).Unwrap();
            return this.transition;
        }

        private void Transition<T>(T input, Func<T, Task> handler)
        {
            this.PersistGame();
            this.transition = this.transition.ContinueWith(_ => handler(input)).Unwrap();
        }

        private void PersistGame() => this.PersistedGame.Value = this.game?.Game;

        private void ClearPersistedGame() => this.PersistedGame.Value = null;

        private async Task OnGameStarted(Game game)
        {
            await Task.Delay(1000);

            await this.StateManager.SetStateAsync<Info>(state =>
            {
                state.ContextClass = ContextCssClass.Dark;
                state.Title = L("Pages.Play.GameStartedTitle");
                state.Message = string.Empty;
                state.Loading = true;
            });
        }

        private Task OnGameFinished(Game game)
        {
            this.ClearPersistedGame();
            return this.StateManager.SetStateAsync<GameFinished>(state =>
            {
                state.Game = game.Map();
                state.Winner = true;
            });
        }

        private Task OnRoundStarted(Round round) =>
            this.StateManager.SetStateAsync<Info>(state =>
            {
                state.ContextClass = ContextCssClass.Dark;
                state.Title = $"{L("Pages.Play.RoundStartedTitle")}: {round.Type}";
                state.Message = string.Empty;
                state.Loading = true;
            });

        private Task OnRoundFinished(Round round) =>
            this.StateManager.SetStateAsync<RoundFinished>(state => state.Round = round.MapSummary());

        private Task OnPeriodSetup(Period period) =>
            this.StateManager.SetStateAsync<PeriodSetupPlay>(
                state => state.Period = period.Map(this.Game.Game.CurrentRound));

        private Task OnPeriodStarted(Period period) =>
            this.StateManager.SetStateAsync<PeriodPlay>(state =>
            {
                state.Word = null;
                state.Expired = false;
                state.ScoreCount = 0;
                state.Period = period.MapRunning(this.Game.Game.CurrentRound);
            });

        private Task OnPeriodFinished(Period period) =>
            this.StateManager.SetStateAsync<PeriodFinished>(state => state.Period = period.Map());

        private void OnWordSetup(Player player, Word word) =>
            this.transition = this.transition
                .ContinueWith(_ => this.StateManager.SetParameters<PeriodPlay>(state => state.Word = word.Map()));

        private void StartPeriod(DateTimeOffset timestamp) => this.Game.StartPeriod(timestamp);

        private void AddScore(ScoreViewModel score)
        {
            this.Game.AddScore(score.Map());
            this.UpdateScore();
            this.Game.NextWord(score.Timestamp);
        }

        private void FinishPeriod(DateTimeOffset timestamp) => this.Game.FinishPeriod(timestamp);

        private void RevokeLastScore()
        {
            this.Game.RevokeLastScore();
            this.UpdateScore();
        }

        private void UpdateScore() =>
            this.transition = this.transition
                .ContinueWith(_ =>
                {
                    this.PersistGame();
                    this.StateManager.SetParameters<PeriodPlay>(state =>
                    state.ScoreCount = this.Game.Game.CurrentRound.CurrentPeriod.Scores.Count);
                });

        private async Task Abort(string messageKey)
        {
            await this.StateManager.SetStateAsync<Info>(state => 
            {
                state.ContextClass = ContextCssClass.Danger;
                state.Title = L("Pages.Play.ErrorTitle");
                state.Message = L(messageKey);
                state.Loading = false;
            });
            this.ClearPersistedGame();
            this.Reload();
        }

        private void Reload() => this.NavigationManager.NavigateTo(this.NavigationManager.Uri, true);
    }
}