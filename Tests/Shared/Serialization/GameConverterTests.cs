using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using Fishbowl.Net.Shared;
using Fishbowl.Net.Shared.GameEntities;
using Fishbowl.Net.Shared.Serialization;
using Xunit;

namespace Fishbowl.Net.Tests.Shared.Serialization
{
    public class GameConverterTests
    {
        [Fact]
        public void ShouldSerializeCorrectly()
        {
            var options = new JsonSerializerOptions(JsonSerializerDefaults.Web);
            options.Converters.Add(new GameConverter());
            options.Converters.Add(new TeamConverter());
            options.Converters.Add(new RandomEnumeratorConverter());

            var teams = CreatePlayers(5, 2).CreateTeams(2);
            
            var rounds = new[] { "GameType1", "GameType2" };

            var game = new Game(Guid.NewGuid(), new(), teams, rounds, false);

            int roundCount = 0;
            int periodCount = 0;
            int totalPeriodCount = 0;
            int wordCount = 0;
            int totalWordCount = 0;

            var guessedWords = new[]
            {
                new[]
                {
                    new[] { "Player0Word0", "Player0Word1", "Player1Word0", "Player1Word1", "Player2Word0", "Player2Word1" },
                    new[] { "Player3Word0", "Player3Word1", "Player4Word0", "Player4Word1" }
                },
                new[]
                {
                    new[] { "Player0Word0", "Player0Word1" },
                    new[] { "Player1Word0", "Player1Word1", "Player2Word0", "Player2Word1", "Player3Word0", "Player3Word1" },
                    new[] { "Player4Word0", "Player4Word1" }
                }
            };

            var playerList = new[] { "Player0", "Player1", "Player1", "Player2", "Player3" };

            foreach (var round in game.RoundLoop())
            {
                Assert.Equal(rounds[roundCount], Serialize(game, options).CurrentRound.Type);

                foreach (var period in game.PeriodLoop())
                {
                    Assert.Equal(playerList[totalPeriodCount], Serialize(game, options).CurrentRound.CurrentPeriod.Player.Name);

                    var now = new DateTimeOffset(2021, 3, 31, 10, 0, 0, TimeSpan.Zero);

                    game.StartPeriod(now);

                    do
                    {
                        Assert.Equal(
                            guessedWords[roundCount][periodCount][wordCount],
                            Serialize(game, options).CurrentWord.Value);
                        
                        now += TimeSpan.FromSeconds(10);
                        game.AddScore(new Score(game.CurrentWord, now));
                        wordCount++;
                        totalWordCount++;
                    }
                    while (game.NextWord(now));

                    wordCount = 0;
                    periodCount++;
                    totalPeriodCount++;
                }

                periodCount = 0;
                roundCount++;
            }
        }



        private static IEnumerable<Player> CreatePlayers(int count, int wordCount) =>
            Enumerable.Range(0, count)
                .Select(i => new Player(
                    Guid.NewGuid(),
                    $"Player{i}",
                    Enumerable.Range(0, wordCount)
                        .Select(j => new Word(Guid.NewGuid(), $"Player{i}Word{j}"))
                        .ToList()))
                .ToList();

        private static Game Serialize(Game game, JsonSerializerOptions options) =>
            JsonSerializer.Deserialize<Game>(JsonSerializer.Serialize(game, options), options)!;
    }
}