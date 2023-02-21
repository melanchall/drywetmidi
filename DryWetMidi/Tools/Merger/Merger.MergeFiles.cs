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
        #region Methods

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
            var initialState = true;

            foreach (var midiFile in midiFiles)
            {
                var tempoMap = midiFile.GetTempoMap();
                var fileDuration = GetFileDuration(midiFile, tempoMap, settings.FileDurationRoundingStep);

                var chunks = GetChunksForProcessing(midiFile, initialState);
                InsertMarkers(midiFile, chunks, fileDuration, settings);
                var deltaTimeFactor = GetDeltaTimeFactor(timeDivision, midiFile.TimeDivision);

                var newChunks = new List<MidiChunk>();

                foreach (var chunk in chunks)
                {
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

                initialState = false;
            }

            return result;
        }

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
                    // TODO: implement TempoMap.Equals

                    if (!referenceTempoMap.TimeDivision.Equals(tempoMap.TimeDivision) ||
                        !referenceTempoMap.GetTempoChanges().SequenceEqual(tempoMap.GetTempoChanges()) ||
                        !referenceTempoMap.GetTimeSignatureChanges().SequenceEqual(tempoMap.GetTimeSignatureChanges()))
                        throw new InvalidOperationException("MIDI files have different tempo maps.");
                }
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

        private static ICollection<MidiChunk> GetChunksForProcessing(
            MidiFile midiFile,
            bool initialState)
        {
            var chunks = midiFile.Chunks.Select(c => c.Clone()).ToArray();

            if (!initialState)
            {
                var zeroTimeEvents = chunks
                    .OfType<TrackChunk>()
                    .GetTimedEventsLazy(null, false)
                    .TakeWhile(e => e.Object.Time == 0)
                    .ToArray();

                var firstTrackChunk = chunks.OfType<TrackChunk>().FirstOrDefault();
                if (firstTrackChunk != null)
                {
                    if (!zeroTimeEvents.Any(e => e.Object.Event.EventType == MidiEventType.SetTempo))
                        firstTrackChunk.Events.Insert(0, new SetTempoEvent());
                    if (!zeroTimeEvents.Any(e => e.Object.Event.EventType == MidiEventType.TimeSignature))
                        firstTrackChunk.Events.Insert(0, new TimeSignatureEvent());
                }
            }

            return chunks;
        }

        private static void InsertMarkers(
            MidiFile originalMidiFile,
            ICollection<MidiChunk> chunks,
            long fileDuration,
            SequentialMergingSettings settings)
        {
            var fileStartMarkerEventFactory = settings.FileStartMarkerEventFactory;
            var fileEndMarkerEventFactory = settings.FileEndMarkerEventFactory;

            if (fileStartMarkerEventFactory == null && fileEndMarkerEventFactory == null)
                return;

            foreach (var trackChunk in chunks.OfType<TrackChunk>())
            {
                var fileStartMarkerEvent = fileStartMarkerEventFactory?.Invoke(originalMidiFile);
                if (fileStartMarkerEvent != null)
                    trackChunk.Events.Insert(0, fileStartMarkerEvent);

                var fileEndMarkerEvent = fileEndMarkerEventFactory?.Invoke(originalMidiFile);
                if (fileEndMarkerEvent != null)
                {
                    fileEndMarkerEvent.DeltaTime = fileDuration - trackChunk.Events.Sum(e => e.DeltaTime);
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
