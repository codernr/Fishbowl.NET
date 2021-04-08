using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Fishbowl.Net.Client.Components;
using Fishbowl.Net.Client.Services;
using Fishbowl.Net.Client.Shared;
using Fishbowl.Net.Shared.Data;
using Microsoft.AspNetCore.SignalR.Client;

namespace Fishbowl.Net.Client.Pages
{
    public partial class Play : IGameClient, IAsyncDisposable
    {
        private StateManager? stateManager;

        private StateManager StateManager => this.stateManager ?? throw new InvalidOperationException();

        private HubConnection connection = default!;

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

        protected override async Task OnInitializedAsync()
        {
            this.connection = new HubConnectionBuilder()
                .WithUrl(this.NavigationManager.ToAbsoluteUri("/game"))
                .Build();

            this.connection.On("DefineTeamCount", this.DefineTeamCount);
            this.connection.On("DefineRoundTypes", this.DefineRoundTypes);
            this.connection.On("DefinePlayer", this.DefinePlayer);
            this.connection.On("ReceiveWaitForPlayers", this.ReceiveWaitForPlayers);
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
        }

        public async Task DefineTeamCount()
        {
            await this.StateManager.SetState(GameState.WaitingForTeamCount);
        }

        public Task DefineRoundTypes()
        {
            Console.WriteLine("DefineRoundTypes");
            return this.StateManager.SetState(GameState.WaitingForRoundTypes);
        }

        public Task DefinePlayer()
        {
            Console.WriteLine("DefinePlayer");
            return this.StateManager.SetState(GameState.WaitingForPlayer);
        }

        public Task ReceiveWaitForPlayers()
        {
            Console.WriteLine("ReceiveWaitForPlayers");
            return this.StateManager.SetState(GameState.WaitingForPlayers);
        }

        public Task ReceiveGameStarted(Game game)
        {
            this.Game = game;
            this.team = this.Game.Teams.First(
                team => team.Players.Any(player => player.Id == this.Player.Id));

            Console.WriteLine($"My team id: {this.Team.Id}");

            return this.StateManager.SetState(GameState.GameStarted);
        }

        public Task ReceiveGameFinished(Game game)
        {
            this.Game = game;

            foreach (var teamScore in this.Game.GetTeamScores())
            {
                Console.WriteLine($"Team {teamScore.Key} scores: {teamScore.Value}");
            }

            return this.StateManager.SetState(GameState.GameFinished);
        }

        public Task ReceiveRoundStarted(Round round)
        {
            Console.WriteLine($"Round started: {round.Type}");
            this.Round = round;

            return this.StateManager.SetState(GameState.RoundStarted);
        }

        public Task ReceiveRoundFinished(Round round)
        {
            Console.WriteLine($"Round finished: {round.Type}");
            this.Round = round;

            return this.StateManager.SetState(GameState.RoundFinished);
        }

        public Task ReceivePeriodSetup(Period period)
        {
            Console.WriteLine($"Period player: {period.Player.Name}");
            Console.WriteLine($"Period length: {period.Length()}");
            this.Period = period;

            if (period.Player == this.Player)
            {
                Console.WriteLine("Local player period");
                return this.StateManager.SetState(GameState.PeriodSetupPlay);
            }
            else
            {
                Console.WriteLine("Remote player period");
                return this.StateManager.SetState(GameState.PeriodSetupWatch);
            }
        }

        public Task ReceivePeriodStarted(Period period)
        {
            Console.WriteLine("Period started at: " + period);
            this.Period = period;
            return this.StateManager.SetState(
                period.Player == this.Player ? GameState.PeriodPlay : GameState.PeriodWatch);
        }

        public Task ReceivePeriodFinished(Period period)
        {
            Console.WriteLine($"Period finished at: {period.FinishedAt}; scores ({period.Player.Name}): {period.Scores.Count}");
            this.PreviousPeriod = period;
            return this.StateManager.SetState(GameState.PeriodFinished);
        }

        public Task ReceiveWordSetup(Word word)
        {
            Console.WriteLine($"Word: {word.Value}");
            this.Word = word;
            this.StateHasChanged();
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

        private Task SubmitTeamCount(int teamCount) => this.connection.SendAsync("SetTeamCount", teamCount);

        private Task SubmitRoundTypes(IEnumerable<string> roundTypes) =>
            this.connection.SendAsync("SetRoundTypes", roundTypes);

        private Task SubmitPlayerData(Player player)
        {
            this.player = player;
            return this.connection.SendAsync("AddPlayer", player);
        }

        private async Task SetPlayerAsync()
        {
            var random = new Random();

            this.player = new Player(
                Guid.NewGuid(),
                $"Player{random.Next()}",
                new[]
                {
                new Word(Guid.NewGuid(), $"Word{random.Next()}"),
                new Word(Guid.NewGuid(), $"Word{random.Next()}")
                });

            await this.connection.InvokeAsync("AddPlayer", this.Player);

            Console.WriteLine($"{this.player.Name} set");
        }
    }
}