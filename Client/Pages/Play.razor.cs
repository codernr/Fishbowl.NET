using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Fishbowl.Net.Client.Components;
using Fishbowl.Net.Client.Components.States;
using Fishbowl.Net.Client.Services;
using Fishbowl.Net.Client.Shared;
using Fishbowl.Net.Shared.Data;
using Fishbowl.Net.Shared.Data.ViewModels;
using Microsoft.AspNetCore.SignalR.Client;
using Microsoft.Extensions.Logging;

namespace Fishbowl.Net.Client.Pages
{
    public partial class Play : IGameClient, IAsyncDisposable
    {
        private StateManager? stateManager;

        private StateManager StateManager => this.stateManager ?? throw new InvalidOperationException();

        private Toast? playerCountDisplay;

        private Toast PlayerCountDisplay => this.playerCountDisplay ?? throw new InvalidOperationException();

        private ToastContainer? toastContainer;

        private HubConnection connection = default!;

        private string L(string key) => this.StringLocalizer[key] ?? key;

        private void Notify(string message, string contextClass) => this.toastContainer?.DisplayToast(message, contextClass);

        protected override async Task OnInitializedAsync()
        {
            this.connection = new HubConnectionBuilder()
                .WithUrl(this.NavigationManager.ToAbsoluteUri("/game"))
                .WithAutomaticReconnect()
                .Build();

            this.connection.Reconnecting += this.Reconnecting;
            this.connection.Reconnected += this.Reconnected;
            this.connection.Closed += this.Closed;

            this.connection.On<GameSetupViewModel>(nameof(this.ReceiveSetupPlayer), this.ReceiveSetupPlayer);
            this.connection.On<PlayerCountViewModel>(nameof(this.ReceivePlayerCount), this.ReceivePlayerCount);
            this.connection.On<Player>(nameof(this.ReceiveWaitForOtherPlayers), this.ReceiveWaitForOtherPlayers);
            this.connection.On<TeamSetupViewModel>(nameof(this.ReceiveSetTeamName), this.ReceiveSetTeamName);
            this.connection.On<TeamSetupViewModel>(nameof(this.ReceiveWaitForTeamSetup), this.ReceiveWaitForTeamSetup);
            this.connection.On<TeamNameViewModel>(nameof(this.ReceiveTeamName), this.ReceiveTeamName);
            this.connection.On<Player, Round>(nameof(this.RestoreGameState), this.RestoreGameState);
            this.connection.On<GameAbortViewModel>(nameof(this.ReceiveGameAborted), this.ReceiveGameAborted);
            this.connection.On(nameof(this.ReceiveGameStarted), this.ReceiveGameStarted);
            this.connection.On<GameSummaryViewModel>(nameof(this.ReceiveGameFinished), this.ReceiveGameFinished);
            this.connection.On<RoundViewModel>(nameof(this.ReceiveRoundStarted), this.ReceiveRoundStarted);
            this.connection.On<RoundSummaryViewModel>(nameof(this.ReceiveRoundFinished), this.ReceiveRoundFinished);
            this.connection.On<PeriodSetupViewModel>(nameof(this.ReceivePeriodSetup), this.ReceivePeriodSetup);
            this.connection.On<PeriodRunningViewModel>(nameof(this.ReceivePeriodStarted), this.ReceivePeriodStarted);
            this.connection.On<PeriodSummaryViewModel>(nameof(this.ReceivePeriodFinished), this.ReceivePeriodFinished);
            this.connection.On<WordViewModel>(nameof(this.ReceiveWordSetup), this.ReceiveWordSetup);
            this.connection.On<ScoreViewModel>(nameof(this.ReceiveScoreAdded), this.ReceiveScoreAdded);
            this.connection.On<ScoreViewModel>(nameof(this.ReceiveLastScoreRevoked), this.ReceiveLastScoreRevoked);

            await this.connection.StartAsync();

            if (this.connection.State == HubConnectionState.Connected)
            {
                await this.Connected();
            }
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

        private async Task Connected()
        {
            if (this.ClientState.Password is not null && await this.connection.GameContextExists(this.ClientState.Password))
            {
                await this.JoinGameContext();
                return;
            }

            await this.StateManager.SetStateAsync<Password>();
        }

        private Task Reconnecting(Exception exception) =>
            this.StateManager.SetStateAsync<Info>(state =>
                {
                    state.ContextClass = ContextCssClass.Danger;
                    state.Title = L("Pages.Play.ErrorTitle");
                    state.Message = L("Pages.Play.Reconnecting");
                });

        private Task Reconnected(string connectionId) => this.JoinGameContext();

        private Task Closed(Exception error) =>
            this.StateManager.SetStateAsync<ConnectionClosed>();

        public async Task ReceiveSetupPlayer(GameSetupViewModel gameSetup)
        {
            this.Logger.LogInformation("ReceiveSetupPlayer: {Setup}", gameSetup);

            if (this.ClientState.IsCreating)
            {
                await this.StateManager.SetStateAsync<Info>(state =>
                {
                    state.ContextClass = ContextCssClass.Success;
                    state.Title = L("Pages.Play.GameCreatedTitle");
                    state.Message = L("Pages.Play.GameCreatedMessage");
                });
                this.ClientState.IsCreating = false;
            }
            
            this.ClientState.PlayerCount = gameSetup.PlayerCount;
            this.ClientState.WordCount = gameSetup.WordCount;
            this.ClientState.TeamCount = gameSetup.TeamCount;
            this.ClientState.RoundTypes = gameSetup.RoundTypes;

            await this.StateManager.SetStateAsync<PlayerName>();
        }

        public Task ReceivePlayerCount(PlayerCountViewModel playerCount)
        {
            this.Logger.LogInformation("ReceiveConnectionCount: {PlayerCount}", playerCount);
            this.ClientState.PlayerCount = playerCount.TotalCount;
            this.ClientState.SetupPlayerCount = playerCount.SetupCount;
            this.StateHasChanged();
            return Task.CompletedTask;
        }

        public async Task ReceiveWaitForOtherPlayers(Player player)
        {
            this.Logger.LogInformation(
                "ReceiveWaitForOtherPlayers: {{PlayerName: {PlayerName}, Words: {Words}}}",
                player.Name, (object)player.Words.Select(word => word.Value));
            
            this.ClientState.Id = player.Id;
            this.ClientState.Name = player.Name;

            await this.StateManager.SetStateAsync<WaitingForPlayers>();
        }

        public Task ReceiveSetTeamName(TeamSetupViewModel teamSetup)
        {
            this.Logger.LogInformation("ReceiveSetTeamName: {TeamSetup}", teamSetup);
            this.ClientState.Teams = teamSetup.Teams;

            return this.StateManager.SetStateAsync<TeamName>(state =>
                state.Team = this.ClientState.Team);
        }

        public Task ReceiveWaitForTeamSetup(TeamSetupViewModel teamSetup)
        {
            this.Logger.LogInformation("ReceiveWaitForTeamSetup: {TeamSetup}", teamSetup);
            this.ClientState.Teams = teamSetup.Teams;

            return this.StateManager.SetStateAsync<WaitingForTeamNames>(state =>
            {
                state.Team = this.ClientState.Team;
                state.Teams = this.ClientState.Teams.Where(team => team.Name is not null).ToList();
            });
        }

        public Task ReceiveTeamName(TeamNameViewModel teamName)
        {
            this.Logger.LogInformation("ReceiveTeamName: {TeamName}", teamName);

            this.ClientState.Teams[teamName.Id] = this.ClientState.Teams[teamName.Id] with { Name = teamName.Name };

            this.StateManager.SetParameters<WaitingForTeamNames>(state =>
            {
                state.Teams = this.ClientState.Teams.Where(team => team.Name is not null).ToList();
            });
            return Task.CompletedTask;
        }

        public Task RestoreGameState(Player player, Round round)
        {
            this.ClientState.Id = player.Id;
            this.ClientState.Name = player.Name;
            return Task.CompletedTask;
        }

        public async Task ReceiveGameAborted(GameAbortViewModel abort)
        {
            this.Logger.LogInformation("ReceiveGameAborted: {Abort}", abort);
            await this.StateManager.SetStateAsync<Info>(state => 
            {
                state.ContextClass = ContextCssClass.Danger;
                state.Title = L("Pages.Play.ErrorTitle");
                state.Message = L(abort.MessageKey);
            });
            await this.connection.StopAsync();
            this.NavigationManager.NavigateTo(this.NavigationManager.Uri, true);
        }

        public async Task ReceiveGameStarted()
        {
            this.Logger.LogInformation("ReceiveGameStarted");

            await Task.Delay(1000);

            await this.StateManager.SetStateAsync<Info>(state =>
            {
                state.ContextClass = ContextCssClass.Dark;
                state.Title = L("Pages.Play.GameStartedTitle");
                state.Message = string.Empty;
            });
        }

        public Task ReceiveGameFinished(GameSummaryViewModel game)
        {
            this.ClientState.Password = null;
            this.Logger.LogInformation("ReceiveGameFinished: {Game}", game);
            return this.StateManager.SetStateAsync<GameFinished>(state =>
            {
                state.Game = game;
                state.Winner = game.Teams[0].Players.Any(player => player.Id == this.ClientState.Id);
            });
        }

        public Task ReceiveRoundStarted(RoundViewModel round)
        {
            this.Logger.LogInformation("ReceiveRoundStarted: {Round}", round);

            return this.StateManager.SetStateAsync<Info>(state =>
            {
                state.Title = $"{L("Pages.Play.RoundStartedTitle")}: {round.Type}";
                state.Message = string.Empty;
            });
        }

        public Task ReceiveRoundFinished(RoundSummaryViewModel round)
        {
            this.Logger.LogInformation("ReceiveRoundFinished: {Round}", round);

            return this.StateManager.SetStateAsync<RoundFinished>(state => state.Round = round);
        }

        public Task ReceivePeriodSetup(PeriodSetupViewModel period)
        {
            this.Logger.LogInformation("ReceivePeriodSetup: {Period}", period);

            return period.Player.Id == this.ClientState.Id ?
                this.StateManager.SetStateAsync<PeriodSetupPlay>(state => state.Period = period) :
                this.StateManager.SetStateAsync<PeriodSetupWatch>(state => state.Period = period);
        }

        public Task ReceivePeriodStarted(PeriodRunningViewModel period)
        {
            this.Logger.LogInformation("ReceivePeriodStarted: {Period}", period);
            this.ClientState.PeriodScores.Clear();

            return period.Player.Id == this.ClientState.Id ?
                this.StateManager.SetStateAsync<PeriodPlay>(state => {
                    state.Word = null;
                    state.ScoreCount = 0;
                    state.Period = period;
                }) :
                this.StateManager.SetStateAsync<PeriodWatch>(state => state.Period = period);
        }

        public Task ReceivePeriodFinished(PeriodSummaryViewModel period)
        {
            this.Logger.LogInformation("ReceivePeriodFinished: {Period}", period);

            return this.StateManager.SetStateAsync<PeriodFinished>(state => state.Period = period);
        }

        public Task ReceiveWordSetup(WordViewModel word)
        {
            this.Logger.LogInformation("ReceiveWordSetup: {Word}", word.Value);

            this.StateManager.SetParameters<PeriodPlay>(state => state.Word = word);
            return Task.CompletedTask;
        }

        public Task ReceiveScoreAdded(ScoreViewModel score)
        {
            this.Logger.LogInformation("ReceiveScoreAdded: {Score}", score);

            this.ClientState.PeriodScores.Add(score);
            this.Notify($"{this.L("Pages.Play.Scored")}: {score.Word.Value}", ContextCssClass.Primary);
            this.StateManager.SetParameters<PeriodPlay>(state => state.ScoreCount = this.ClientState.PeriodScores.Count);
            return Task.CompletedTask;
        }

        public Task ReceiveLastScoreRevoked(ScoreViewModel score)
        {
            this.Logger.LogInformation("ReceiveLastScoreRevoked: {Score}", score);
                
            this.ClientState.PeriodScores.Remove(score);
            this.Notify($"{this.L("Pages.Play.ScoreRevoked")}: {score.Word.Value}", ContextCssClass.Warning);
            this.StateManager.SetParameters<PeriodPlay>(state => state.ScoreCount = this.ClientState.PeriodScores.Count);
            return Task.CompletedTask;
        }

        private Task StartPeriod(DateTimeOffset timestamp) => this.connection.SendAsync("StartPeriod", timestamp);

        private Task NextWord(DateTimeOffset timestamp) => this.connection.SendAsync("NextWord", timestamp);

        private async Task AddScore(ScoreViewModel score)
        {
            await this.connection.SendAsync("AddScore", score);
            await this.NextWord(score.Timestamp);
        }

        private Task RevokeLastScore() => this.connection.SendAsync("RevokeLastScore");

        private Task FinishPeriod(DateTimeOffset timestamp) => this.connection.SendAsync("FinishPeriod", timestamp);

        public ValueTask DisposeAsync() => this.connection.DisposeAsync();

        private Task CreateGame(string password)
        {
            this.ClientState.Password = password;
            return this.AfterPasswordCheck<PlayerCount>(() => {});
        }

        private Task SetPlayerCount(int playerCount) =>
            this.AfterPasswordCheck<TeamCount>(
                () => this.ClientState.PlayerCount = playerCount,
                teamCount => teamCount.MaxTeamCount = playerCount / 2);

        private Task SetTeamCount(int teamCount) =>
            this.AfterPasswordCheck<WordCount>(() => this.ClientState.TeamCount = teamCount);

        private Task SetWordCount(int wordCount) =>
            this.AfterPasswordCheck<RoundTypes>(() => this.ClientState.WordCount = wordCount);

        private async Task SetRoundTypes(string[] roundTypes)
        {
            this.ClientState.RoundTypes = roundTypes;
            this.ClientState.IsCreating = true;
            await this.connection.CreateGameContext(new(
                new(this.ClientState.Password ?? throw new InvalidOperationException(), this.ClientState.Id),
                new(this.ClientState.PlayerCount, this.ClientState.WordCount, this.ClientState.TeamCount, this.ClientState.RoundTypes)));
        }

        private async Task AfterPasswordCheck<TNextState>(
            Action setup, Action<TNextState>? setParameters = null) where TNextState : State
        {
            var passwordExists = this.ClientState.Password is null ?
                false : await this.connection.GameContextExists(this.ClientState.Password);

            if (passwordExists)
            {
                await this.StateManager.SetStateAsync<Info>(state =>
                {
                    state.ContextClass = ContextCssClass.Danger;
                    state.Title = L("Pages.Play.ErrorTitle");
                    state.Message = L("Pages.Play.PasswordIsInUse");
                });
                await this.StateManager.SetStateAsync<Password>();
            }
            else
            {
                setup();
                await this.StateManager.SetStateAsync<TNextState>(setParameters);
            }
        }

        private Task JoinGame(string password)
        {
            this.ClientState.Password = password;

            return this.JoinGameContext();
        }

        private async Task JoinGameContext()
        {
            var success = await this.connection.JoinGameContext(
                new(this.ClientState.Password ?? throw new InvalidOperationException(), this.ClientState.Id));
            
            if (success)
            {
                return;
            }
            
            await this.StateManager.SetStateAsync<Info>(state =>
            {
                state.ContextClass = ContextCssClass.Danger;
                state.Title = L("Pages.Play.ErrorTitle");
                state.Message = L("Pages.Play.JoinGameContextError");
            });
            await this.StateManager.SetStateAsync<Password>();
        }

        private Task SetPlayerName(string name)
        {
            this.ClientState.Name = name;
            return this.StateManager.SetStateAsync<PlayerWords>(state => state.WordCount = this.ClientState.WordCount);
        }

        private Task SubmitPlayerData(string[] words) =>
            this.connection.AddPlayer(new(
                this.ClientState.Id,
                this.ClientState.Name,
                words.Select(word => new Word(Guid.NewGuid(), word))));

        private Task SetTeamName(TeamNameViewModel teamName) =>
            this.connection.SetTeamName(teamName);
    }
}