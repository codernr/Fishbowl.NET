using System;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Fishbowl.Net.Shared.Serialization
{
    public class TypeConverter<TInterface, TConcrete> : JsonConverter<TInterface> where TConcrete : TInterface
    {
        public override TInterface? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options) =>
            JsonSerializer.Deserialize<TConcrete>(ref reader, options);

        public override void Write(Utf8JsonWriter writer, TInterface value, JsonSerializerOptions options) =>
            JsonSerializer.Serialize<TConcrete>(writer, (TConcrete)value!, options);

    }
}