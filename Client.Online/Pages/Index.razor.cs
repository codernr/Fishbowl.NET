using System;
using System.Linq;
using System.Threading.Tasks;
using Fishbowl.Net.Client.Online.Components.States;
using Fishbowl.Net.Client.Online.Services;
using Fishbowl.Net.Client.Online.Shared;
using Fishbowl.Net.Client.Shared.Common;
using Fishbowl.Net.Client.Shared.Components;
using Fishbowl.Net.Client.Shared.Components.States;
using Fishbowl.Net.Shared;
using Fishbowl.Net.Shared.GameEntities;
using Fishbowl.Net.Shared.ViewModels;
using Microsoft.Extensions.Logging;
using MudBlazor;

namespace Fishbowl.Net.Client.Online.Pages
{
    public partial class Index : IGameClient, IAsyncDisposable
    {
        private StateManager? stateManager;

        private StateManager StateManager => this.stateManager ?? throw new InvalidOperationException();

        private bool isPlayerCountPopoverVisible;

        private bool IsPlayerCountPopoverVisible
        {
            get => this.isPlayerCountPopoverVisible;
            set
            {
                var changed = this.isPlayerCountPopoverVisible != value;
                this.isPlayerCountPopoverVisible = value;
                if (changed) this.StateHasChanged();
            }
        }

        private ClientConnection Connection => this.connection ?? throw new InvalidOperationException();

        private ClientConnection? connection;

        private string L(string key) => this.StringLocalizer[key]?.Value ?? key;

        protected override Task OnInitializedAsync()
        {
            this.connection = new ClientConnection(
                this.NavigationManager.ToAbsoluteUri("/game"),
                this,
                this.LoggerFactory.CreateLogger<ClientConnection>());

            return this.Connection.StartAsync();
        }

        private void OnStateTransition(object newState)
        {
            this.IsPlayerCountPopoverVisible = newState is PlayerWords || newState is WaitingForPlayers;
        }

        public async Task Connected()
        {
            if (this.ClientState.Password is not null && this.ClientState.Username is not null)
            {
                var response = await this.Connection.JoinGameContext(
                    new(this.ClientState.Password, this.ClientState.Username));
                if (response.Status == StatusCode.Ok) return;
            }

            await this.StateManager.SetStateAsync<UsernamePassword>(state =>
            {
                state.OnCreateGame = this.CreateGame;
                state.OnJoinGame = this.JoinGame;
                state.Username = this.ClientState.Username ?? string.Empty;
            });
        }

        public Task Reconnecting(Exception? exception) =>
            this.StateManager.SetStateAsync<Info>(state =>
                {
                    state.ContextClass = ContextCssClass.Error;
                    state.Title = L("Pages.Play.ErrorTitle");
                    state.Message = L("Pages.Play.Reconnecting");
                    state.Loading = true;
                });

        public Task Reconnected(string? connectionId) => this.JoinGameContext();

        public Task Closed(Exception? error) =>
            this.StateManager.SetStateAsync<ConnectionClosed>(state => state.ReloadRequested = this.Reload);

        public async Task ReceiveSetupPlayer(GameSetupViewModel gameSetup)
        {
            if (this.ClientState.IsCreating)
            {
                await this.StateManager.SetStateAsync<Info>(state =>
                {
                    state.ContextClass = ContextCssClass.Success;
                    state.Title = L("Pages.Play.GameCreatedTitle");
                    state.Message = L("Pages.Play.GameCreatedMessage");
                    state.Loading = false;
                }, TimeSpan.FromSeconds(2));
                this.ClientState.IsCreating = false;
            }
            
            this.ClientState.TotalPlayerCount = gameSetup.PlayerCount;
            this.ClientState.WordCount = gameSetup.WordCount;
            this.ClientState.TeamCount = gameSetup.TeamCount;
            this.ClientState.RoundTypes = gameSetup.RoundTypes;

            await this.StateManager.SetStateAsync<PlayerWords>(state =>
            {
                state.OnPlayerWordsSet = this.SubmitPlayerData;
                state.WordCount = this.ClientState.WordCount;
            });
        }

        public Task ReceivePlayerCount(PlayerCountViewModel playerCount)
        {
            this.ClientState.TotalPlayerCount = playerCount.TotalCount;
            this.ClientState.SetupPlayerCount = playerCount.SetupCount;
            this.ClientState.ConnectedPlayerCount = playerCount.ConnectedCount;
            this.StateHasChanged();
            return Task.CompletedTask;
        }

        public async Task ReceiveWaitForOtherPlayers(PlayerViewModel player)
        {
            this.ClientState.Username = player.Username;

            await this.StateManager.SetStateAsync<WaitingForPlayers>();
        }

