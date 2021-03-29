using System;
using System.Collections.Generic;
using System.Linq;
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
        public void ShouldRunToGameFinished()
        {
            var players = CreatePlayers(5, 2);

            var gameManager = new GameManager(
                Guid.NewGuid(),
                players,
                new[] { "GameType1", "GameType2" },
                2) { Randomize = false };

            Assert.Equal("GameType1", gameManager.NextRound.Type);
            
            var (period, firstWord) = gameManager.SetupPeriod();

            Assert.Equal("Player0Word0", firstWord.Value);
        }

        private static IEnumerable<Player> CreatePlayers(int count, int wordCount) =>
            Enumerable.Range(0, count)
                .Select(i => new Player(
                    Guid.NewGuid(),
                    $"Player{i}",
                    Enumerable.Range(0, wordCount)
                        .Select(j => new Word(Guid.NewGuid(), $"Player{i}Word{j}"))));
    }
}