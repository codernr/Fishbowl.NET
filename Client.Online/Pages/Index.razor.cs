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

namespace Fishbowl.Net.Client.Online.Pages
{
    public partial class Index : IGameClient, IAsyncDisposable
    {
        private StateManager? stateManager;

        private StateManager StateManager => this.stateManager ?? throw new InvalidOperationException();

        private Toast? playerCountDisplay;

        private Toast PlayerCountDisplay => this.playerCountDisplay ?? throw new InvalidOperationException();

        private ToastContainer? toastContainer;

        private ClientConnection Connection => this.connection ?? throw new InvalidOperationException();

        private ClientConnection? connection;

        private string L(string key) => this.StringLocalizer[key]?.Value ?? key;

        private void Notify(string message, string contextClass) => this.toastContainer?.DisplayToast(message, contextClass);

        protected override Task OnInitializedAsync()
        {
            this.connection = new ClientConnection(
                this.NavigationManager.ToAbsoluteUri("/game"),
                this,
                this.LoggerFactory.CreateLogger<ClientConnection>());

            return this.Connection.StartAsync();
        }

        private async void OnStateTransition(State newState)
        {
            if (newState is PlayerName || newState is PlayerWords || newState is WaitingForPlayers)
            {
                await this.PlayerCountDisplay.Show();
                return;
            }
            await this.PlayerCountDisplay.Hide();
        }

        public async Task Connected()
        {
            if (this.ClientState.Password is not null)
            {
                var response = await this.Connection.JoinGameContext(new(this.ClientState.Password, this.ClientState.Id));
                if (response.Status == StatusCode.Ok) return;
            }

            await this.StateManager.SetStateAsync<Password>();
        }

        public Task Reconnecting(Exception exception) =>
            this.StateManager.SetStateAsync<Info>(state =>
                {
                    state.ContextClass = ContextCssClass.Danger;
                    state.Title = L("Pages.Play.ErrorTitle");
                    state.Message = L("Pages.Play.Reconnecting");
                    state.Loading = true;
                });

        public Task Reconnected(string connectionId) => this.JoinGameContext();

