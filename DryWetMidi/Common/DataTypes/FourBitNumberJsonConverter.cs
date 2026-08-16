#if NET7_0_OR_GREATER
using System;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Melanchall.DryWetMidi.Common
{
    internal sealed class FourBitNumberJsonConverter : JsonConverter<FourBitNumber>
    {
        public override FourBitNumber Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            if (reader.TokenType != JsonTokenType.Number)
                throw new JsonException("FourBitNumber value must be a JSON number.");

            try
            {
                return (FourBitNumber)reader.GetByte();
            }
            catch (Exception ex) when (ex is InvalidOperationException || ex is FormatException || ex is OverflowException || ex is ArgumentOutOfRangeException)
            {
                throw new JsonException("FourBitNumber value must be in range 0..15.", ex);
            }
        }

        public override void Write(Utf8JsonWriter writer, FourBitNumber value, JsonSerializerOptions options)
        {
            writer.WriteNumberValue((byte)value);
        }
    }
}
#endif