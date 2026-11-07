using Melanchall.DryWetMidi.Common;
using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace DwmAotApp
{
    [JsonSerializable(typeof(FourBitNumber))]
    [JsonSerializable(typeof(SevenBitNumber))]
    [JsonSerializable(typeof(Dictionary<FourBitNumber, SevenBitNumber>), TypeInfoPropertyName = "FourBitNumberDictionary")]
    [JsonSerializable(typeof(Dictionary<SevenBitNumber, FourBitNumber>), TypeInfoPropertyName = "SevenBitNumberDictionary")]
    internal partial class AotJsonContext : JsonSerializerContext
    {
    }
}
