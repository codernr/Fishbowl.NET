using System;
using System.Linq;
using System.Text.Json;
using Fishbowl.Net.Shared.GameEntities;
using Fishbowl.Net.Shared.Serialization;
using Xunit;

namespace Fishbowl.Net.Tests.Shared.Serialization
{
    public class TeamConverterTests
    {
        [Fact]
        public void ShouldSerializeCorrectly()
        {
            var options = new JsonSerializerOptions(JsonSerializerDefaults.Web);
            options.Converters.Add(new TeamConverter());

            var players = new[]
            {
                CreatePlayer("Player 1", "Word 1", "Word 2"),
                CreatePlayer("Player 2", "Word 3", "Word 4"),
                CreatePlayer("Player 3", "Word 5", "Word 6")
            };

            var original = new Team(0, players);

            var json1 = JsonSerializer.Serialize(original, options);
            
            original.Name = "TeamName";
            original.PlayerEnumerator.MoveNext();
            original.PlayerEnumerator.MoveNext();

            var json2 = JsonSerializer.Serialize(original, options);

            var deserialized1 = JsonSerializer.Deserialize<Team>(json1, options);
            var deserialized2 = JsonSerializer.Deserialize<Team>(json2, options);

            Assert.Equal(0, deserialized1!.Id);

            var playerIds = players.Select(player => player.Username);
            var deserializedIds = deserialized1.Players.Select(player => player.Username).ToList();

            Assert.True(playerIds.All(deserializedIds.Contains));
            Assert.Null(deserialized1.Name);

            Assert.Equal("TeamName", deserialized2!.Name);
            Assert.Equal(players[2].Username, deserialized2.PlayerEnumerator.Current.Username);
        }

        private static Player CreatePlayer(string username, params string[] words) =>
            new(username, words.Select(word => new Word(Guid.NewGuid(), word)));
    }
}