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

        private PlayerCountViewModel playerCount = new PlayerCountViewModel(0, 0);

        private Round Round
        { 
            get => this.round ?? throw new InvalidOperationException();
            set => this.round = value;
        }

        private Round? round;

        private GameSetup gameSetup = new();

        private readonly List<Score> periodScores = new();

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

            this.connection.On<GameSetup>(nameof(this.ReceiveSetupPlayer), this.ReceiveSetupPlayer);
            this.connection.On<PlayerCountViewModel>(nameof(this.ReceivePlayerCount), this.ReceivePlayerCount);
            this.connection.On<Player>(nameof(this.ReceiveWaitForOtherPlayers), this.ReceiveWaitForOtherPlayers);
            this.connection.On<Player, Round>(nameof(this.RestoreGameState), this.RestoreGameState);
            this.connection.On<string>(nameof(this.ReceiveGameAborted), this.ReceiveGameAborted);
            this.connection.On<IEnumerable<TeamViewModel>>(nameof(this.ReceiveGameStarted), this.ReceiveGameStarted);
            this.connection.On<Game>(nameof(this.ReceiveGameFinished), this.ReceiveGameFinished);
            this.connection.On<RoundViewModel>(nameof(this.ReceiveRoundStarted), this.ReceiveRoundStarted);
            this.connection.On<Round>(nameof(this.ReceiveRoundFinished), this.ReceiveRoundFinished);
            this.connection.On<PeriodSetupViewModel>(nameof(this.ReceivePeriodSetup), this.ReceivePeriodSetup);
            this.connection.On<PeriodRunningViewModel>(nameof(this.ReceivePeriodStarted), this.ReceivePeriodStarted);
            this.connection.On<PeriodSummaryViewModel>(nameof(this.ReceivePeriodFinished), this.ReceivePeriodFinished);
            this.connection.On<WordViewModel>(nameof(this.ReceiveWordSetup), this.ReceiveWordSetup);
            this.connection.On<Score>(nameof(this.ReceiveScoreAdded), this.ReceiveScoreAdded);
            this.connection.On<Score>(nameof(this.ReceiveLastScoreRevoked), this.ReceiveLastScoreRevoked);

            await this.connection.StartAsync();

            if (this.connection.State == HubConnectionState.Connected)
            {
                await this.Connected();
            }
        }

        private Task OnStateTransition(State newState) =>
            (newState is PlayerName || newState is PlayerWords || newState is WaitingForPlayers) ?
            this.PlayerCountDisplay.Show() :
            this.PlayerCountDisplay.Hide();

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

        public async Task ReceiveSetupPlayer(GameSetup gameSetup)
        {
            this.Logger.LogInformation(
                "ReceiveSetupPlayer: {{WordCount: {WordCount}, TeamCount: {TeamCount}, RoundTypes: {RoundTypes}}}",
                gameSetup.WordCount, gameSetup.TeamCount, (object)gameSetup.RoundTypes);

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
            
            this.gameSetup = gameSetup;

            await this.StateManager.SetStateAsync<PlayerName>();
        }

        public Task ReceivePlayerCount(PlayerCountViewModel playerCount)
        {
            this.Logger.LogInformation($"ReceiveConnectionCount: {this.playerCount}");
            this.playerCount = playerCount;
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

        public Task RestoreGameState(Player player, Round round)
        {
            this.ClientState.Id = player.Id;
            this.ClientState.Name = player.Name;
            this.Round = round;
            return Task.CompletedTask;
        }

        public async Task ReceiveGameAborted(string message)
        {
            await this.StateManager.SetStateAsync<Info>(state => 
            {
                state.ContextClass = ContextCssClass.Danger;
                state.Title = L("Pages.Play.ErrorTitle");
                state.Message = message;
            });
            await this.connection.StopAsync();
            this.NavigationManager.NavigateTo(this.NavigationManager.Uri, true);
        }

        public Task ReceiveGameStarted(IEnumerable<TeamViewModel> teams)
        {
            var playerTeam = teams.First(
                team => team.Players.Any(player => player.Id == this.ClientState.Id));

            this.Logger.LogInformation("ReceiveGameStarted: {Team}", playerTeam);

            return this.StateManager.SetStateAsync<GameStarted>(state => state.Team = playerTeam);
        }

        public Task ReceiveGameFinished(Game game)
        {
            this.ClientState.Password = null;
            return this.StateManager.SetStateAsync<GameFinished>(state => state.Game = game);
        }

        public Task ReceiveRoundStarted(RoundViewModel round)
        {
            this.Logger.LogInformation("ReceiveRoundStarted: {Round}", round);

            return this.StateManager.SetStateAsync<RoundStarted>(state => state.Round = round);
        }

        public Task ReceiveRoundFinished(Round round)
        {
            this.Logger.LogInformation("Round finished: {RoundType}", round.Type);

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
            this.periodScores.Clear();

            return period.Player.Id == this.ClientState.Id ?
                this.StateManager.SetStateAsync<PeriodPlay>(state => {
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

        public Task ReceiveScoreAdded(Score score)
        {
            this.Logger.LogInformation(
                "Score received: {{Word: {Word}, Timestamp: {Timestamp}}}",
                score.Word.Value,
                score.Timestamp);

            this.periodScores.Add(score);
            this.Notify($"{this.L("Pages.Play.Scored")}: {score.Word.Value}", ContextCssClass.Primary);
            this.StateManager.SetParameters<PeriodPlay>(state => state.ScoreCount = this.periodScores.Count);
            return Task.CompletedTask;
        }

        public Task ReceiveLastScoreRevoked(Score score)
        {
            this.Logger.LogInformation(
                "Last score revoked: {{Word: {Word}, Timestamp: {Timestamp}}}",
                score.Word.Value,
                score.Timestamp);
                
            this.periodScores.Remove(score);
            this.Notify($"{this.L("Pages.Play.ScoreRevoked")}: {score.Word.Value}", ContextCssClass.Warning);
            this.StateManager.SetParameters<PeriodPlay>(state => state.ScoreCount = this.periodScores.Count);
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
                () => this.gameSetup.PlayerCount = playerCount,
                teamCount => teamCount.MaxTeamCount = playerCount / 2);

        private Task SetTeamCount(int teamCount) =>
            this.AfterPasswordCheck<WordCount>(() => this.gameSetup.TeamCount = teamCount);

        private Task SetWordCount(int wordCount) =>
            this.AfterPasswordCheck<RoundTypes>(() => this.gameSetup.WordCount = wordCount);

        private async Task SetRoundTypes(string[] roundTypes)
        {
            this.gameSetup.RoundTypes = roundTypes;
            this.ClientState.IsCreating = true;
            await this.connection.CreateGameContext(new()
            {
                GameContextJoin = new()
                {
                    Password = this.ClientState.Password ?? throw new InvalidOperationException(),
                    UserId = this.ClientState.Id
                },
                GameSetup = this.gameSetup
            });
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
            var password = this.ClientState.Password ?? throw new InvalidOperationException();
            
            var success = await this.connection.JoinGameContext(new() { Password = password, UserId = this.ClientState.Id });
            
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
            return this.StateManager.SetStateAsync<PlayerWords>(state => state.WordCount = this.gameSetup.WordCount);
        }

        private Task SubmitPlayerData(string[] words) =>
            this.connection.AddPlayer(new(
                this.ClientState.Id,
                this.ClientState.Name,
                words.Select(word => new Word(Guid.NewGuid(), word))));
    }
}