using Melanchall.DryWetMidi.Common;
using Melanchall.DryWetMidi.Core;
using Melanchall.DryWetMidi.Interaction;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Melanchall.DryWetMidi.Tools
{
    public static partial class Merger
    {
        #region Nested classes

        private sealed class ChunkDescriptor
        {
            public ChunkDescriptor(MidiChunk chunk)
            {
                Chunk = chunk;
            }

            public MidiChunk Chunk { get; }

            public long LastEventTime { get; set; }
        }

        #endregion

        #region Constants

        private static readonly Dictionary<MidiEventType, Func<MidiEvent, object>> EventsKeysGetters =
            new Dictionary<MidiEventType, Func<MidiEvent, object>>
            {
                [MidiEventType.SetTempo] = midiEvent => MidiEventType.SetTempo,
                [MidiEventType.TimeSignature] = midiEvent => MidiEventType.TimeSignature,
                [MidiEventType.PitchBend] = midiEvent =>
                {
                    var pitchBendEvent = (PitchBendEvent)midiEvent;
                    return Tuple.Create(MidiEventType.PitchBend, pitchBendEvent.Channel);
                },
            };

        private static readonly Dictionary<object, Func<MidiEvent>> DefaultEventsGetters = GetDefaultEventsGetters();

        private static readonly MidiEventEqualityCheckSettings MidiEventEqualityCheckSettings = new MidiEventEqualityCheckSettings
        {
            CompareDeltaTimes = false
        };

        #endregion

        #region Methods

        /// <summary>
        /// Merges the specified MIDI files sequentially so they are placed one after other in the result file.
        /// More info in the <see href="xref:a_files_merging#mergesequentially">MIDI files merging: MergeSequentially</see> article.
        /// </summary>
        /// <param name="midiFiles">MIDI files to merge.</param>
        /// <param name="settings">Settings that control how <paramref name="midiFiles"/> should be merged.</param>
        /// <returns>An instance of the <see cref="MidiFile"/> which represents <paramref name="midiFiles"/>
        /// that are merged sequentially.</returns>
        /// <exception cref="ArgumentNullException"><paramref name="midiFiles"/> is <c>null</c>.</exception>
        /// <exception cref="ArgumentException">
        /// <para>One of the following errors occurred:</para>
        /// <list type="bullet">
        /// <item>
        /// <description><paramref name="midiFiles"/> collection contains <c>null</c>.</description>
        /// </item>
        /// <item>
        /// <description><paramref name="midiFiles"/> is an empty collection.</description>
        /// </item>
        /// </list>
        /// </exception>
        /// <exception cref="InvalidOperationException">
        /// <para>One of the following errors occurred:</para>
        /// <list type="bullet">
        /// <item>
        /// <description>Time division of one of the <paramref name="midiFiles"/> is not an instance
        /// of the <see cref="TicksPerQuarterNoteTimeDivision"/>.</description>
        /// </item>
        /// <item>
        /// <description>Failed to provide common time division since its value exceeds <see cref="short.MaxValue"/>.</description>
        /// </item>
        /// </list>
        /// </exception>
        public static MidiFile MergeSequentially(
            this IEnumerable<MidiFile> midiFiles,
            SequentialMergingSettings settings = null)
        {
            ThrowIfArgument.IsNull(nameof(midiFiles), midiFiles);
            ThrowIfArgument.ContainsNull(nameof(midiFiles), midiFiles);
            ThrowIfArgument.IsEmptyCollection(nameof(midiFiles), midiFiles, "MIDI files collection is empty.");

            settings = settings ?? new SequentialMergingSettings();

            var result = PrepareResultFile(midiFiles);
            var timeDivision = (TicksPerQuarterNoteTimeDivision)result.TimeDivision;
            var resultTrackChunksCreationPolicy = settings.ResultTrackChunksCreationPolicy;

            var offset = 0L;
            var eventsContext = new Dictionary<object, MidiEvent>();

            foreach (var midiFile in midiFiles)
            {
                var tempoMap = midiFile.GetTempoMap();
                var fileDuration = GetFileDuration(midiFile, tempoMap, settings.FileDurationRoundingStep);

                var chunks = GetChunksForProcessing(midiFile, eventsContext);
                InsertMarkers(midiFile, chunks, fileDuration, settings);
                var deltaTimeFactor = GetDeltaTimeFactor(timeDivision, midiFile.TimeDivision);

                var newChunks = new List<MidiChunk>();

                foreach (var chunkDescriptor in chunks)
                {
                    var chunk = chunkDescriptor.Chunk;

                    var trackChunk = chunk as TrackChunk;
                    if (trackChunk != null)
                        ScaleTrackChunk(trackChunk, deltaTimeFactor);

                    if (trackChunk != null || settings.CopyNonTrackChunks)
                    {
                        newChunks.Add(chunk);

                        if (resultTrackChunksCreationPolicy == ResultTrackChunksCreationPolicy.CreateForEachFile)
                            result.Chunks.Add(chunk);
                    }
                }

                AddOffset(newChunks, offset);
                UpdateOffset(ref offset, fileDuration, settings.DelayBetweenFiles, deltaTimeFactor, tempoMap);

                if (resultTrackChunksCreationPolicy == ResultTrackChunksCreationPolicy.MinimizeCount)
                    AddTrackChunksMinimizingCount(result, newChunks);
            }

            return result;
        }

        /// <summary>
        /// Merges the specified MIDI files "simultaneously" so they are placed "one below other" in the result file.
        /// More info in the <see href="xref:a_files_merging#mergesimultaneously">MIDI files merging: MergeSimultaneously</see> article.
        /// </summary>
        /// <param name="midiFiles">MIDI files to merge.</param>
        /// <param name="settings">Settings that control how <paramref name="midiFiles"/> should be merged.</param>
        /// <remarks>
        /// The method has a limitation: it can process only the files which have the same tempo maps, i.e.
        /// the same changes of tempo and time signature,and the same time divisions. You can disable the
        /// exception throwing by setting <see cref="SimultaneousMergingSettings.IgnoreDifferentTempoMaps"/> of
        /// the <paramref name="settings"/> to <c>true</c>, but proper structure (in terms of correct playing for example)
        /// of the result MIDI file is not guaranteed in this case.
        /// </remarks>
        /// <returns>An instance of the <see cref="MidiFile"/> which represents <paramref name="midiFiles"/>
        /// that are merged sequentially.</returns>
        /// <exception cref="ArgumentNullException"><paramref name="midiFiles"/> is <c>null</c>.</exception>
        /// <exception cref="ArgumentException">
        /// <para>One of the following errors occurred:</para>
        /// <list type="bullet">
        /// <item>
        /// <description><paramref name="midiFiles"/> collection contains <c>null</c>.</description>
        /// </item>
        /// <item>
        /// <description><paramref name="midiFiles"/> is an empty collection.</description>
        /// </item>
        /// </list>
        /// </exception>
        /// <exception cref="InvalidOperationException">
        /// <para>One of the following errors occurred:</para>
        /// <list type="bullet">
        /// <item>
        /// <description>Time division of one of the <paramref name="midiFiles"/> is not an instance
        /// of the <see cref="TicksPerQuarterNoteTimeDivision"/>.</description>
        /// </item>
        /// <item>
        /// <description>Failed to provide common time division since its value exceeds <see cref="short.MaxValue"/>.</description>
        /// </item>
        /// <item>
        /// <description>MIDI files have different tempo maps and <see cref="SimultaneousMergingSettings.IgnoreDifferentTempoMaps"/>
        /// of the <paramref name="settings"/> is set to <c>false</c>.</description>
        /// </item>
        /// </list>
        /// </exception>
        public static MidiFile MergeSimultaneously(
            this IEnumerable<MidiFile> midiFiles,
            SimultaneousMergingSettings settings = null)
        {
            ThrowIfArgument.IsNull(nameof(midiFiles), midiFiles);
            ThrowIfArgument.ContainsNull(nameof(midiFiles), midiFiles);
            ThrowIfArgument.IsEmptyCollection(nameof(midiFiles), midiFiles, "MIDI files collection is empty.");

            settings = settings ?? new SimultaneousMergingSettings();

            var result = PrepareResultFile(midiFiles);
            var timeDivision = result.TimeDivision;

            var newTempoMaps = new List<TempoMap>();

            foreach (var midiFile in midiFiles)
            {
                var chunks = midiFile.Chunks.Select(c => c.Clone()).ToArray();
                var deltaTimeFactor = GetDeltaTimeFactor(timeDivision, midiFile.TimeDivision);

                var newTrackChunks = new List<TrackChunk>();

                foreach (var chunk in chunks)
                {
                    var trackChunk = chunk as TrackChunk;
                    if (trackChunk != null)
                    {
                        ScaleTrackChunk(trackChunk, deltaTimeFactor);
                        newTrackChunks.Add(trackChunk);
                        result.Chunks.Add(trackChunk);
                    }
                    else if (settings.CopyNonTrackChunks)
                    {
                        result.Chunks.Add(chunk);
                    }
                }

                var newTempoMap = newTrackChunks.GetTempoMap(timeDivision);
                newTempoMaps.Add(newTempoMap);
            }

            if (!settings.IgnoreDifferentTempoMaps)
            {
                var referenceTempoMap = newTempoMaps.First();

                foreach (var tempoMap in newTempoMaps.Skip(1))
                {
                    if (!referenceTempoMap.Equals(tempoMap))
                        throw new InvalidOperationException("MIDI files have different tempo maps.");
                }
            }

            return result;
        }

        private static Dictionary<object, Func<MidiEvent>> GetDefaultEventsGetters()
        {
            var result = new Dictionary<object, Func<MidiEvent>>
            {
                [MidiEventType.SetTempo] = () => new SetTempoEvent(),
                [MidiEventType.TimeSignature] = () => new TimeSignatureEvent(),
            };

            foreach (var channel in FourBitNumber.Values)
            {
                result.Add(Tuple.Create(MidiEventType.PitchBend, channel), () => new PitchBendEvent { Channel = channel });
            }

            return result;
        }

        private static void AddTrackChunksMinimizingCount(
            MidiFile result,
            List<MidiChunk> newChunks)
        {
            var trackChunksEnumerator = result.GetTrackChunks().GetEnumerator();
            var i = 0;

            for (; i < newChunks.Count && trackChunksEnumerator.MoveNext(); i++)
            {
                for (; i < newChunks.Count && !(newChunks[i] is TrackChunk); i++)
                {
                    result.Chunks.Add(newChunks[i]);
                }

                if (i >= newChunks.Count)
                    break;

                var resultTrackChunk = trackChunksEnumerator.Current;
                var newTrackChunk = (TrackChunk)newChunks[i];
                if (!newTrackChunk.Events.Any())
                    continue;

                newTrackChunk.Events.First().DeltaTime -= resultTrackChunk.Events.Sum(e => e.DeltaTime);
                resultTrackChunk.Events.AddRange(newTrackChunk.Events);
            }

            for (; i < newChunks.Count; i++)
            {
                result.Chunks.Add(newChunks[i]);
            }

            trackChunksEnumerator.Dispose();
        }

        private static void ScaleTrackChunk(TrackChunk trackChunk, int deltaTimeFactor)
        {
            foreach (var midiEvent in trackChunk.Events)
            {
                midiEvent.DeltaTime *= deltaTimeFactor;
            }
        }

        private static int GetDeltaTimeFactor(TimeDivision baseTimeDivision, TimeDivision timeDivision)
        {
            var ticksPerQuarterNote = ((TicksPerQuarterNoteTimeDivision)timeDivision).TicksPerQuarterNote;
            return ((TicksPerQuarterNoteTimeDivision)baseTimeDivision).TicksPerQuarterNote / ticksPerQuarterNote;
        }

        private static long GetFileDuration(
            MidiFile midiFile,
            TempoMap tempoMap,
            ITimeSpan fileDurationRoundingStep)
        {
            var fileDuration = midiFile.GetDuration<MidiTimeSpan>().TimeSpan;
            if (fileDurationRoundingStep != null)
            {
                var roundedDuration = ((MidiTimeSpan)fileDuration).Round(TimeSpanRoundingPolicy.RoundUp, 0, fileDurationRoundingStep, tempoMap);
                fileDuration = TimeConverter.ConvertFrom(roundedDuration, tempoMap);
            }

            return fileDuration;
        }

        private static void AddOffset(IEnumerable<MidiChunk> chunks, long offset)
        {
            foreach (var trackChunk in chunks.OfType<TrackChunk>())
            {
                var events = trackChunk.Events;
                if (events.Any())
                    events[0].DeltaTime += offset;
            }
        }

        private static ICollection<ChunkDescriptor> GetChunksForProcessing(
            MidiFile midiFile,
            Dictionary<object, MidiEvent> eventsContext)
        {
            var chunksCount = midiFile.Chunks.Count;
            var result = new ChunkDescriptor[chunksCount];

            TrackChunk firstTrackChunk = null;

            var eventsAtStart = new Dictionary<object, MidiEvent>();
            var trackedEvents = new Dictionary<object, Tuple<MidiEvent, long>>();

            for (var i = 0; i < chunksCount; i++)
            {
                var chunk = midiFile.Chunks[i].Clone();
                result[i] = new ChunkDescriptor(chunk);

                var trackChunk = chunk as TrackChunk;
                if (trackChunk == null)
                    continue;

                firstTrackChunk = firstTrackChunk ?? trackChunk;

                var events = trackChunk.Events;
                var evensCount = events.Count;
                var time = 0L;

                for (var j = 0; j < evensCount; j++)
                {
                    var midiEvent = events[j];
                    time += midiEvent.DeltaTime;

                    Func<MidiEvent, object> keyGetter;
                    EventsKeysGetters.TryGetValue(midiEvent.EventType, out keyGetter);

                    if (keyGetter != null)
                    {
                        var key = keyGetter(midiEvent);
                        if (time == 0)
                            eventsAtStart[key] = midiEvent;

                        Tuple<MidiEvent, long> trackedEvent;
                        if (!trackedEvents.TryGetValue(key, out trackedEvent))
                            trackedEvents.Add(key, trackedEvent = Tuple.Create(midiEvent, time));

                        if (time >= trackedEvent.Item2)
                            trackedEvents[key] = Tuple.Create(midiEvent, time);
                    }
                }

                result[i].LastEventTime = time;
            }

            if (firstTrackChunk != null)
            {
                foreach (var keyToEvent in eventsContext)
                {
                    MidiEvent midiEvent;
                    if (eventsAtStart.TryGetValue(keyToEvent.Key, out midiEvent))
                        continue;

                    Func<MidiEvent> defaultEventGetter;
                    if (!DefaultEventsGetters.TryGetValue(keyToEvent.Key, out defaultEventGetter))
                        continue;

                    var defaultEvent = defaultEventGetter();
                    
                    string message;
                    if (!MidiEvent.Equals(keyToEvent.Value, defaultEvent, MidiEventEqualityCheckSettings, out message))
                        firstTrackChunk.Events.Insert(0, defaultEvent);
                }
            }

            foreach (var keyToTrackedEvent in trackedEvents)
            {
                eventsContext[keyToTrackedEvent.Key] = keyToTrackedEvent.Value.Item1;
            }

            return result;
        }

        private static void InsertMarkers(
            MidiFile originalMidiFile,
            ICollection<ChunkDescriptor> chunks,
            long fileDuration,
            SequentialMergingSettings settings)
        {
            var fileStartMarkerEventFactory = settings.FileStartMarkerEventFactory;
            var fileEndMarkerEventFactory = settings.FileEndMarkerEventFactory;

            if (fileStartMarkerEventFactory == null && fileEndMarkerEventFactory == null)
                return;

            foreach (var chunkDescriptor in chunks)
            {
                var trackChunk = chunkDescriptor.Chunk as TrackChunk;
                if (trackChunk == null)
                    continue;

                var fileStartMarkerEvent = fileStartMarkerEventFactory?.Invoke(originalMidiFile);
                if (fileStartMarkerEvent != null)
                    trackChunk.Events.Insert(0, fileStartMarkerEvent);

                var fileEndMarkerEvent = fileEndMarkerEventFactory?.Invoke(originalMidiFile);
                if (fileEndMarkerEvent != null)
                {
                    fileEndMarkerEvent.DeltaTime = fileDuration - chunkDescriptor.LastEventTime;
                    trackChunk.Events.Add(fileEndMarkerEvent);
                }
            }
        }

        private static void UpdateOffset(
            ref long offset,
            long fileDuration,
            ITimeSpan delayBetweenFiles,
            int deltaTimeFactor,
            TempoMap tempoMap)
        {
            offset += fileDuration * deltaTimeFactor;
            if (delayBetweenFiles != null)
                offset += LengthConverter.ConvertFrom(delayBetweenFiles, fileDuration, tempoMap) * deltaTimeFactor;
        }

        private static MidiFile PrepareResultFile(IEnumerable<MidiFile> midiFiles)
        {
            var lastTicksPerQuarterNote = 1L;

            foreach (var timeDivision in midiFiles.Select(f => f.TimeDivision))
            {
                var ticksPerQuarterNote = (timeDivision as TicksPerQuarterNoteTimeDivision)?.TicksPerQuarterNote;
                if (ticksPerQuarterNote == null)
                    throw new InvalidOperationException($"Time division '{timeDivision}' can't be processed. Only ticks-per-quarter-note ones are supported.");

                lastTicksPerQuarterNote = MathUtilities.LeastCommonMultiple(lastTicksPerQuarterNote, ticksPerQuarterNote.Value);
            }

            if (lastTicksPerQuarterNote > short.MaxValue)
                throw new InvalidOperationException($"Failed to provide common time division since its value exceeds {short.MaxValue}.");

            return new MidiFile
            {
                TimeDivision = new TicksPerQuarterNoteTimeDivision((short)lastTicksPerQuarterNote)
            };
        }

        #endregion
    }
}
