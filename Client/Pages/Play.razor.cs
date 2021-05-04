using System;
using System.Linq;
using System.Threading.Tasks;
using Fishbowl.Net.Client.Components;
using Fishbowl.Net.Client.Components.States;
using Fishbowl.Net.Client.Services;
using Fishbowl.Net.Shared.Data;
using Microsoft.AspNetCore.SignalR.Client;
using Microsoft.Extensions.Logging;

namespace Fishbowl.Net.Client.Pages
{
    public partial class Play : IGameClient, IAsyncDisposable
    {
        private StateManager? stateManager;

        private StateManager StateManager => this.stateManager ?? throw new InvalidOperationException();

        private HubConnection connection = default!;

        private string playerName = string.Empty;

        private Player Player
        {
            get => this.player ?? throw new InvalidOperationException();
            set => this.player = value;
        }

        private Player? player;

        private Round Round
        { 
            get => this.round ?? throw new InvalidOperationException();
            set => this.round = value;
        }

        private Round? round;

        private readonly GameContextSetup gameContextSetup = new();

        private GameSetup gameSetup = new();

        protected override async Task OnInitializedAsync()
        {
            this.gameContextSetup.UserId = this.UserIdProvider.GetUserId();

            this.connection = new HubConnectionBuilder()
                .WithUrl(this.NavigationManager.ToAbsoluteUri("/game"))
                .WithAutomaticReconnect()
                .Build();

            this.connection.Reconnecting += this.Reconnecting;
            this.connection.Reconnected += this.Reconnected;

            this.connection.On<string>("ReceiveGameAborted", this.ReceiveGameAborted);
            this.connection.On<Game>("ReceiveGameStarted", this.ReceiveGameStarted);
            this.connection.On<Game>("ReceiveGameFinished", this.ReceiveGameFinished);
            this.connection.On<Round>("ReceiveRoundStarted", this.ReceiveRoundStarted);
            this.connection.On<Round>("ReceiveRoundFinished", this.ReceiveRoundFinished);
            this.connection.On<Period>("ReceivePeriodSetup", this.ReceivePeriodSetup);
            this.connection.On<Period>("ReceivePeriodStarted", this.ReceivePeriodStarted);
            this.connection.On<Period>("ReceivePeriodFinished", this.ReceivePeriodFinished);
            this.connection.On<Word>("ReceiveWordSetup", this.ReceiveWordSetup);
            this.connection.On<Score>("ReceiveScoreAdded", this.ReceiveScoreAdded);

            await this.connection.StartAsync();

            if (this.connection.State == HubConnectionState.Connected)
            {
                await this.StateManager.SetStateAsync<Password>();
            }
        }

        public Task Reconnecting(Exception exception) =>
            this.StateManager.SetStateAsync<Error>(state =>
                state.Message = "Connection lost, reconnecting...");

        public async Task Reconnected(string connectionId)
        {
            var exists = await this.connection.InvokeAsync<bool>("GameContextExists", this.gameContextSetup.Password);

            if (!exists)
            {
                await this.StateManager.SetStateAsync<Password>();
                return;
            }

            var game = await this.connection.InvokeAsync<Game>("ReconnectGameContext", this.gameContextSetup);

            await this.RestoreState(game);
        }

        public async Task ReceiveGameAborted(string message)
        {
            await this.StateManager.SetStateAsync<Error>(state => state.Message = message);
            await this.connection.StopAsync();
            this.NavigationManager.NavigateTo(this.NavigationManager.Uri, true);
        }

        public Task ReceiveGameStarted(Game game)
        {
            var playerTeam = game.Teams.First(
                team => team.Players.Any(player => player.Id == this.Player.Id));

            this.Logger.LogInformation("My team id: {TeamId}", playerTeam.Id);

            return this.StateManager.SetStateAsync<GameStarted>(
                state => state.Team = playerTeam);
        }

        public Task ReceiveGameFinished(Game game) =>
            this.StateManager.SetStateAsync<GameFinished>(state => state.Game = game);

        public Task ReceiveRoundStarted(Round round)
        {
            this.Logger.LogInformation("Round started: {RoundType}", round.Type);
            this.Round = round;

            return this.StateManager.SetStateAsync<RoundStarted>(state => state.Round = round);
        }

        public Task ReceiveRoundFinished(Round round)
        {
            this.Logger.LogInformation("Round finished: {RoundType}", round.Type);

            return this.StateManager.SetStateAsync<RoundFinished>(state => state.Round = round);
        }

        public Task ReceivePeriodSetup(Period period)
        {
            this.Logger.LogInformation(
                "Period: {{PlayerName: {PlayerName}, Length: {Length}}}",
                period.Player.Name,
                period.Length());

            return period.Player == this.Player ?
                this.StateManager.SetStateAsync<PeriodSetupPlay>(state => state.Round = this.Round) :
                this.StateManager.SetStateAsync<PeriodSetupWatch>(state => {
                    state.Round = this.Round;
                    state.Period = period;
                });
        }

