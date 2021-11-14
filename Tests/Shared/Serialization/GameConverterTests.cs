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
            options.Converters.Add(new RoundConverter());
            options.Converters.Add(new ShuffleEnumeratorConverter());
            options.WriteIndented = true;

            var teams = CreatePlayers(5, 2).CreateTeams(2);
            
            var rounds = new[] { "GameType1", "GameType2" };

            var game = new Game(Guid.NewGuid(), new(), teams, rounds, true);

            int roundCount = 0;
            int periodCount = 0;
            int totalPeriodCount = 0;
            int wordCount = 0;
            int totalWordCount = 0;

            string serialized = JsonSerializer.Serialize(game, options);
            Game deserialized = JsonSerializer.Deserialize<Game>(serialized, options)!;

            foreach (var round in game.RoundLoop())
            {
                serialized = JsonSerializer.Serialize(game, options);
                deserialized = JsonSerializer.Deserialize<Game>(serialized, options)!;

                foreach (var period in game.PeriodLoop())
                {
                    serialized = JsonSerializer.Serialize(game, options);
                    deserialized = JsonSerializer.Deserialize<Game>(serialized, options)!;

                    var now = new DateTimeOffset(2021, 3, 31, 10, 0, 0, TimeSpan.Zero);

                    game.StartPeriod(now);

                    do
                    {
                        serialized = JsonSerializer.Serialize(game, options);
                        deserialized = JsonSerializer.Deserialize<Game>(serialized, options)!;
                        
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
    }
}