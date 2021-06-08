using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
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

        private int playerCount = 4;

        private int teamCount = 2;

        private int wordCount = 1;

        private string[] roundTypes = new string[0];

        private List<Player> players = new();

        private List<Team> teams = new();

        private string currentPlayerName = string.Empty;

        private AsyncGame Game => this.game ?? throw new InvalidOperationException();

        private AsyncGame? game;

        private string L(string key) => this.StringLocalizer[key] ?? key;

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

            return this.StateManager.SetStateAsync<PlayerName>();
        }

        private Task SetPlayerName(string name)
        {
            this.currentPlayerName = name;

            return this.StateManager.SetStateAsync<PlayerWords>();
        }

        private Task SetPlayerData(string[] words)
        {
            this.players.Add(
                new Player(Guid.NewGuid(), this.currentPlayerName, words.Select(word => new Word(Guid.NewGuid(), word))));

            return this.players.Count < this.playerCount ?
                this.StateManager.SetStateAsync<PlayerName>() :
                this.CreateTeams();
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
                this.StateManager.SetStateAsync<TeamName>(state => state.Team = nextTeam.Map());
        }

        private async Task StartGame()
        {
            try
            {
                this.game = new(new(), this.teams, this.roundTypes);
                this.SetEventHandlers();
                this.Game.Run();
            }
            catch (ArgumentException e)
            {
                await this.Abort(e is InvalidReturnValueException ? "Common.Abort.InvalidReturnValue" : "Common.Abort.PlayerCount");
            }
        }

        private void SetEventHandlers()
        {
            this.Game.GameStarted += this.OnGameStarted;
            this.Game.GameFinished += this.OnGameFinished;
            this.Game.RoundStarted += this.OnRoundStarted;
            this.Game.RoundFinished += this.OnRoundFinished;
            this.Game.PeriodSetup += this.OnPeriodSetup;
            this.Game.PeriodStarted += this.OnPeriodStarted;
            this.Game.PeriodFinished += this.OnPeriodFinished;
            this.Game.WordSetup += this.OnWordSetup;
        }

        public async void OnGameStarted(Game game)
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

        public async void OnGameFinished(Game game) =>
            await this.StateManager.SetStateAsync<GameFinished>(state =>
            {
                state.Game = game.Map();
                state.Winner = true;
            });

        public async void OnRoundStarted(Round round) =>
            await this.StateManager.SetStateAsync<Info>(state =>
            {
                state.ContextClass = ContextCssClass.Dark;
                state.Title = $"{L("Pages.Play.RoundStartedTitle")}: {round.Type}";
                state.Message = string.Empty;
                state.Loading = true;
            });

        public async void OnRoundFinished(Round round) =>
            await this.StateManager.SetStateAsync<RoundFinished>(state => state.Round = round.MapSummary());

        public async void OnPeriodSetup(Period period) =>
            await this.StateManager.SetStateAsync<PeriodSetupPlay>(
                state => state.Period = period.MapRunning(this.Game.Game.CurrentRound));

        public async void OnPeriodStarted(Period period) =>
            await this.StateManager.SetStateAsync<PeriodPlay>(state =>
            {
                state.Word = null;
                state.ScoreCount = 0;
                state.Period = period.MapRunning(this.Game.Game.CurrentRound);
            });

        public async void OnPeriodFinished(Period period) =>
            await this.StateManager.SetStateAsync<PeriodFinished>(state => state.Period = period.Map());

        public void OnWordSetup(Player player, Word word) =>
            this.StateManager.SetParameters<PeriodPlay>(state => state.Word = word.Map());

        private void StartPeriod(DateTimeOffset timestamp) => this.Game.StartPeriod(timestamp);

        private void AddScore(ScoreViewModel score) => this.Game.AddScore(score.Map());

        private void FinishPeriod(DateTimeOffset timestamp) => this.Game.FinishPeriod(timestamp);

        private void RevokeLastScore() => this.Game.RevokeLastScore();

        private async Task Abort(string messageKey)
        {
            await this.StateManager.SetStateAsync<Info>(state => 
            {
                state.ContextClass = ContextCssClass.Danger;
                state.Title = L("Pages.Play.ErrorTitle");
                state.Message = L(messageKey);
                state.Loading = false;
            });
            this.Reload();
        }

        private void Reload() => this.NavigationManager.NavigateTo(this.NavigationManager.Uri, true);
    }
}