        public Task ReceiveSetTeamName(TeamSetupViewModel teamSetup)
        {
            this.ClientState.Teams = teamSetup.Teams;

            return this.StateManager.SetStateAsync<TeamName>(state =>
            {
                state.OnTeamNameSet = this.SetTeamName;
                state.Team = this.ClientState.Team;
                state.Title = L("Components.States.TeamName.Title.Online");
            });
        }

        public Task ReceiveWaitForTeamSetup(TeamSetupWatchViewModel teamSetup)
        {
            this.ClientState.Teams = teamSetup.Teams;

            return this.StateManager.SetStateAsync<WaitingForTeamNames>(state =>
            {
                state.SetupPlayer = teamSetup.SetupPlayer;
                state.Team = this.ClientState.Team;
                state.Teams = this.ClientState.Teams.Where(team => team.Name is not null).ToList();
            }, TimeSpan.FromSeconds(2));
        }

        public Task ReceiveTeamName(TeamNameViewModel teamName)
        {
            this.ClientState.Teams[teamName.Id] = this.ClientState.Teams[teamName.Id] with { Name = teamName.Name };

            this.StateManager.SetParameters<WaitingForTeamNames>(state =>
            {
                state.Teams = this.ClientState.Teams.Where(team => team.Name is not null).ToList();
            });
            return Task.CompletedTask;
        }

        public Task ReceiveRestoreState(PlayerViewModel player)
        {
            this.ClientState.Username = player.Username;
            return Task.CompletedTask;
        }

        public async Task ReceiveGameAborted(GameAbortViewModel abort)
        {
            await this.StateManager.SetStateAsync<Info>(state => 
            {
                state.ContextClass = ContextCssClass.Error;
                state.Title = L("Pages.Play.ErrorTitle");
                state.Message = L(abort.MessageKey);
                state.Loading = false;
            }, TimeSpan.FromSeconds(2));
            await this.Connection.StopAsync();
            this.Reload();
        }

        public async Task ReceiveGameStarted()
        {
            await this.StateManager.SetStateAsync<Info>(state =>
            {
                state.ContextClass = ContextCssClass.Default;
                state.Title = L("Pages.Play.GameStartedTitle");
                state.Message = string.Empty;
                state.Loading = true;
            }, TimeSpan.FromSeconds(2));
        }

        public Task ReceiveGameFinished(GameSummaryViewModel game)
        {
            this.ClientState.Password = null;
            return this.StateManager.SetStateAsync<GameFinished>(state =>
            {
                state.ReloadRequested = this.Reload;
                state.Game = game;
            });
        }

        public Task ReceiveRoundStarted(RoundViewModel round) =>
            this.StateManager.SetStateAsync<Info>(state =>
            {
                state.ContextClass = ContextCssClass.Default;
                state.Title = $"{L("Pages.Play.RoundStartedTitle")}: {round.Type}";
                state.Message = string.Empty;
                state.Loading = true;
            }, TimeSpan.FromSeconds(2));

        public Task ReceiveRoundFinished(RoundSummaryViewModel round) =>
            this.StateManager.SetStateAsync<RoundFinished>(
                state => state.Round = round, TimeSpan.FromSeconds(4));

        public Task ReceivePeriodSetup(PeriodSetupViewModel period) =>
            period.Player.Username == this.ClientState.Username ?
                this.StateManager.SetStateAsync<PeriodSetupPlay>(state =>
                {
                    state.OnStarted = this.Connection.StartPeriod;
                    state.Period = period;
                }) :
                this.StateManager.SetStateAsync<PeriodSetupWatch>(state => state.Period = period);

        public Task ReceivePeriodStarted(PeriodRunningViewModel period)
        {
            this.ClientState.PeriodScores.Clear();

            return period.Player.Username == this.ClientState.Username ?
                this.StateManager.SetStateAsync<PeriodPlay>(state => {
                    state.OnScoreAdded = this.AddScore;
                    state.OnPeriodFinished = this.Connection.FinishPeriod;
                    state.OnLastScoreRevoked = this.Connection.RevokeLastScore;
                    state.Word = null;
                    state.Expired = period.StartedAt + period.Length < DateTimeOffset.UtcNow;
                    state.ScoreCount = period.ScoreCount;
                    state.Period = period;
                }) :
                this.StateManager.SetStateAsync<PeriodWatch>(state => state.Period = period);
        }

        public Task ReceivePeriodFinished(PeriodSummaryViewModel period) =>
            this.StateManager.SetStateAsync<PeriodFinished>(state => state.Period = period, TimeSpan.FromSeconds(4));

