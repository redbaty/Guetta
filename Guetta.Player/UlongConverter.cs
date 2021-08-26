using System;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Guetta.Player
{
    public class UlongConverter : JsonConverter<ulong>
    {
        public override ulong Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            return ulong.Parse(reader.GetString() ?? throw new InvalidOperationException());
        }

        public override void Write(Utf8JsonWriter writer, ulong value, JsonSerializerOptions options)
        {
            throw new NotImplementedException();
        }
    }
}