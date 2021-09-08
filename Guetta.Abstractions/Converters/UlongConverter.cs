using System;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Guetta.Abstractions.Converters
{
    public class UlongConverter : JsonConverter<ulong>
    {
        public override ulong Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            return reader.TokenType == JsonTokenType.Number
                ? reader.GetUInt64()
                : ulong.Parse(reader.GetString() ?? throw new InvalidOperationException());
        }

        public override void Write(Utf8JsonWriter writer, ulong value, JsonSerializerOptions options)
        {
            writer.WriteNumberValue(value);
        }
    }
}