using Melanchall.DryWetMidi.Common;
using Melanchall.DryWetMidi.Core;
using Melanchall.DryWetMidi.Interaction;
using System.Collections.Generic;

namespace Melanchall.DryWetMidi.Tools
{
    public static class MidiRepeaterUtilities
    {
        #region Methods

        public static MidiFile Repeat(this MidiFile midiFile, int repeatsNumber, MidiRepeaterSettings settings = null)
        {
            ThrowIfArgument.IsNull(nameof(midiFile), midiFile);
            ThrowIfArgument.IsNonpositive(nameof(repeatsNumber), repeatsNumber, "Repeats number is zero or negative.");

            return new MidiRepeater().Repeat(midiFile, repeatsNumber, settings);
        }

        public static TrackChunk Repeat(this TrackChunk trackChunk, int repeatsNumber, TempoMap tempoMap, MidiRepeaterSettings settings = null)
        {
            ThrowIfArgument.IsNull(nameof(trackChunk), trackChunk);
            ThrowIfArgument.IsNull(nameof(tempoMap), tempoMap);
            ThrowIfArgument.IsNonpositive(nameof(repeatsNumber), repeatsNumber, "Repeats number is zero or negative.");

            return new MidiRepeater().Repeat(trackChunk, repeatsNumber, tempoMap, settings);
        }

        public static ICollection<TrackChunk> Repeat(this IEnumerable<TrackChunk> trackChunks, int repeatsNumber, TempoMap tempoMap, MidiRepeaterSettings settings = null)
        {
            ThrowIfArgument.IsNull(nameof(trackChunks), trackChunks);
            ThrowIfArgument.IsNull(nameof(tempoMap), tempoMap);
            ThrowIfArgument.IsNonpositive(nameof(repeatsNumber), repeatsNumber, "Repeats number is zero or negative.");

            return new MidiRepeater().Repeat(trackChunks, repeatsNumber, tempoMap, settings);
        }

        public static ICollection<ITimedObject> Repeat(this IEnumerable<ITimedObject> timedObjects, int repeatsNumber, TempoMap tempoMap, MidiRepeaterSettings settings = null)
        {
            ThrowIfArgument.IsNull(nameof(timedObjects), timedObjects);
            ThrowIfArgument.IsNull(nameof(tempoMap), tempoMap);
            ThrowIfArgument.IsNonpositive(nameof(repeatsNumber), repeatsNumber, "Repeats number is zero or negative.");

            return new MidiRepeater().Repeat(timedObjects, repeatsNumber, tempoMap, settings);
        }

        #endregion
    }
}