        public Task ReceivePeriodStarted(Period period)
        {
            this.Logger.LogInformation("Period started at: {PeriodStartTime}", period.StartedAt);

            return period.Player == this.Player ?
                this.StateManager.SetStateAsync<PeriodPlay>(state => {
                    state.Round = this.Round;
                    state.Period = period;
                }) :
                this.StateManager.SetStateAsync<PeriodWatch>(state => {
                    state.Round = this.Round;
                    state.Period = period;
                });
        }

        public Task ReceivePeriodFinished(Period period)
        {
            this.Logger.LogInformation(
                "Period finished at: {PeriodFinishedAt}; scores ({PlayerName}): {ScoreCount}",
                period.FinishedAt,
                period.Player.Name,
                period.Scores.Count);

            return this.StateManager.SetStateAsync<PeriodFinished>(state => state.Period = period);
        }

        public Task ReceiveWordSetup(Word word)
        {
            this.Logger.LogInformation("Word setup: {Word}", word.Value);

            this.StateManager.SetParameters<PeriodPlay>(state => state.Word = word);
            return Task.CompletedTask;
        }

        public Task ReceiveScoreAdded(Score score)
        {
            this.Logger.LogInformation(
                "Score received: {{Word: {Word}, Timestamp: {Timestamp}}}",
                score.Word.Value,
                score.Timestamp);
            return Task.CompletedTask;
        }


        private Task StartPeriod(DateTimeOffset timestamp) => this.connection.SendAsync("StartPeriod", timestamp);

        private Task NextWord(DateTimeOffset timestamp) => this.connection.SendAsync("NextWord", timestamp);

        private async Task AddScore(Score score)
        {
            await this.connection.SendAsync("AddScore", score);
            await this.NextWord(score.Timestamp);
        }

        private Task FinishPeriod(DateTimeOffset timestamp) => this.connection.SendAsync("FinishPeriod", timestamp);

        public ValueTask DisposeAsync() => this.connection.DisposeAsync();

        private async Task SetPassword(string password)
        {
            this.gameContextSetup.Password = password;

            var passwordExists = await this.connection.InvokeAsync<bool>("GameContextExists", this.gameContextSetup.Password);

            if (passwordExists)
            {
                await this.StateManager.SetStateAsync<Error>(
                    state => state.Message = "The password is already in use, choose another one.");
                await this.StateManager.SetStateAsync<Password>();
            }
            else
            {
                await this.StateManager.SetStateAsync<WordCount>();
            }
        }

        private async Task SetWordCount(int wordCount)
        {
            this.gameContextSetup.WordCount = wordCount;

            var passwordExists = await this.connection.InvokeAsync<bool>("GameContextExists", this.gameContextSetup.Password);

            if (passwordExists)
            {
                await this.StateManager.SetStateAsync<Error>(
                    state => state.Message = "The password is already in use, choose another one.");
                await this.StateManager.SetStateAsync<Password>();
            }
            else
            {
                await this.connection.InvokeAsync("CreateGameContext", this.gameContextSetup);
                await this.StateManager.SetStateAsync<TeamCount>();
            }
        }

        private async Task JoinGameContext(string password)
        {
            this.gameContextSetup.Password = password;

            var success = await this.connection.InvokeAsync<bool>(
                "JoinGameContext", this.gameContextSetup);
            
            if (success)
            {
                this.gameContextSetup.Password = password;
                this.gameContextSetup.WordCount = await this.connection.InvokeAsync<int>("GetWordCount");
                await this.StateManager.SetStateAsync<PlayerName>();
            }
            else
            {
                await this.StateManager.SetStateAsync<Error>(
                    state => state.Message = 
                    "User is already connected or password is invalid, or game is already running. Try reloading the page.");
                await this.StateManager.SetStateAsync<Password>();
            }
        }

        private Task SetTeamCount(int teamCount)
        {
            this.gameSetup.TeamCount = teamCount;
            return this.StateManager.SetStateAsync<RoundTypes>();
        }

        private async Task SetRoundTypes(string[] roundTypes)
        {
            this.gameSetup.RoundTypes = roundTypes;
            await this.connection.InvokeAsync("SetupGame", this.gameSetup);
            await this.StateManager.SetStateAsync<PlayerName>();
        }

        private Task SetPlayerName(string name)
        {
            this.playerName = name;
            return this.StateManager.SetStateAsync<PlayerWords>();
        }

        private async Task SubmitPlayerData(string[] words)
        {
            this.Player = new Player(
                this.UserIdProvider.GetUserId(),
                this.playerName,
                words.Select(word => new Word(Guid.NewGuid(), word)));
            await this.connection.InvokeAsync("AddPlayer", this.Player);
            await this.StateManager.SetStateAsync<WaitingForPlayers>();
        }

        private async Task RestoreState(Game game)
        {
            this.Player = game.Teams
                .SelectMany(team => team.Players)
                .First(player => player.Id == this.gameContextSetup.UserId);

            this.Round = game.Rounds.Last();

            if (this.Round.Periods.Count == 0) return;

            var period = this.Round.Periods.Last();

            var task = period switch
            {
                { StartedAt: null }     => this.ReceivePeriodSetup(period),
                { FinishedAt: null }    => this.ReceivePeriodStarted(period),
                _                       => this.ReceiveGameFinished(game)
            };

            await task;
        }
    }
}