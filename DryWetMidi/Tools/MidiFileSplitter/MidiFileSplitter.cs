using System;
using System.Collections.Generic;
using System.Linq;
using Melanchall.DryWetMidi.Common;
using Melanchall.DryWetMidi.Core;
using Melanchall.DryWetMidi.Interaction;

namespace Melanchall.DryWetMidi.Tools
{
    /// <summary>
    /// Provides methods to split a MIDI file.
    /// </summary>
    /// <remarks>
    /// See <see href="xref:a_file_splitter">MidiFileSplitter</see> article to learn more.
    /// </remarks>
    public static class MidiFileSplitter
    {
        #region Methods

        /// <summary>
        /// Splits <see cref="MidiFile"/> by chunks within it.
        /// </summary>
        /// <param name="settings">Settings accoridng to which MIDI file should be split.</param>
        /// <param name="midiFile"><see cref="MidiFile"/> to split.</param>
        /// <returns>Collection of <see cref="MidiFile"/> where each file contains single chunk from
        /// the original file.</returns>
        /// <exception cref="ArgumentNullException"><paramref name="midiFile"/> is <c>null</c>.</exception>
        public static IEnumerable<MidiFile> SplitByChunks(this MidiFile midiFile, SplitFileByChunksSettings settings = null)
        {
            ThrowIfArgument.IsNull(nameof(midiFile), midiFile);

            settings = settings ?? new SplitFileByChunksSettings();

            foreach (var midiChunk in midiFile.Chunks.Where(c => settings.Filter?.Invoke(c) != false))
            {
                yield return new MidiFile(midiChunk.Clone())
                {
                    TimeDivision = midiFile.TimeDivision.Clone()
                };
            }
        }

        /// <summary>
        /// Splits <see cref="MidiFile"/> by channel.
        /// </summary>
        /// <param name="settings">Settings accoridng to which MIDI file should be split.</param>
        /// <param name="midiFile"><see cref="MidiFile"/> to split.</param>
        /// <returns>Collection of <see cref="MidiFile"/> where each file contains events for single channel
        /// and meta and sysex ones as defined by <paramref name="settings"/>.</returns>
        /// <exception cref="ArgumentNullException"><paramref name="midiFile"/> is <c>null</c>.</exception>
        public static IEnumerable<MidiFile> SplitByChannel(this MidiFile midiFile, SplitFileByChannelSettings settings = null)
        {
            ThrowIfArgument.IsNull(nameof(midiFile), midiFile);

            settings = settings ?? new SplitFileByChannelSettings();

            var channelsUsed = new bool[FourBitNumber.MaxValue + 1];
            var timedEventsByChannel = FourBitNumber.Values.ToDictionary(
                channel => channel,
                channel => new List<TimedEvent>());

            var timedEvents = midiFile.GetTrackChunks().GetTimedEventsLazy();

            var filter = settings.Filter;
            if (filter != null)
                timedEvents = timedEvents.Where(e => filter(e.Item1));

            foreach (var timedEventTuple in timedEvents)
            {
                var timedEvent = timedEventTuple.Item1;

                var channelEvent = timedEvent.Event as ChannelEvent;
                if (channelEvent != null)
                {
                    timedEventsByChannel[channelEvent.Channel].Add(timedEvent);
                    channelsUsed[channelEvent.Channel] = true;
                }
                else if (settings.CopyNonChannelEventsToEachFile)
                {
                    foreach (var channel in FourBitNumber.Values)
                    {
                        timedEventsByChannel[channel].Add(timedEvent);
                    }
                }
            }

            if (Array.TrueForAll(channelsUsed, c => !c))
            {
                var midiFileClone = midiFile.Clone();
                if (filter != null)
                    midiFileClone.RemoveTimedEvents(e => !filter(e));

                yield return midiFileClone;
                yield break;
            }

            foreach (var channel in FourBitNumber.Values.Where(c => channelsUsed[c]))
            {
                var newFile = timedEventsByChannel[channel].ToFile();
                newFile.TimeDivision = midiFile.TimeDivision.Clone();

                yield return newFile;
            }
        }

        /// <summary>
        /// Splits <see cref="MidiFile"/> by notes.
        /// </summary>
        /// <param name="midiFile"><see cref="MidiFile"/> to split.</param>
        /// <param name="settings">Settings accoridng to which notes should be detected and built.</param>
        /// <returns>Collection of <see cref="MidiFile"/> where each file contains events for single note and
        /// other events as defined by <paramref name="settings"/>.</returns>
        /// <exception cref="ArgumentNullException"><paramref name="midiFile"/> is <c>null</c>.</exception>
        public static IEnumerable<MidiFile> SplitByNotes(this MidiFile midiFile, SplitFileByNotesSettings settings = null)
        {
            ThrowIfArgument.IsNull(nameof(midiFile), midiFile);

            settings = settings ?? new SplitFileByNotesSettings();

            return settings.IgnoreChannel
                ? midiFile.SplitByNotes(noteEvent => noteEvent.NoteNumber, settings.Filter, settings.CopyNonNoteEventsToEachFile)
                : midiFile.SplitByNotes(noteEvent => noteEvent.GetNoteId(), settings.Filter, settings.CopyNonNoteEventsToEachFile);
        }

