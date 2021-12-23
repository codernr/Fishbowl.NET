using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Fishbowl.Net.Client.Pwa.Components.States;
using Fishbowl.Net.Client.Shared.Components;
using Fishbowl.Net.Client.Shared.Components.States;
using Fishbowl.Net.Shared;
using Fishbowl.Net.Shared.Exceptions;
using Fishbowl.Net.Shared.GameEntities;
using Fishbowl.Net.Shared.ViewModels;
using MudBlazor;

namespace Fishbowl.Net.Client.Pwa.Pages
{
    public partial class Index
    {
        private StateManager? stateManager;

        private StateManager StateManager => this.stateManager ?? throw new InvalidOperationException();

        private GameSetupViewModel? setup;

        private GameSetupViewModel Setup => this.setup ?? throw new InvalidOperationException();

        private List<Player> players = new();

        private List<Team> teams = new();

        private string currentPlayerName = string.Empty;

        private AsyncGame Game => this.game ?? throw new InvalidOperationException();

        private AsyncGame? game;

        private Task transition = Task.CompletedTask;

        private string L(string key) => this.StringLocalizer[key]?.Value ?? key;

        protected override async Task OnAfterRenderAsync(bool firstRender)
        {
            if (!firstRender) return;
            
            await (this.PersistedGame.Value is not null ?
                this.Transition<Restore>(state =>
                {
                    state.OnNewGameRequested = () => this.StateManager.SetStateAsync<GameSetup>(
                        state => state.OnGameSetup = this.SetupGame);
                    state.OnRestoreRequested = this.RestoreGame;
                }) :
                this.Transition<GameSetup>(state => state.OnGameSetup = this.SetupGame));
        }

        private Task SetupGame(GameSetupViewModel setup)
        {
            this.setup = setup;
            
            return this.Transition<PlayerSetup>(state =>
            {
                state.OnPlayerSetup = this.SetupPlayer;
                state.WordCount = this.Setup.WordCount;
                state.Title = string.Format(L("Components.States.PlayerSetup.Title"), 1);
            });
        }

        private Task SetupPlayer(PlayerSetup.Model data)
        {
            this.players.Add(new Player(
                data.PlayerName,
                data.Words.Select(word => new Word(Guid.NewGuid(), word)).ToList()));

            this.StateHasChanged();

            if (this.players.Count < this.Setup.PlayerCount)
            {
                return this.StateManager.SetStateAsync<PlayerSetup>(state =>
                {
                    state.OnPlayerSetup = this.SetupPlayer;
                    state.WordCount = this.Setup.WordCount;
                    state.Title = string.Format(L("Components.States.PlayerSetup.Title"), this.players.Count + 1);
                });
            }

            return this.CreateTeams();
        }

        private Task CreateTeams()
        {
            this.teams = this.players.Randomize().ToList().CreateTeams(this.Setup.TeamCount).ToList();

            return this.StateManager.SetStateAsync<TeamName>(state =>
            {
                state.Team = this.teams[0].Map();
                state.OnTeamNameSet = this.SetTeamName;
                state.Title = string.Format(L("Components.States.TeamName.Title.Pwa"), 1);
            });
        }

        private Task SetTeamName(TeamNameViewModel teamName)
        {
            this.teams[teamName.Id].Name = teamName.Name;

            var nextTeam = this.teams.FirstOrDefault(team => team.Name is null);
            var nextTeamIndex = this.teams.Where(team => team.Name is not null).Count() + 1;

            return nextTeam is null ? this.StartGame() :
                this.StateManager.SetStateAsync<TeamName>(state =>
                {
                    state.Team = nextTeam.Map();
                    state.OnTeamNameSet = this.SetTeamName;
                    state.Title = string.Format(L("Components.States.TeamName.Title.Pwa"), nextTeamIndex);
                });
        }

