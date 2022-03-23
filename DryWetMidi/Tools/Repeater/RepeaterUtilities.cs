using Melanchall.DryWetMidi.Common;
using Melanchall.DryWetMidi.Core;
using Melanchall.DryWetMidi.Interaction;
using System.Collections.Generic;

namespace Melanchall.DryWetMidi.Tools
{
    public static class RepeaterUtilities
    {
        #region Methods

        public static MidiFile Repeat(this MidiFile midiFile, int repeatsNumber, RepeatingSettings settings = null)
        {
            ThrowIfArgument.IsNull(nameof(midiFile), midiFile);
            ThrowIfArgument.IsNonpositive(nameof(repeatsNumber), repeatsNumber, "Repeats number is zero or negative.");

            return new Repeater().Repeat(midiFile, repeatsNumber, settings);
        }

        public static TrackChunk Repeat(this TrackChunk trackChunk, int repeatsNumber, TempoMap tempoMap, RepeatingSettings settings = null)
        {
            ThrowIfArgument.IsNull(nameof(trackChunk), trackChunk);
            ThrowIfArgument.IsNull(nameof(tempoMap), tempoMap);
            ThrowIfArgument.IsNonpositive(nameof(repeatsNumber), repeatsNumber, "Repeats number is zero or negative.");

            return new Repeater().Repeat(trackChunk, repeatsNumber, tempoMap, settings);
        }

        public static ICollection<TrackChunk> Repeat(this IEnumerable<TrackChunk> trackChunks, int repeatsNumber, TempoMap tempoMap, RepeatingSettings settings = null)
        {
            ThrowIfArgument.IsNull(nameof(trackChunks), trackChunks);
            ThrowIfArgument.IsNull(nameof(tempoMap), tempoMap);
            ThrowIfArgument.IsNonpositive(nameof(repeatsNumber), repeatsNumber, "Repeats number is zero or negative.");

            return new Repeater().Repeat(trackChunks, repeatsNumber, tempoMap, settings);
        }

        public static ICollection<ITimedObject> Repeat(this IEnumerable<ITimedObject> timedObjects, int repeatsNumber, TempoMap tempoMap, RepeatingSettings settings = null)
        {
            ThrowIfArgument.IsNull(nameof(timedObjects), timedObjects);
            ThrowIfArgument.IsNull(nameof(tempoMap), tempoMap);
            ThrowIfArgument.IsNonpositive(nameof(repeatsNumber), repeatsNumber, "Repeats number is zero or negative.");

            return new Repeater().Repeat(timedObjects, repeatsNumber, tempoMap, settings);
        }

        #endregion
    }
}
