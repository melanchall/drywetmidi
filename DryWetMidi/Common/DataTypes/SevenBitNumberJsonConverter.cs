#if NET7_0_OR_GREATER
using System;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Melanchall.DryWetMidi.Common
{
    internal sealed class SevenBitNumberJsonConverter : JsonConverter<SevenBitNumber>
    {
        public override SevenBitNumber Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            if (reader.TokenType != JsonTokenType.Number)
                throw new JsonException("SevenBitNumber value must be a JSON number.");

            try
            {
                return (SevenBitNumber)reader.GetByte();
            }
            catch (Exception ex)
            {
                throw new JsonException("SevenBitNumber value must be in range 0-127.", ex);
            }
        }

        public override void Write(Utf8JsonWriter writer, SevenBitNumber value, JsonSerializerOptions options)
        {
            writer.WriteNumberValue((byte)value);
        }
    }
}
#endif