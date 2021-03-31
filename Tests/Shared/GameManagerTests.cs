using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Fishbowl.Net.Shared;
using Fishbowl.Net.Shared.Data;
using Xunit;

namespace Fishbowl.Net.Tests.Shared
{
    public class GameManagerTests
    {
        [Fact]
        public void ShouldThrowWithInvalidTeamConfig()
        {
            Assert.Throws<ArgumentException>(() => new GameManager(
                Guid.NewGuid(),
                new[]
                {
                    new Player(Guid.NewGuid(), "Player1", new Word[0]),
                    new Player(Guid.NewGuid(), "Player2", new Word[0]),
                    new Player(Guid.NewGuid(), "Player3", new Word[0]),
                },
                new string[0],
                2));
        }

        [Fact]
        public void TestRunWithNoPasses()
        {
            var players = CreatePlayers(5, 2);

            var rounds = new[] { "GameType1", "GameType2" };

            var gameManager = new GameManager(
                Guid.NewGuid(),
                players,
                rounds,
                2,
                false);

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

            foreach (var round in gameManager.GetRounds())
            {
                Assert.Equal(rounds[roundCount], round.Type);

                foreach (var period in gameManager.GetPeriods())
                {
                    Assert.Equal(playerList[totalPeriodCount], period.Player.Name);

                    var now = new DateTimeOffset(2021, 3, 31, 10, 0, 0, TimeSpan.Zero);

                    Word? previousWord = null;

                    while(gameManager.NextWord(now, previousWord))
                    {
                        Assert.Equal(guessedWords[roundCount][periodCount][wordCount], gameManager.CurrentWord.Value);
                        
                        previousWord = gameManager.CurrentWord;
                        now += TimeSpan.FromSeconds(10);
                        wordCount++;
                        totalWordCount++;
                    }

                    wordCount = 0;
                    periodCount++;
                    totalPeriodCount++;
                }

                periodCount = 0;
                roundCount++;
            }

            Assert.Equal(20, totalWordCount);
        }

        [Fact]
        public void TestRunWithPasses()
        {
            var players = CreatePlayers(5, 2);

            var rounds = new[] { "GameType1", "GameType2" };

            var gameManager = new GameManager(
                Guid.NewGuid(),
                players,
                rounds,
                2,
                false);

            int roundCount = 0;
            int periodCount = 0;
            int totalPeriodCount = 0;
            int wordCount = 0;
            int totalWordCount = 0;

            var currentWord = new[]
            {
                new[]
                {
                    new[] { "Player0Word0", "Player0Word1", "Player1Word0", "Player1Word1", "Player2Word0", "Player2Word1" },
                    new[] { "Player2Word1", "Player3Word0", "Player3Word1", "Player4Word0", "Player4Word1" }
                },
                new[]
                {
                    new[] { "Player0Word0" },
                    new[] { "Player0Word0", "Player0Word1", "Player1Word0", "Player1Word1", "Player2Word0", "Player2Word1" },
                    new[] { "Player2Word1", "Player3Word0", "Player3Word1", "Player4Word0", "Player4Word1" }
                }
            };

            var playerList = new[] { "Player0", "Player1", "Player1", "Player2", "Player3" };

            foreach (var round in gameManager.GetRounds())
            {
                Assert.Equal(rounds[roundCount], round.Type);

                foreach (var period in gameManager.GetPeriods())
                {
                    Assert.Equal(playerList[totalPeriodCount], period.Player.Name);

                    var start = new DateTimeOffset(2021, 3, 31, 10, 0, 0, TimeSpan.Zero);
                    var now = start;

                    Word? previousWord = null;

                    while(gameManager.NextWord(now, previousWord))
                    {
                        Assert.Equal(currentWord[roundCount][periodCount][wordCount], gameManager.CurrentWord.Value);
                        
                        now += TimeSpan.FromSeconds(10);
                        if (now >= start + period.Length)
                        {
                            previousWord = null;
                        }
                        else
                        {
                            previousWord = gameManager.CurrentWord;
                            totalWordCount++;
                        }
                        wordCount++;
                    }

                    wordCount = 0;
                    periodCount++;
                    totalPeriodCount++;
                }

                periodCount = 0;
                roundCount++;
            }

            Assert.Equal(20, totalWordCount);
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