        private async Task StartGame()
        {
            try
            {
                this.game = new(new(), this.teams, this.Setup.RoundTypes);
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

        private Task Transition<T>(Action<T>? setParameters = null) where T : ComponentState<T>
        {
            this.transition = this.transition
                .ContinueWith(_ => this.StateManager.SetStateAsync<T>(setParameters)).Unwrap();
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
            await this.StateManager.SetStateAsync<Info>(state =>
            {
                state.Title = L("Pages.Play.GameStartedTitle");
                state.Loading = true;
            }, TimeSpan.FromSeconds(2));
        }

        private Task OnGameFinished(Game game)
        {
            this.ClearPersistedGame();
            return this.StateManager.SetStateAsync<GameFinished>(state =>
            {
                state.Game = game.Map();
                state.ReloadRequested = this.Reload;
            });
        }

        private Task OnRoundStarted(Round round) =>
            this.StateManager.SetStateAsync<Info>(state =>
            {
                state.Title = $"{L("Pages.Play.RoundStartedTitle")}: {round.Type}";
                state.Severity = Severity.Info;
                state.Message = L($"Components.States.Common.RoundTypes.{round.Type}.Description");
                state.Loading = true;
            }, TimeSpan.FromSeconds(2));

        private Task OnRoundFinished(Round round) =>
            this.StateManager.SetStateAsync<RoundFinished>(
                state =>state.Round = round.MapSummary(), TimeSpan.FromSeconds(4));

        private Task OnPeriodSetup(Period period) =>
            this.StateManager.SetStateAsync<PeriodSetupPlay>(state =>
            {
                state.Period = period.Map(this.Game.Game.CurrentRound);
                state.OnStarted = this.StartPeriod;
            });

        private Task OnPeriodStarted(Period period) =>
            this.StateManager.SetStateAsync<PeriodPlay>(state =>
            {
                state.Word = null;
                state.Expired = period.StartedAt!.Value + period.Length < DateTimeOffset.UtcNow;
                state.ScoreCount = period.Scores.Count;
                state.Period = period.MapRunning(this.Game.Game.CurrentRound);
                state.OnScoreAdded = this.AddScore;
                state.OnPeriodFinished = this.FinishPeriod;
                state.OnLastScoreRevoked = this.RevokeLastScore;
            });

        private Task OnPeriodFinished(Period period) =>
            this.StateManager.SetStateAsync<PeriodFinished>(
                state => state.Period = period.Map(), TimeSpan.FromSeconds(4));

        private void OnWordSetup(Player player, Word word) =>
            this.transition = this.transition
                .ContinueWith(_ => this.StateManager.SetParameters<PeriodPlay>(state => state.Word = word.Map()));

        private Task StartPeriod(DateTimeOffset timestamp)
        {
            this.Game.StartPeriod(timestamp);
            return Task.CompletedTask;
        }

        private Task AddScore(ScoreViewModel score)
        {
            this.Game.AddScore(score.Map());
            this.UpdateScore();
            this.Game.NextWord(score.Timestamp);
            return Task.CompletedTask;
        }

        private Task FinishPeriod(DateTimeOffset timestamp)
        {
            this.Game.FinishPeriod(timestamp);
            return Task.CompletedTask;
        }

        private Task RevokeLastScore()
        {
            var score = this.Game.Game.CurrentRound.CurrentPeriod.Scores.Last();
            this.Game.RevokeLastScore();
            this.UpdateScore(score.Word);
            return Task.CompletedTask;
        }

        private void UpdateScore(Word? previousWord = null) =>
            this.transition = this.transition
                .ContinueWith(_ =>
                {
                    this.PersistGame();
                    this.StateManager.SetParameters<PeriodPlay>(state =>
                    {
                        state.ScoreCount = this.Game.Game.CurrentRound.CurrentPeriod.Scores.Count;
                        if (previousWord is not null) state.Word = previousWord.Map();
                    });
                });

        private async Task Abort(string messageKey)
        {
            await this.StateManager.SetStateAsync<Info>(state => 
            {
                state.Title = L("Pages.Play.ErrorTitle");
                state.Severity = Severity.Error;
                state.Message = L(messageKey);
                state.Loading = false;
            }, TimeSpan.FromSeconds(2));
            this.ClearPersistedGame();
            this.Reload();
        }

        private void Reload() => this.NavigationManager.NavigateTo(this.NavigationManager.Uri, true);
    }
}