        /// <summary>
        /// Splits <see cref="MidiFile"/> by the specified grid.
        /// </summary>
        /// <remarks>
        /// Non-track chunks will not be copied to any of the new files.
        /// </remarks>
        /// <param name="midiFile"><see cref="MidiFile"/> to split.</param>
        /// <param name="grid">Grid to split <paramref name="midiFile"/> by.</param>
        /// <param name="settings">Settings according to which file should be split.</param>
        /// <returns>Collection of <see cref="MidiFile"/> produced during splitting the input file by grid.</returns>
        /// <exception cref="ArgumentNullException">
        /// <para>One of the following errors occured:</para>
        /// <list type="bullet">
        /// <item>
        /// <description><paramref name="midiFile"/> is <c>null</c>.</description>
        /// </item>
        /// <item>
        /// <description><paramref name="grid"/> is <c>null</c>.</description>
        /// </item>
        /// </list>
        /// </exception>
        public static IEnumerable<MidiFile> SplitByGrid(this MidiFile midiFile, IGrid grid, SliceMidiFileSettings settings = null)
        {
            ThrowIfArgument.IsNull(nameof(midiFile), midiFile);
            ThrowIfArgument.IsNull(nameof(grid), grid);

            if (!midiFile.GetEvents().Any())
                yield break;

            settings = settings ?? new SliceMidiFileSettings();
            midiFile = PrepareMidiFileForSlicing(midiFile, grid, settings);

            var tempoMap = midiFile.GetTempoMap();

            using (var slicer = MidiFileSlicer.CreateFromFile(midiFile))
            {
                foreach (var time in grid.GetTimes(tempoMap))
                {
                    if (time == 0)
                        continue;

                    yield return slicer.GetNextSlice(time, settings);

                    if (slicer.AllEventsProcessed)
                        break;
                }

                if (!slicer.AllEventsProcessed)
                    yield return slicer.GetNextSlice(long.MaxValue, settings);
            }
        }

        /// <summary>
        /// Skips part of the specified length of MIDI file and returns remaining part as
        /// an instance of <see cref="MidiFile"/>.
        /// </summary>
        /// <param name="midiFile"><see cref="MidiFile"/> to skip part of.</param>
        /// <param name="partLength">The length of part to skip.</param>
        /// <param name="settings">Settings according to which <paramref name="midiFile"/>
        /// should be split.</param>
        /// <returns><see cref="MidiFile"/> which is result of skipping a part of the <paramref name="midiFile"/>.</returns>
        /// <exception cref="ArgumentNullException">
        /// <para>One of the following errors occured:</para>
        /// <list type="bullet">
        /// <item>
        /// <description><paramref name="midiFile"/> is <c>null</c>.</description>
        /// </item>
        /// <item>
        /// <description><paramref name="partLength"/> is <c>null</c>.</description>
        /// </item>
        /// </list>
        /// </exception>
        public static MidiFile SkipPart(this MidiFile midiFile, ITimeSpan partLength, SliceMidiFileSettings settings = null)
        {
            ThrowIfArgument.IsNull(nameof(midiFile), midiFile);
            ThrowIfArgument.IsNull(nameof(partLength), partLength);

            var grid = new ArbitraryGrid(partLength);

            settings = settings ?? new SliceMidiFileSettings();
            midiFile = PrepareMidiFileForSlicing(midiFile, grid, settings);

            var tempoMap = midiFile.GetTempoMap();
            var time = grid.GetTimes(tempoMap).First();

            using (var slicer = MidiFileSlicer.CreateFromFile(midiFile))
            {
                slicer.GetNextSlice(time, settings);
                return slicer.GetNextSlice(long.MaxValue, settings);
            }
        }

        /// <summary>
        /// Takes part of the specified length of a MIDI file (starting at the beginning of the file)
        /// and returns it as an instance of <see cref="MidiFile"/>.
        /// </summary>
        /// <param name="midiFile"><see cref="MidiFile"/> to take part of.</param>
        /// <param name="partLength">The length of part to take.</param>
        /// <param name="settings">Settings according to which <paramref name="midiFile"/>
        /// should be split.</param>
        /// <returns><see cref="MidiFile"/> which is part of the <paramref name="midiFile"/>.</returns>
        /// <exception cref="ArgumentNullException">
        /// <para>One of the following errors occured:</para>
        /// <list type="bullet">
        /// <item>
        /// <description><paramref name="midiFile"/> is <c>null</c>.</description>
        /// </item>
        /// <item>
        /// <description><paramref name="partLength"/> is <c>null</c>.</description>
        /// </item>
        /// </list>
        /// </exception>
        public static MidiFile TakePart(this MidiFile midiFile, ITimeSpan partLength, SliceMidiFileSettings settings = null)
        {
            ThrowIfArgument.IsNull(nameof(midiFile), midiFile);
            ThrowIfArgument.IsNull(nameof(partLength), partLength);

            var grid = new ArbitraryGrid(partLength);

            settings = settings ?? new SliceMidiFileSettings();
            midiFile = PrepareMidiFileForSlicing(midiFile, grid, settings);

            var tempoMap = midiFile.GetTempoMap();
            var time = grid.GetTimes(tempoMap).First();

            using (var slicer = MidiFileSlicer.CreateFromFile(midiFile))
            {
                return slicer.GetNextSlice(time, settings);
            }
        }

