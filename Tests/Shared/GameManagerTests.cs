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
        public async Task ShouldRun()
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
                    new[] { "Player4Word1", "Player4Word0", "Player3Word1", "Player3Word0", "Player2Word1", "Player2Word0" },
                    new[] { "Player1Word1", "Player1Word0", "Player0Word1", "Player0Word0" }
                },
                new[]
                {
                    new[] { "Player4Word1", "Player4Word0" },
                    new[] { "Player3Word1", "Player3Word0", "Player2Word1", "Player2Word0", "Player1Word1", "Player1Word0" },
                    new[] { "Player0Word1", "Player0Word0" }
                }
            };

            var playerList = new[] { "Player0", "Player1", "Player1", "Player2", "Player3" };

            await foreach (var round in gameManager.GetRounds())
            {
                Assert.Equal(rounds[roundCount], round.Type);

                await foreach (var period in gameManager.GetPeriods())
                {
                    var submissionItem = new SubmissionItem();

                    var submissions = submissionItem.Submissions();

                    Assert.Equal(playerList[totalPeriodCount], period.Player.Name);

                    await foreach (var remoteWord in gameManager.GetWords(submissions))
                    {
                        Assert.Equal(guessedWords[roundCount][periodCount][wordCount++], remoteWord.Value);
                        submissionItem.Word = remoteWord;
                        submissionItem.Now += TimeSpan.FromSeconds(10);
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

        private class SubmissionItem
        {
            public DateTimeOffset Now { get; set; } = DateTimeOffset.UtcNow;

            public Word? Word { get; set; } = null;

            public async IAsyncEnumerable<(Word?, DateTimeOffset)> Submissions()
            {
                while (true)
                {
                    yield return (this.Word, this.Now);
                    await Task.CompletedTask;
                }
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