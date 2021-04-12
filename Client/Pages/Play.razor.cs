using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Fishbowl.Net.Client.Components;
using Fishbowl.Net.Client.Components.States;
using Fishbowl.Net.Client.Services;
using Fishbowl.Net.Shared.Data;
using Microsoft.AspNetCore.SignalR.Client;

namespace Fishbowl.Net.Client.Pages
{
    public partial class Play : IGameClient, IAsyncDisposable
    {
        private StateManager? stateManager;

        private StateManager StateManager => this.stateManager ?? throw new InvalidOperationException();

        private HubConnection connection = default!;

        private string playerName = string.Empty;

        private Player? player;

        private Player Player => this.player ?? throw new InvalidOperationException();

        private Team? team;

        private Team Team => this.team ?? throw new InvalidOperationException();

        private Round? round;

        private Round Round
        { 
            get => this.round ?? throw new InvalidOperationException();
            set => this.round = value;
        }

        private Round? previousRound;
        
        private Round PreviousRound
        { 
            get => this.previousRound ?? throw new InvalidOperationException();
            set => this.previousRound = value;
        }

        private Period? period;

        private Period Period
        {
            get => this.period ?? throw new InvalidOperationException();
            set => this.period = value;
        }

        private Period? previousPeriod;

        private Period PreviousPeriod
        {
            get => this.previousPeriod ?? throw new InvalidOperationException();
            set => this.previousPeriod = value;
        }

        private Word? Word { get; set; }

        private Game? game;

        private Game Game
        {
            get => this.game ?? throw new InvalidOperationException();
            set => this.game = value;
        }

        private GameContextSetup gameContextSetup = new();

        private GameSetup gameSetup = new();

        protected override async Task OnInitializedAsync()
        {
            this.connection = new HubConnectionBuilder()
                .WithUrl(this.NavigationManager.ToAbsoluteUri("/game"))
                .Build();

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

        public Task ReceiveGameStarted(Game game)
        {
            this.Game = game;
            this.team = this.Game.Teams.First(
                team => team.Players.Any(player => player.Id == this.Player.Id));

            Console.WriteLine($"My team id: {this.Team.Id}");

            return this.StateManager.SetStateAsync<GameStarted>(
                state => state.Team = this.team);
        }

        public Task ReceiveGameFinished(Game game)
        {
            this.Game = game;

            foreach (var teamScore in this.Game.GetTeamScores())
            {
                Console.WriteLine($"Team {teamScore.Key} scores: {teamScore.Value}");
            }

            return this.StateManager.SetStateAsync<GameFinished>(state => state.Game = game);
        }

        public Task ReceiveRoundStarted(Round round)
        {
            Console.WriteLine($"Round started: {round.Type}");
            this.Round = round;

            return this.StateManager.SetStateAsync<RoundStarted>(state => state.Round = round);
        }

        public Task ReceiveRoundFinished(Round round)
        {
            Console.WriteLine($"Round finished: {round.Type}");
            this.Round = round;

            return this.StateManager.SetStateAsync<RoundFinished>(state => state.Round = round);
        }

        public Task ReceivePeriodSetup(Period period)
        {
            Console.WriteLine($"Period player: {period.Player.Name}");
            Console.WriteLine($"Period length: {period.Length()}");
            this.Period = period;

            if (period.Player == this.Player)
            {
                Console.WriteLine("Local player period");
                return this.StateManager.SetStateAsync<PeriodSetupPlay>(state => {
                    state.Round = this.Round;
                });
            }
            else
            {
                Console.WriteLine("Remote player period");
                return this.StateManager.SetStateAsync<PeriodSetupWatch>(state => {
                    state.Round = this.Round;
                    state.Period = period;
                });
            }
        }

        public Task ReceivePeriodStarted(Period period)
        {
            Console.WriteLine("Period started at: " + period);
            this.Period = period;

            if (period.Player == this.Player)
            {
                return this.StateManager.SetStateAsync<PeriodPlay>(state => {
                    state.Round = this.Round;
                    state.Period = this.Period;
                });
            }
            else
            {
                return this.StateManager.SetStateAsync<PeriodWatch>(state => {
                    state.Round = this.Round;
                    state.Period = this.Period;
                });
            }
        }

        public Task ReceivePeriodFinished(Period period)
        {
            Console.WriteLine($"Period finished at: {period.FinishedAt}; scores ({period.Player.Name}): {period.Scores.Count}");
            this.PreviousPeriod = period;

            return this.StateManager.SetStateAsync<PeriodFinished>(state => state.Period = period);
        }

        public Task ReceiveWordSetup(Word word)
        {
            Console.WriteLine($"Word: {word.Value}");
            this.Word = word;

            this.StateManager.SetParameters<PeriodPlay>(state => state.Word = word);
            return Task.CompletedTask;
        }

        public Task ReceiveScoreAdded(Score score)
        {
            Console.WriteLine($"Score received: [{score.Word.Value}, {score.Timestamp}]");
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

        private Task SetPassword(string password)
        {
            this.gameContextSetup.Password = password;
            return this.StateManager.SetStateAsync<WordCount>();
        }

        private async Task SetWordCount(int wordCount)
        {
            this.gameContextSetup.WordCount = wordCount;

            await this.connection.InvokeAsync("CreateGameContext", this.gameContextSetup);
            await this.StateManager.SetStateAsync<TeamCount>();
        }

        private async Task JoinGameContext(string password)
        {
            await this.connection.InvokeAsync("JoinGameContext", password);
            this.gameContextSetup.Password = password;
            this.gameContextSetup.WordCount = await this.connection.InvokeAsync<int>("GetWordCount");
            await this.StateManager.SetStateAsync<PlayerName>();
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
            this.player = new Player(
                Guid.NewGuid(),
                this.playerName,
                words.Select(word => new Word(Guid.NewGuid(), word)));
            await this.connection.InvokeAsync("AddPlayer", player);
            await this.StateManager.SetStateAsync<WaitingForPlayers>();
        }
    }
}