        /// <summary>
        /// Takes part of the specified length of a MIDI file (starting at the specified time within the file)
        /// and returns it as an instance of <see cref="MidiFile"/>.
        /// </summary>
        /// <param name="midiFile"><see cref="MidiFile"/> to take part of.</param>
        /// <param name="partStart">The start time of part to take.</param>
        /// <param name="partLength">The length of part to take.</param>
        /// <param name="settings">Settings according to which <paramref name="midiFile"/>
        /// should be split.</param>
        /// <returns><see cref="MidiFile"/> which is part of the <paramref name="midiFile"/>.</returns>
        /// <exception cref="ArgumentNullException">
        /// <para>One of the following errors occured:</para>
        /// <list type="bullet">
        /// <item>
        /// <description><paramref name="midiFile"/> is <c>null</c>.</description>
        /// </item>
        /// <item>
        /// <description><paramref name="partStart"/> is <c>null</c>.</description>
        /// </item>
        /// <item>
        /// <description><paramref name="partLength"/> is <c>null</c>.</description>
        /// </item>
        /// </list>
        /// </exception>
        public static MidiFile TakePart(this MidiFile midiFile, ITimeSpan partStart, ITimeSpan partLength, SliceMidiFileSettings settings = null)
        {
            ThrowIfArgument.IsNull(nameof(midiFile), midiFile);
            ThrowIfArgument.IsNull(nameof(partStart), partStart);
            ThrowIfArgument.IsNull(nameof(partLength), partLength);

            var grid = new ArbitraryGrid(partStart, partStart.Add(partLength, TimeSpanMode.TimeLength));

            settings = settings ?? new SliceMidiFileSettings();
            midiFile = PrepareMidiFileForSlicing(midiFile, grid, settings);

            var tempoMap = midiFile.GetTempoMap();
            var times = grid.GetTimes(tempoMap).ToArray();

            using (var slicer = MidiFileSlicer.CreateFromFile(midiFile))
            {
                slicer.GetNextSlice(times[0], settings);
                return slicer.GetNextSlice(times[1], settings);
            }
        }

        private static IEnumerable<MidiFile> SplitByNotes<TNoteId>(
            this MidiFile midiFile,
            Func<NoteEvent, TNoteId> getNoteId,
            Predicate<TimedEvent> filter,
            bool copyNonNoteEventsToEachFile)
        {
            var timedEventsByIds = new Dictionary<TNoteId, List<TimedEvent>>();
            var nonNoteEvents = new List<TimedEvent>();

            var timedEvents = midiFile.GetTrackChunks().GetTimedEventsLazy();
            if (filter != null)
                timedEvents = timedEvents.Where(e => filter(e.Item1));

            foreach (var timedEventTuple in timedEvents)
            {
                var timedEvent = timedEventTuple.Item1;

                var noteEvent = timedEvent.Event as NoteEvent;
                if (noteEvent != null)
                {
                    var noteId = getNoteId(noteEvent);

                    List<TimedEvent> timedEventsById;
                    if (!timedEventsByIds.TryGetValue(noteId, out timedEventsById))
                    {
                        timedEventsByIds.Add(noteId, timedEventsById = new List<TimedEvent>());

                        if (copyNonNoteEventsToEachFile)
                            timedEventsById.AddRange(nonNoteEvents);
                    }

                    timedEventsById.Add(timedEvent);
                }
                else if (copyNonNoteEventsToEachFile)
                {
                    foreach (var timedEventsById in timedEventsByIds)
                    {
                        timedEventsById.Value.Add(timedEvent);
                    }

                    nonNoteEvents.Add(timedEvent);
                }
            }

            if (!timedEventsByIds.Any())
            {
                var midiFileClone = midiFile.Clone();
                if (filter != null)
                    midiFileClone.RemoveTimedEvents(e => !filter(e));
                
                yield return midiFileClone;
                yield break;
            }

            foreach (var timedEventsById in timedEventsByIds)
            {
                var newFile = timedEventsById.Value.ToFile();
                newFile.TimeDivision = midiFile.TimeDivision.Clone();

                yield return newFile;
            }
        }

        private static MidiFile PrepareMidiFileForSlicing(MidiFile midiFile, IGrid grid, SliceMidiFileSettings settings)
        {
            if (settings.SplitNotes)
            {
                midiFile = midiFile.Clone();
                midiFile.SplitNotesByGrid(grid);
            }

            return midiFile;
        }

        #endregion
    }
}
