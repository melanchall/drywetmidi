#if NET7_0_OR_GREATER
using System;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Melanchall.DryWetMidi.Common
{
    /// <summary>
    /// Converts <see cref="FourBitNumber"/> values to and from JSON numbers and dictionary keys.
    /// </summary>
    /// <remarks>
    /// This converter is automatically selected by the <see cref="FourBitNumber"/> JSON converter
    /// attribute and supports source-generated serialization contexts. Available on .NET 7 and later.
    /// </remarks>
    public sealed class FourBitNumberJsonConverter : JsonConverter<FourBitNumber>
    {
        /// <inheritdoc/>
        /// <exception cref="JsonException">The JSON value is not an integer in the range 0-15.</exception>
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

        /// <inheritdoc/>
        public override void Write(Utf8JsonWriter writer, FourBitNumber value, JsonSerializerOptions options)
        {
            writer.WriteNumberValue((byte)value);
        }

        /// <inheritdoc/>
        /// <exception cref="JsonException">The dictionary key is not a valid four-bit number.</exception>
        public override FourBitNumber ReadAsPropertyName(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            var keyText = reader.GetString() ?? throw new JsonException("Key cannot be null.");

            if (FourBitNumber.TryParse(keyText, out var result))
                return result;

            throw new JsonException($"Invalid FourBitNumber key: {keyText}");
        }

        /// <inheritdoc/>
        public override void WriteAsPropertyName(Utf8JsonWriter writer, FourBitNumber value, JsonSerializerOptions options)
        {
            writer.WritePropertyName(value.ToString());
        }
    }
}
#endif