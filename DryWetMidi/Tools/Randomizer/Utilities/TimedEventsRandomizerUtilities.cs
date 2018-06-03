using System;
using System.Collections.Generic;
using Melanchall.DryWetMidi.Common;
using Melanchall.DryWetMidi.Smf;
using Melanchall.DryWetMidi.Smf.Interaction;

namespace Melanchall.DryWetMidi.Tools
{
    /// <summary>
    /// Provides methods to randomize timed events time.
    /// </summary>
    public static class TimedEventsRandomizerUtilities
    {
        #region Methods

        /// <summary>
        /// Randomizes timed events contained in the specified <see cref="TrackChunk"/>.
        /// </summary>
        /// <param name="trackChunk"><see cref="TrackChunk"/> to randomize timed events in.</param>
        /// <param name="bounds">Bounds to randomize time within.</param>
        /// <param name="tempoMap">Tempo map used to calculate time bounds to randomize within.</param>
        /// <param name="settings">Settings according to which timed events should be randomized.</param>
        /// <exception cref="ArgumentNullException"><paramref name="trackChunk"/> is null. -or-
        /// <paramref name="bounds"/> is null. -or- <paramref name="tempoMap"/> is null.</exception>
        public static void RandomizeTimedEvents(this TrackChunk trackChunk, IBounds bounds, TempoMap tempoMap, TimedEventsRandomizingSettings settings = null)
        {
            ThrowIfArgument.IsNull(nameof(trackChunk), trackChunk);
            ThrowIfArgument.IsNull(nameof(bounds), bounds);
            ThrowIfArgument.IsNull(nameof(tempoMap), tempoMap);

            using (var timedEventsManager = trackChunk.ManageTimedEvents())
            {
                new TimedEventsRandomizer().Randomize(timedEventsManager.Events, bounds, tempoMap, settings);
            }
        }

        /// <summary>
        /// Randomizes timed events contained in the specified collection of <see cref="TrackChunk"/>.
        /// </summary>
        /// <param name="trackChunks">Collection of <see cref="TrackChunk"/> to randomize timed events in.</param>
        /// <param name="bounds">Bounds to randomize time within.</param>
        /// <param name="tempoMap">Tempo map used to calculate time bounds to randomize within.</param>
        /// <param name="settings">Settings according to which timed events should be randomized.</param>
        /// <exception cref="ArgumentNullException"><paramref name="trackChunks"/> is null. -or-
        /// <paramref name="bounds"/> is null. -or- <paramref name="tempoMap"/> is null.</exception>
        public static void RandomizeTimedEvents(this IEnumerable<TrackChunk> trackChunks, IBounds bounds, TempoMap tempoMap, TimedEventsRandomizingSettings settings = null)
        {
            ThrowIfArgument.IsNull(nameof(trackChunks), trackChunks);
            ThrowIfArgument.IsNull(nameof(bounds), bounds);
            ThrowIfArgument.IsNull(nameof(tempoMap), tempoMap);

            foreach (var trackChunk in trackChunks)
            {
                trackChunk.RandomizeTimedEvents(bounds, tempoMap, settings);
            }
        }

        /// <summary>
        /// Randomizes timed events contained in the specified <see cref="MidiFile"/>.
        /// </summary>
        /// <param name="midiFile"><see cref="MidiFile"/> to randomize timed events in.</param>
        /// <param name="bounds">Bounds to randomize time within.</param>
        /// <param name="settings">Settings according to which timed events should be randomized.</param>
        /// <exception cref="ArgumentNullException"><paramref name="midiFile"/> is null. -or-
        /// <paramref name="bounds"/> is null.</exception>
        public static void RandomizeTimedEvents(this MidiFile midiFile, IBounds bounds, TimedEventsRandomizingSettings settings = null)
        {
            ThrowIfArgument.IsNull(nameof(midiFile), midiFile);
            ThrowIfArgument.IsNull(nameof(bounds), bounds);

            var tempoMap = midiFile.GetTempoMap();

            midiFile.GetTrackChunks().RandomizeTimedEvents(bounds, tempoMap, settings);
        }

        #endregion
    }
}
