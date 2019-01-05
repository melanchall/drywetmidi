using System;
using System.Collections.Generic;
using System.Linq;
using Melanchall.DryWetMidi.Common;
using Melanchall.DryWetMidi.Smf;
using Melanchall.DryWetMidi.Smf.Interaction;

namespace Melanchall.DryWetMidi.Devices
{
    public static class PlaybackUtilities
    {
        #region Methods

        public static Playback GetPlayback(this TrackChunk trackChunk, TempoMap tempoMap, OutputDevice outputDevice)
        {
            ThrowIfArgument.IsNull(nameof(trackChunk), trackChunk);
            ThrowIfArgument.IsNull(nameof(tempoMap), tempoMap);
            ThrowIfArgument.IsNull(nameof(outputDevice), outputDevice);

            return new Playback(trackChunk.Events, tempoMap, outputDevice);
        }

        public static Playback GetPlayback(this IEnumerable<TrackChunk> trackChunks, TempoMap tempoMap, OutputDevice outputDevice)
        {
            ThrowIfArgument.IsNull(nameof(trackChunks), trackChunks);
            ThrowIfArgument.IsNull(nameof(tempoMap), tempoMap);
            ThrowIfArgument.IsNull(nameof(outputDevice), outputDevice);

            return new Playback(trackChunks.Select(c => c.Events), tempoMap, outputDevice);
        }

        public static Playback GetPlayback(this MidiFile midiFile, OutputDevice outputDevice)
        {
            ThrowIfArgument.IsNull(nameof(midiFile), midiFile);
            ThrowIfArgument.IsNull(nameof(outputDevice), outputDevice);

            return GetPlayback(midiFile.GetTrackChunks(), midiFile.GetTempoMap(), outputDevice);
        }

        public static Playback GetPlayback(this Pattern pattern, TempoMap tempoMap, FourBitNumber channel, OutputDevice outputDevice)
        {
            ThrowIfArgument.IsNull(nameof(pattern), pattern);
            ThrowIfArgument.IsNull(nameof(tempoMap), tempoMap);
            ThrowIfArgument.IsNull(nameof(outputDevice), outputDevice);

            return pattern.ToTrackChunk(tempoMap, channel).GetPlayback(tempoMap, outputDevice);
        }

        public static Playback GetPlayback(this IEnumerable<Note> notes, TempoMap tempoMap, OutputDevice outputDevice, bool ignoreStartTime)
        {
            ThrowIfArgument.IsNull(nameof(notes), notes);
            ThrowIfArgument.IsNull(nameof(tempoMap), tempoMap);
            ThrowIfArgument.IsNull(nameof(outputDevice), outputDevice);

            return GetPlayback(notes, events => events.AddNotes(notes), tempoMap, outputDevice, ignoreStartTime);
        }

        public static Playback GetPlayback(this IEnumerable<Chord> chords, TempoMap tempoMap, OutputDevice outputDevice, bool ignoreStartTime)
        {
            ThrowIfArgument.IsNull(nameof(chords), chords);
            ThrowIfArgument.IsNull(nameof(tempoMap), tempoMap);
            ThrowIfArgument.IsNull(nameof(outputDevice), outputDevice);

            return GetPlayback(chords, events => events.AddChords(chords), tempoMap, outputDevice, ignoreStartTime);
        }

        public static void Play(this TrackChunk trackChunk, TempoMap tempoMap, OutputDevice outputDevice)
        {
            ThrowIfArgument.IsNull(nameof(trackChunk), trackChunk);
            ThrowIfArgument.IsNull(nameof(tempoMap), tempoMap);
            ThrowIfArgument.IsNull(nameof(outputDevice), outputDevice);

            using (var playback = trackChunk.GetPlayback(tempoMap, outputDevice))
            {
                playback.Start();
            }
        }

        public static void Play(this IEnumerable<TrackChunk> trackChunks, TempoMap tempoMap, OutputDevice outputDevice)
        {
            ThrowIfArgument.IsNull(nameof(trackChunks), trackChunks);
            ThrowIfArgument.IsNull(nameof(tempoMap), tempoMap);
            ThrowIfArgument.IsNull(nameof(outputDevice), outputDevice);

            using (var playback = trackChunks.GetPlayback(tempoMap, outputDevice))
            {
                playback.Start();
            }
        }

        public static void Play(this MidiFile midiFile, OutputDevice outputDevice)
        {
            ThrowIfArgument.IsNull(nameof(midiFile), midiFile);
            ThrowIfArgument.IsNull(nameof(outputDevice), outputDevice);

            midiFile.GetTrackChunks().Play(midiFile.GetTempoMap(), outputDevice);
        }

        public static void Play(this Pattern pattern, TempoMap tempoMap, FourBitNumber channel, OutputDevice outputDevice)
        {
            ThrowIfArgument.IsNull(nameof(pattern), pattern);
            ThrowIfArgument.IsNull(nameof(tempoMap), tempoMap);
            ThrowIfArgument.IsNull(nameof(outputDevice), outputDevice);

            pattern.ToTrackChunk(tempoMap, channel).Play(tempoMap, outputDevice);
        }

        public static void Play(this IEnumerable<Note> notes, TempoMap tempoMap, OutputDevice outputDevice, bool ignoreStartTime)
        {
            ThrowIfArgument.IsNull(nameof(notes), notes);
            ThrowIfArgument.IsNull(nameof(tempoMap), tempoMap);
            ThrowIfArgument.IsNull(nameof(outputDevice), outputDevice);

            notes.GetPlayback(tempoMap, outputDevice, ignoreStartTime).Start();
        }

        public static void Play(this IEnumerable<Chord> chords, TempoMap tempoMap, OutputDevice outputDevice, bool ignoreStartTime)
        {
            ThrowIfArgument.IsNull(nameof(chords), chords);
            ThrowIfArgument.IsNull(nameof(tempoMap), tempoMap);
            ThrowIfArgument.IsNull(nameof(outputDevice), outputDevice);

            chords.GetPlayback(tempoMap, outputDevice, ignoreStartTime).Start();
        }

        private static Playback GetPlayback<TObject>(this IEnumerable<TObject> objects,
                                                     Action<EventsCollection> objectsToEventsCollectionAdder,
                                                     TempoMap tempoMap,
                                                     OutputDevice outputDevice,
                                                     bool ignoreStartTime)
            where TObject : ILengthedObject
        {
            var eventsCollection = new EventsCollection();
            objectsToEventsCollectionAdder(eventsCollection);
            if (ignoreStartTime && eventsCollection.Any())
                eventsCollection.First().DeltaTime = 0;

            return new Playback(eventsCollection, tempoMap, outputDevice);
        }

        #endregion
    }
}
