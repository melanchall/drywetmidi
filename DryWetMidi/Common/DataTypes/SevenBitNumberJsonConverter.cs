#if NET7_0_OR_GREATER
using System;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Melanchall.DryWetMidi.Common
{
    /// <summary>
    /// Converts <see cref="SevenBitNumber"/> values to and from JSON numbers and dictionary keys.
    /// </summary>
    /// <remarks>
    /// This converter is automatically selected by the <see cref="SevenBitNumber"/> JSON converter
    /// attribute and supports source-generated serialization contexts. Available on .NET 7 and later.
    /// </remarks>
    public sealed class SevenBitNumberJsonConverter : JsonConverter<SevenBitNumber>
    {
        /// <inheritdoc/>
        /// <exception cref="JsonException">The JSON value is not an integer in the range 0-127.</exception>
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

        /// <inheritdoc/>
        public override void Write(Utf8JsonWriter writer, SevenBitNumber value, JsonSerializerOptions options)
        {
            writer.WriteNumberValue((byte)value);
        }

        /// <inheritdoc/>
        /// <exception cref="JsonException">The dictionary key is not a valid seven-bit number.</exception>
        public override SevenBitNumber ReadAsPropertyName(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            var keyText = reader.GetString() ?? throw new JsonException("Key cannot be null.");

            if (SevenBitNumber.TryParse(keyText, out var result))
                return result;

            throw new JsonException($"Invalid SevenBitNumber key: {keyText}");
        }

        /// <inheritdoc/>
        public override void WriteAsPropertyName(Utf8JsonWriter writer, SevenBitNumber value, JsonSerializerOptions options)
        {
            writer.WritePropertyName(value.ToString());
        }
    }
}
#endif