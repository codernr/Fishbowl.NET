using System;
using System.Collections.Generic;
using System.Text.Json;
using System.Text.Json.Serialization;
using Fishbowl.Net.Shared.GameEntities;

namespace Fishbowl.Net.Shared.Serialization
{
    public class TeamConverter : JsonConverter<Team>
    {
        public override Team? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options) =>
            Map(JsonSerializer.Deserialize<Data>(ref reader, options));

        public override void Write(Utf8JsonWriter writer, Team value, JsonSerializerOptions options) =>
            JsonSerializer.Serialize(writer, Map(value), options);

        private record Data(string? Name, int Id, IList<Player> Players, int CurrentPlayerIndex);

        private static Data Map(Team team) => new(team.Name, team.Id, team.Players, team.PlayerEnumerator.Id);

        private static Team? Map(Data? data) => data is null ? null : new(data.Name, data.Id, data.Players, data.CurrentPlayerIndex);
    }
}