        public Task ReceiveWordSetup(WordViewModel word)
        {
            this.StateManager.SetParameters<PeriodPlay>(state => state.Word = word);
            return Task.CompletedTask;
        }

        public Task ReceiveScoreAdded(ScoreViewModel score)
        {
            this.ClientState.PeriodScores.Add(score);
            this.Snackbar?.Add($"{this.L("Pages.Play.Scored")}", Severity.Success);
            this.StateManager.SetParameters<PeriodPlay>(state => state.ScoreCount = this.ClientState.PeriodScores.Count);
            return Task.CompletedTask;
        }

        public Task ReceiveLastScoreRevoked(ScoreViewModel score)
        {
            this.ClientState.PeriodScores.Remove(score);
            this.Snackbar?.Add(this.L("Pages.Play.ScoreRevoked"), Severity.Warning);
            this.StateManager.SetParameters<PeriodPlay>(state =>
            {
                state.ScoreCount = this.ClientState.PeriodScores.Count;
                state.Word = score.Word;
            });
            return Task.CompletedTask;
        }

        private async Task AddScore(ScoreViewModel score)
        {
            await this.Connection.AddScore(score);
            await this.Connection.NextWord(score.Timestamp);
        }

        public ValueTask DisposeAsync() => this.Connection.DisposeAsync();

        private async Task CreateGame(GameContextJoinViewModel input)
        {
            this.ClientState.Username = input.Username;
            this.ClientState.Password = input.Password;
            
            await this.ScreenService.RequestWakeLock();
            await this.ScreenService.RequestFullScreen();
            await this.StateManager.SetStateAsync<GameSetup>(state =>
            {
                state.OnGameSetup = this.SetupGame;
                state.Info = L("Components.States.GameSetup.Info");
            });
        }

        private async Task SetupGame(GameSetupViewModel setup)
        {
            this.ClientState.TotalPlayerCount = setup.PlayerCount;
            this.ClientState.TeamCount = setup.TeamCount;
            this.ClientState.WordCount = setup.WordCount;
            this.ClientState.RoundTypes = setup.RoundTypes;

            var response = await this.Connection.CreateGameContext(new(
                new(this.ClientState.Password ?? throw new InvalidOperationException(), this.ClientState.Username),
                new(this.ClientState.TotalPlayerCount, this.ClientState.WordCount, this.ClientState.TeamCount, this.ClientState.RoundTypes)));

            if (response.Status == StatusCode.Ok)
            {
                this.Snackbar.Add(L("Common.GameCreated"), Severity.Success);
                return;
            }

            this.StatusError(response.Status);

            await this.StateManager.SetStateAsync<UsernamePassword>(state =>
            {
                state.OnCreateGame = this.CreateGame;
                state.OnJoinGame = this.JoinGame;
            });
        }

        private async Task AfterPasswordCheck<T>(
            Action setup, Action<T>? setParameters = null) where T : State<T>
        {
            var passwordExists = this.ClientState.Password is null ?
                false : (await this.Connection.GameContextExists(this.ClientState.Password)).Data;

            if (passwordExists)
            {
                this.StatusError(StatusCode.GameContextExists);
                await this.StateManager.SetStateAsync<UsernamePassword>(state =>
                {
                    state.OnCreateGame = this.CreateGame;
                    state.OnJoinGame = this.JoinGame;
                });
                return;
            }
            
            setup();
            await this.StateManager.SetStateAsync<T>(setParameters);
        }

        private async Task JoinGame(GameContextJoinViewModel input)
        {
            this.ClientState.Username = input.Username;
            this.ClientState.Password = input.Password;

            await this.ScreenService.RequestWakeLock();
            await this.ScreenService.RequestFullScreen();
            await this.JoinGameContext();
        }

        private async Task JoinGameContext()
        {
            var response = await this.Connection.JoinGameContext(
                new(this.ClientState.Password ?? throw new InvalidOperationException(), this.ClientState.Username));
            
            if (response.Status == StatusCode.Ok) return;

            this.StatusError(response.Status);
        }

        private void StatusError(StatusCode status) =>
            this.Snackbar.Add(L($"Pages.Play.StatusCode.{status}"), Severity.Error);

        private Task SubmitPlayerData(string[] words) =>
            this.Connection.AddPlayer(new(
                this.ClientState.Username,
                words.Select(word => new Word(Guid.NewGuid(), word))));

        private Task SetTeamName(TeamNameViewModel teamName) =>
            this.Connection.SetTeamName(teamName);

        private void Reload() => this.NavigationManager.NavigateTo(this.NavigationManager.Uri, true);
    }
}