        public Task Closed(Exception error) =>
            this.StateManager.SetStateAsync<ConnectionClosed>();

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
                });
                this.ClientState.IsCreating = false;
            }
            
            this.ClientState.TotalPlayerCount = gameSetup.PlayerCount;
            this.ClientState.WordCount = gameSetup.WordCount;
            this.ClientState.TeamCount = gameSetup.TeamCount;
            this.ClientState.RoundTypes = gameSetup.RoundTypes;

            await this.StateManager.SetStateAsync<PlayerName>(state => state.Value = this.ClientState.Name);
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
            this.ClientState.Id = player.Id;
            this.ClientState.Name = player.Name;

            await this.StateManager.SetStateAsync<WaitingForPlayers>();
        }

        public Task ReceiveSetTeamName(TeamSetupViewModel teamSetup)
        {
            this.ClientState.Teams = teamSetup.Teams;

            return this.StateManager.SetStateAsync<TeamName>(state =>
                state.Team = this.ClientState.Team);
        }

        public Task ReceiveWaitForTeamSetup(TeamSetupViewModel teamSetup)
        {
            this.ClientState.Teams = teamSetup.Teams;

            return this.StateManager.SetStateAsync<WaitingForTeamNames>(state =>
            {
                state.Team = this.ClientState.Team;
                state.Teams = this.ClientState.Teams.Where(team => team.Name is not null).ToList();
            });
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
            this.ClientState.Id = player.Id;
            this.ClientState.Name = player.Name;
            return Task.CompletedTask;
        }

        public async Task ReceiveGameAborted(GameAbortViewModel abort)
        {
            await this.StateManager.SetStateAsync<Info>(state => 
            {
                state.ContextClass = ContextCssClass.Danger;
                state.Title = L("Pages.Play.ErrorTitle");
                state.Message = L(abort.MessageKey);
                state.Loading = false;
            });
            await this.Connection.StopAsync();
            this.Reload();
        }

        public async Task ReceiveGameStarted()
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

        public Task ReceiveGameFinished(GameSummaryViewModel game)
        {
            this.ClientState.Password = null;
            return this.StateManager.SetStateAsync<GameFinished>(state =>
            {
                state.Game = game;
                state.Winner = game.Teams[0].Players.Any(player => player.Id == this.ClientState.Id);
            });
        }

        public Task ReceiveRoundStarted(RoundViewModel round) =>
            this.StateManager.SetStateAsync<Info>(state =>
            {
                state.ContextClass = ContextCssClass.Dark;
                state.Title = $"{L("Pages.Play.RoundStartedTitle")}: {round.Type}";
                state.Message = string.Empty;
                state.Loading = true;
            });

        public Task ReceiveRoundFinished(RoundSummaryViewModel round) =>
            this.StateManager.SetStateAsync<RoundFinished>(state => state.Round = round);

        public Task ReceivePeriodSetup(PeriodSetupViewModel period) =>
            period.Player.Id == this.ClientState.Id ?
                this.StateManager.SetStateAsync<PeriodSetupPlay>(state => state.Period = period) :
                this.StateManager.SetStateAsync<PeriodSetupWatch>(state => state.Period = period);

        public Task ReceivePeriodStarted(PeriodRunningViewModel period)
        {
            this.ClientState.PeriodScores.Clear();

            return period.Player.Id == this.ClientState.Id ?
                this.StateManager.SetStateAsync<PeriodPlay>(state => {
                    state.Word = null;
                    state.Expired = period.StartedAt + period.Length < DateTimeOffset.UtcNow;
                    state.ScoreCount = period.ScoreCount;
                    state.Period = period;
                }) :
                this.StateManager.SetStateAsync<PeriodWatch>(state => state.Period = period);
        }

        public Task ReceivePeriodFinished(PeriodSummaryViewModel period) =>
            this.StateManager.SetStateAsync<PeriodFinished>(state => state.Period = period);

        public Task ReceiveWordSetup(WordViewModel word)
        {
            this.StateManager.SetParameters<PeriodPlay>(state => state.Word = word);
            return Task.CompletedTask;
        }

        public Task ReceiveScoreAdded(ScoreViewModel score)
        {
            this.ClientState.PeriodScores.Add(score);
            this.Notify($"{this.L("Pages.Play.Scored")}: {score.Word.Value}", ContextCssClass.Primary);
            this.StateManager.SetParameters<PeriodPlay>(state => state.ScoreCount = this.ClientState.PeriodScores.Count);
            return Task.CompletedTask;
        }

        public Task ReceiveLastScoreRevoked(ScoreViewModel score)
        {
            this.ClientState.PeriodScores.Remove(score);
            this.Notify($"{this.L("Pages.Play.ScoreRevoked")}: {score.Word.Value}", ContextCssClass.Warning);
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

        private async Task CreateGame(string password)
        {
            this.ClientState.Password = password;
            
            await this.ScreenService.RequestWakeLock();
            await this.ScreenService.RequestFullScreen();
            await this.AfterPasswordCheck<PlayerCount>(() => {});
        }

        private Task SetPlayerCount(int playerCount) =>
            this.AfterPasswordCheck<TeamCount>(
                () => this.ClientState.TotalPlayerCount = playerCount,
                teamCount => teamCount.MaxTeamCount = playerCount / 2);

        private Task SetTeamCount(int teamCount) =>
            this.AfterPasswordCheck<WordCount>(() => this.ClientState.TeamCount = teamCount);

        private Task SetWordCount(int wordCount) =>
            this.AfterPasswordCheck<RoundTypes>(() => this.ClientState.WordCount = wordCount);

        private async Task SetRoundTypes(string[] roundTypes)
        {
            this.ClientState.RoundTypes = roundTypes;
            this.ClientState.IsCreating = true;
            var response = await this.Connection.CreateGameContext(new(
                new(this.ClientState.Password ?? throw new InvalidOperationException(), this.ClientState.Id),
                new(this.ClientState.TotalPlayerCount, this.ClientState.WordCount, this.ClientState.TeamCount, this.ClientState.RoundTypes)));

            if (response.Status == StatusCode.Ok) return;
            await this.StatusError(response.Status);
        }

        private async Task AfterPasswordCheck<TNextState>(
            Action setup, Action<TNextState>? setParameters = null) where TNextState : State
        {
            var passwordExists = this.ClientState.Password is null ?
                false : (await this.Connection.GameContextExists(this.ClientState.Password)).Data;

            if (passwordExists)
            {
                await this.StatusError(StatusCode.GameContextExists);
                return;
            }
            
            setup();
            await this.StateManager.SetStateAsync<TNextState>(setParameters);
        }

        private async Task JoinGame(string password)
        {
            this.ClientState.Password = password;

            await this.ScreenService.RequestWakeLock();
            await this.ScreenService.RequestFullScreen();
            await this.JoinGameContext();
        }

        private async Task JoinGameContext()
        {
            var response = await this.Connection.JoinGameContext(
                new(this.ClientState.Password ?? throw new InvalidOperationException(), this.ClientState.Id));
            
            if (response.Status == StatusCode.Ok) return;

            await this.StatusError(response.Status);
        }

        private async Task StatusError(StatusCode status)
        {
            await this.StateManager.SetStateAsync<Info>(state =>
            {
                state.ContextClass = ContextCssClass.Danger;
                state.Title = L("Pages.Play.ErrorTitle");
                state.Message = L($"Pages.Play.StatusCode.{status}");
                state.Loading = false;
            });
            await this.StateManager.SetStateAsync<Password>();
        }

        private Task SetPlayerName(string name)
        {
            this.ClientState.Name = name;
            return this.StateManager.SetStateAsync<PlayerWords>(state => state.WordCount = this.ClientState.WordCount);
        }

        private Task SubmitPlayerData(string[] words) =>
            this.Connection.AddPlayer(new(
                this.ClientState.Id,
                this.ClientState.Name,
                words.Select(word => new Word(Guid.NewGuid(), word))));

        private Task SetTeamName(TeamNameViewModel teamName) =>
            this.Connection.SetTeamName(teamName);

        private void Reload() => this.NavigationManager.NavigateTo(this.NavigationManager.Uri, true);
    }
}