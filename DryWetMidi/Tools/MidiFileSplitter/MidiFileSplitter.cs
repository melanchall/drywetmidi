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
    /// <example>
    /// <para>
    /// Methods that split a MIDI file at time(s) also transfer some events that can affect following ones
    /// to a next part. List of such methods:
    /// </para>
    /// <para>
    /// <list type="bullet">
    /// <item><description><see cref="SplitByGrid(MidiFile, IGrid, SliceMidiFileSettings)"/></description></item>
    /// <item><description><see cref="SkipPart(MidiFile, ITimeSpan, SliceMidiFileSettings)"/></description></item>
    /// <item><description><see cref="TakePart(MidiFile, ITimeSpan, ITimeSpan, SliceMidiFileSettings)"/></description></item>
    /// <item><description><see cref="TakePart(MidiFile, ITimeSpan, SliceMidiFileSettings)"/></description></item>
    /// <item><description><see cref="CutPart(MidiFile, ITimeSpan, ITimeSpan, SliceMidiFileSettings)"/></description></item>
    /// </list>
    /// </para>
    /// <para>
    /// And the list of events that will be transferred to a next part:
    /// </para>
    /// <para>
    /// <list type="bullet">
    /// <item><description><see cref="ChannelAftertouchEvent"/></description></item>
    /// <item><description><see cref="ControlChangeEvent"/></description></item>
    /// <item><description><see cref="NoteAftertouchEvent"/></description></item>
    /// <item><description><see cref="PitchBendEvent"/></description></item>
    /// <item><description><see cref="ProgramChangeEvent"/></description></item>
    /// <item><description><see cref="CopyrightNoticeEvent"/></description></item>
    /// <item><description><see cref="InstrumentNameEvent"/></description></item>
    /// <item><description><see cref="ProgramNameEvent"/></description></item>
    /// <item><description><see cref="SequenceTrackNameEvent"/></description></item>
    /// <item><description><see cref="DeviceNameEvent"/></description></item>
    /// <item><description><see cref="PortPrefixEvent"/></description></item>
    /// <item><description><see cref="SetTempoEvent"/></description></item>
    /// <item><description><see cref="ChannelPrefixEvent"/></description></item>
    /// <item><description><see cref="SequenceNumberEvent"/></description></item>
    /// <item><description><see cref="KeySignatureEvent"/></description></item>
    /// <item><description><see cref="SmpteOffsetEvent"/></description></item>
    /// <item><description><see cref="TimeSignatureEvent"/></description></item>
    /// </list>
    /// </para>
    /// <para>
    /// To illustrate how events are transferred, look at the following image showing
    /// a MIDI file that will be split at the time defined by the vertical line:
    /// </para>
    /// <code language="image">
    /// +-------------║-----------+
    /// |┌────────────║──────────┐|
    /// |│  A    +    ║     B    │|
    /// |└────────────║──────────┘|
    /// |┌────────────║──────────┐|
    /// |│   +     C  ║  D       │|
    /// |└────────────║──────────┘|
    /// +-------------║-----------+
    /// </code>
    /// <para>
    /// where <c>A</c>, <c>B</c>, <c>C</c> and <c>D</c> are some MIDI events not from the list above;
    /// <c>+</c> is a MIDI event from the list of events that should be transferred.
    /// </para>
    /// <para>
    /// After split we'll get following two new files:
    /// </para>
    /// <code language="image">
    /// +--------------+
    /// |┌────────────┐|
    /// |│  A    +    │|
    /// |└────────────┘|
    /// |┌────────────┐|
    /// |│   +     C  │|
    /// |└────────────┘|
    /// +--------------+
    ///              +------------+
    ///              |┌──────────┐|
    ///              |+     B    │|
    ///              |└──────────┘|
    ///              |┌──────────┐|
    ///              |+  D       │|
    ///              |└──────────┘|
    ///              +------------+
    /// </code>
    /// <para>
    /// So as you can see <c>+</c> events are put at the start of the second file.
    /// </para>
    /// </example>
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
        /// <example>
        /// <para>
        /// For example, we have a MIDI file with two chunks:
        /// </para>
        /// <para>
        /// <code language="image">
        /// +-------------------+
        /// |┌─────────────────┐|
        /// |│                 │|
        /// |└─────────────────┘|
        /// |┌─────────────────┐|
        /// |│                 │|
        /// |└─────────────────┘|
        /// +-------------------+
        /// </code>
        /// </para>
        /// <para>
        /// So the file will be split into two new files:
        /// </para>
        /// <para>
        /// <code language="image">
        /// +-------------------+
        /// |┌─────────────────┐|
        /// |│                 │|
        /// |└─────────────────┘|
        /// +-------------------+
        /// +-------------------+
        /// |┌─────────────────┐|
        /// |│                 │|
        /// |└─────────────────┘|
        /// +-------------------+
        /// </code>
        /// </para>
        /// <para>
        /// Each new file will have the same time division (<see cref="MidiFile.TimeDivision"/>) as the original one.
        /// </para>
        /// </example>
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
        /// <remarks>
        /// All channel events (<see cref="ChannelEvent"/>) will be grouped by channel and then events for each
        /// channel will be placed to separate files. So each new file will contain channel events for single channel.
        /// If <see cref="SplitFileByChannelSettings.CopyNonChannelEventsToEachFile"/> of <paramref name="settings"/>
        /// set to <c>true</c> (default value), each new file will also contain all non-channel events from the original file.
        /// If an input file doesn't contain channel events, result file will be just a copy of the input one.
        /// </remarks>
        /// <param name="settings">Settings accoridng to which MIDI file should be split.</param>
        /// <param name="midiFile"><see cref="MidiFile"/> to split.</param>
        /// <returns>Collection of <see cref="MidiFile"/> where each file contains events for single channel
        /// and meta and sysex ones as defined by <paramref name="settings"/>.</returns>
        /// <exception cref="ArgumentNullException"><paramref name="midiFile"/> is <c>null</c>.</exception>
        /// <example>
        /// <para>
        /// Given a MIDI file:
        /// </para>
        /// <code language="image">
        /// +-------------------+
        /// |┌─────────────────┐|
        /// |│. ◊ .  ○.◊ .□□  .│|
        /// |└─────────────────┘|
        /// |┌─────────────────┐|
        /// |│ .□. .◊   . ○ ○. │|
        /// |└─────────────────┘|
        /// +-------------------+
        /// </code>
        /// <para>
        /// where <c>◊</c>, <c>○</c> and <c>□</c> mean channel MIDI events on channel 0, 1 and 2 correspondingly;
        /// <c>.</c> is any non-channel event.
        /// </para>
        /// <para>
        /// So the file will be split in following way (<see cref="SplitFileByChannelSettings.CopyNonChannelEventsToEachFile"/>
        /// of <paramref name="settings"/> set to <c>true</c>):
        /// </para>
        /// <code language="image">
        /// +-------------------+
        /// |┌─────────────────┐|
        /// |│..◊...◊ .◊..   ..│|
        /// |└─────────────────┘|
        /// +-------------------+
        /// +-------------------+
        /// |┌─────────────────┐|
        /// |│.. ... ○. ..○ ○..│|
        /// |└─────────────────┘|
        /// +-------------------+
        /// +-------------------+
        /// |┌─────────────────┐|
        /// |│..□...  . ..□□ ..│|
        /// |└─────────────────┘|
        /// +-------------------+
        /// </code>
        /// <para>
        /// So each new file consist of single track chunk containing channel events of one channel and
        /// all non-channel events. New files will have the same time division (<see cref="MidiFile.TimeDivision"/>) as
        /// the original one.
        /// </para>
        /// </example>
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
        /// <remarks>
        /// The method produces new files where each one contains Note On and Note Off events for single
        /// note number and channel (if it's not ignored according to <see cref="SplitFileByNotesSettings.IgnoreChannel"/>
        /// of <paramref name="settings"/>). Also files can contain all non-note events as defined by
        /// <see cref="SplitFileByNotesSettings.CopyNonNoteEventsToEachFile"/> of <paramref name="settings"/>. If
        /// an input file doesn't contain note events, result file will be just a copy of the input one.
        /// </remarks>
        /// <param name="midiFile"><see cref="MidiFile"/> to split.</param>
        /// <param name="settings">Settings accoridng to which notes should be detected and built.</param>
        /// <returns>Collection of <see cref="MidiFile"/> where each file contains events for single note and
        /// other events as defined by <paramref name="settings"/>.</returns>
        /// <exception cref="ArgumentNullException"><paramref name="midiFile"/> is <c>null</c>.</exception>
        /// <example>
        /// <para>
        /// For example, a MIDI file contains following notes:
        /// </para>
        /// <code language="image">
        /// +---------------------+
        /// |┌───────────────────┐|
        /// |│. [ A ]  [     B ]  . .│
        /// |└───────────────────┘|
        /// |┌───────────────────┐|
        /// |│ .  [ B ]  [    A]   . │
        /// |└───────────────────┘|
        /// +---------------------+
        /// </code>
        /// <para>
        /// where <c>A</c> and <c>B</c> mean different notes; <c>.</c> is any non-note event.
        /// </para>
        /// <para>
        /// After split we'll get following two new files:
        /// </para>
        /// <code language="image">
        /// +---------------------+
        /// |┌───────────────────┐|
        /// |│..[ A ]    [    A]  ...│
        /// |└───────────────────┘|
        /// +---------------------+
        /// +---------------------+
        /// |┌───────────────────┐|
        /// |│..  [ B ][     B ]  ...│
        /// |└───────────────────┘|
        /// +---------------------+
        /// </code>
        /// <para>
        /// So each new file contains one track chunk with notes of the same note number and channel, and
        /// also all non-note events. New files will have the same time division (<see cref="MidiFile.TimeDivision"/>)
        /// as the original one.
        /// </para>
        /// </example>
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
        /// <example>
        /// <para>
        /// Given the MIDI file (vertical lines show the grid the file should be split by):
        /// </para>
        /// <code language="image">
        /// +-----║-------║-----------+
        /// |┌────║───────║──────────┐|
        /// |│  A ║  B    ║     C    │|
        /// |└──⁞─║──⁞────║─────⁞────┘|
        /// |┌──⁞─║──⁞────║─────⁞────┐|
        /// |│  ⁞ ║  ⁞ D  ║  E  ⁞    │|
        /// |└──⁞─║──⁞─⁞──║──⁞──⁞────┘|
        /// +---⁞-║--⁞-⁞--║--⁞--⁞-----+
        /// </code>
        /// <para>
        /// where <c>A</c>, <c>B</c>, <c>C</c>, <c>D</c> and <c>E</c> are some MIDI events.
        /// </para>
        /// <para>
        /// We'll get three new files as the result of the split:
        /// </para>
        /// <code language="image">
        /// +---⁞--+ ⁞ ⁞ +---⁞--⁞-----+
        /// |┌──⁞─┐| ⁞ ⁞ |┌──⁞──⁞────┐|
        /// |│  A │| ⁞ ⁞ |│  ⁞  C    │|
        /// |└────┘| ⁞ ⁞ |└──⁞───────┘|
        /// |┌────┐| ⁞ ⁞ |┌──⁞───────┐|
        /// |│    │| ⁞ ⁞ |│  E       │|
        /// |└────┘| ⁞ ⁞ |└──────────┘|
        /// +------+ ⁞ ⁞ +------------+
        ///      +---⁞-⁞---+
        ///      |┌──⁞─⁞──┐|
        ///      |│  B ⁞  │|
        ///      |└────⁞──┘|
        ///      |┌────⁞──┐|
        ///      |│    D  │|
        ///      |└───────┘|
        ///      +---------+
        /// </code>
        /// </example>
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
        /// <example>
        /// <para>
        /// Given the MIDI file (vertical line shows where the file will be split):
        /// </para>
        /// <code language="image">
        ///  │←─── L ────→│
        /// +-------------║-----------+
        /// |┌────────────║──────────┐|
        /// |│  A    B    ║     C    │|
        /// |└────────────║─────⁞────┘|
        /// |┌────────────║─────⁞────┐|
        /// |│         D  ║  E  ⁞    │|
        /// |└────────────║──⁞──⁞────┘|
        /// +-------------║--⁞--⁞-----+
        /// </code>
        /// <para>
        /// where <c>A</c>, <c>B</c>, <c>C</c>, <c>D</c> and <c>E</c> are some MIDI events;
        /// <c>L</c> is <paramref name="partLength"/>.
        /// </para>
        /// <para>
        /// Skipping the part we'll get following file:
        /// </para>
        /// <code language="image">
        ///              +---⁞--⁞-----+
        ///              |┌──⁞──⁞────┐|
        ///              |│  ⁞  C    │|
        ///              |└──⁞───────┘|
        ///              |┌──⁞───────┐|
        ///              |│  E       │|
        ///              |└──────────┘|
        ///              +------------+
        /// </code>
        /// </example>
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
        /// <example>
        /// <para>
        /// Given the MIDI file (vertical line shows where the file will be split):
        /// </para>
        /// <code language="image">
        ///  │←─── L ────→│
        /// +-------------║-----------+
        /// |┌────────────║──────────┐|
        /// |│  A    B    ║     C    │|
        /// |└──⁞────⁞────║──────────┘|
        /// |┌──⁞────⁞────║──────────┐|
        /// |│  ⁞    ⁞ D  ║  E       │|
        /// |└──⁞────⁞─⁞──║──────────┘|
        /// +---⁞----⁞-⁞--║-----------+
        /// </code>
        /// <para>
        /// where <c>A</c>, <c>B</c>, <c>C</c>, <c>D</c> and <c>E</c> are some MIDI events;
        /// <c>L</c> is <paramref name="partLength"/>.
        /// </para>
        /// <para>
        /// Taking the part we'll get following file:
        /// </para>
        /// <code language="image">
        /// +---⁞----⁞-⁞---+
        /// |┌──⁞────⁞─⁞──┐|
        /// |│  A    B ⁞  │|
        /// |└─────────⁞──┘|
        /// |┌─────────⁞──┐|
        /// |│         D  │|
        /// |└────────────┘|
        /// +--------------+
        /// </code>
        /// </example>
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
        /// Takes a part of the specified length of a MIDI file (starting at the specified time within the file)
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
        /// <example>
        /// <para>
        /// Given the MIDI file (vertical lines shows where the file will be split):
        /// </para>
        /// <code language="image">
        ///  │← S →│←─── L ───→│
        /// +------║-----------║-------+
        /// |┌─────║───────────║──────┐|
        /// |│  A  ║  B        ║ C    │|
        /// |└─────║──⁞────────║──────┘|
        /// |┌─────║──⁞────────║──────┐|
        /// |│     ║  ⁞ D    E ║      │|
        /// |└─────║──⁞─⁞────⁞─║──────┘|
        /// +------║--⁞-⁞----⁞-║-------+
        /// </code>
        /// <para>
        /// where <c>A</c>, <c>B</c>, <c>C</c>, <c>D</c> and <c>E</c> are some MIDI events;
        /// <c>S</c> is <paramref name="partStart"/> and <c>L</c> is <paramref name="partLength"/>.
        /// </para>
        /// <para>
        /// Taking the part we'll get following file:
        /// </para>
        /// <code language="image">
        ///       +---⁞-⁞----⁞--+
        ///       |┌──⁞─⁞────⁞─┐|
        ///       |│  B ⁞    ⁞ │|
        ///       |└────⁞────⁞─┘|
        ///       |┌────⁞────⁞─┐|
        ///       |│    D    E │|
        ///       |└───────────┘|
        ///       +-------------+
        /// </code>
        /// </example>
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

        /// <summary>
        /// Cuts a part of the specified length from a MIDI file (starting at the specified time within the file)
        /// and returns a new instance of <see cref="MidiFile"/> which is the original one without the part.
        /// </summary>
        /// <param name="midiFile"><see cref="MidiFile"/> to cut part from.</param>
        /// <param name="partStart">The start time of part to cut.</param>
        /// <param name="partLength">The length of part to cut.</param>
        /// <param name="settings">Settings according to which <paramref name="midiFile"/>
        /// should be split.</param>
        /// <returns><see cref="MidiFile"/> which is the <paramref name="midiFile"/> without a part defined by
        /// <paramref name="partStart"/> and <paramref name="partLength"/>.</returns>
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
        /// <example>
        /// <para>
        /// Given the MIDI file (vertical lines shows where the file will be split):
        /// </para>
        /// <code language="image">
        ///  │← S →│←─── L ───→│
        /// +------║-----------║-------+
        /// |┌─────║───────────║──────┐|
        /// |│  A  ║   B       ║ D    │|
        /// |└─────║───────────║──────┘|
        /// |┌─────║───────────║──────┐|
        /// |│     ║      C    ║    E │|
        /// |└─────║───────────║──────┘|
        /// +------║-----------║-------+
        /// </code>
        /// <para>
        /// where <c>A</c>, <c>B</c>, <c>C</c>, <c>D</c> and <c>E</c> are some MIDI events;
        /// <c>S</c> is <paramref name="partStart"/> and <c>L</c> is <paramref name="partLength"/>.
        /// </para>
        /// <para>
        /// Cutting the part we'll get following file:
        /// </para>
        /// <code language="image">
        /// +------⁞-------+
        /// |┌─────⁞──────┐|
        /// |│  A  ⁞ D    │|
        /// |└─────⁞──────┘|
        /// |┌─────⁞──────┐|
        /// |│     ⁞    E │|
        /// |└─────⁞──────┘|
        /// +------⁞-------+
        /// </code>
        /// <para>
        /// Dashed line shows where the part was cut from the input file.
        /// </para>
        /// </example>
        public static MidiFile CutPart(this MidiFile midiFile, ITimeSpan partStart, ITimeSpan partLength, SliceMidiFileSettings settings = null)
        {
            ThrowIfArgument.IsNull(nameof(midiFile), midiFile);
            ThrowIfArgument.IsNull(nameof(partStart), partStart);
            ThrowIfArgument.IsNull(nameof(partLength), partLength);

            var grid = new ArbitraryGrid(partStart, partStart.Add(partLength, TimeSpanMode.TimeLength));

            var partsStartId = Guid.NewGuid().ToString();
            var partEndId = Guid.NewGuid().ToString();

            settings = settings ?? new SliceMidiFileSettings();
            
            var internalSettings = new SliceMidiFileSettings
            {
                PreserveTrackChunks = true,
                PreserveTimes = settings.PreserveTimes,
                SplitNotes = settings.SplitNotes,
                Markers = new SliceMidiFileMarkers
                {
                    PartStartMarkerEventFactory = () => new MarkerEvent(partsStartId),
                    PartEndMarkerEventFactory = () => new MarkerEvent(partEndId)
                },
                NoteDetectionSettings = settings.NoteDetectionSettings
            };

            var tempoMap = midiFile.GetTempoMap();
            var times = grid.GetTimes(tempoMap).ToArray();

            //

            var notesToSplitDescriptors = settings.SplitNotes
                ? midiFile
                    .GetTrackChunks()
                    .Select(c =>
                    {
                        var notes = c.Events
                            .GetTimedEventsLazy()
                            .GetNotesAndTimedEventsLazy(settings.NoteDetectionSettings)
                            .OfType<Note>();

                        var descriptors = new List<Tuple<NoteId, SevenBitNumber, SevenBitNumber>>();

                        foreach (var note in notes)
                        {
                            if (note.Time + note.Length <= times[0])
                                continue;

                            if (note.Time >= times[1])
                                break;

                            if (note.Time < times[0] && note.Time + note.Length > times[1])
                                descriptors.Add(Tuple.Create(note.GetNoteId(), note.Velocity, note.OffVelocity));
                        }

                        return descriptors;
                    })
                    .ToList()
                : midiFile.GetTrackChunks().Select(c => new List<Tuple<NoteId, SevenBitNumber, SevenBitNumber>>());

            //
            
            midiFile = PrepareMidiFileForSlicing(midiFile, grid, internalSettings);

            var result = new MidiFile
            {
                TimeDivision = midiFile.TimeDivision
            };

            using (var slicer = MidiFileSlicer.CreateFromFile(midiFile))
            {
                var startPart = slicer.GetNextSlice(times[0], internalSettings);
                var mid = slicer.GetNextSlice(times[1], internalSettings);
                var endPart = slicer.GetNextSlice(Math.Max(times.Last(), midiFile.GetDuration<MidiTimeSpan>()) + 1, internalSettings);

                if (internalSettings.PreserveTimes)
                {
                    var partLengthInTicks = times[1] - times[0];
                    endPart.ProcessTimedEvents(e => e.Time -= partLengthInTicks);
                }

                using (var startPartTrackChunksEnumerator = startPart.GetTrackChunks().GetEnumerator())
                using (var endPartTrackChunksEnumerator = endPart.GetTrackChunks().GetEnumerator())
                using (var notesToSplitDescriptorsEnumerator = notesToSplitDescriptors.GetEnumerator())
                {
                    while (startPartTrackChunksEnumerator.MoveNext() &&
                           endPartTrackChunksEnumerator.MoveNext() &&
                           notesToSplitDescriptorsEnumerator.MoveNext())
                    {
                        var newTrackChunk = new TrackChunk();
                        var eventsCollection = newTrackChunk.Events;
                        eventsCollection.AddRange(startPartTrackChunksEnumerator.Current.Events);
                        eventsCollection.AddRange(endPartTrackChunksEnumerator.Current.Events);

                        eventsCollection.RemoveTimedEvents(e =>
                        {
                            var markerEvent = e.Event as MarkerEvent;
                            if (markerEvent == null)
                                return false;

                            return markerEvent.Text == partsStartId || markerEvent.Text == partEndId;
                        });

                        if (settings.SplitNotes && notesToSplitDescriptorsEnumerator.Current.Any())
                        {
                            var timedEvents = eventsCollection
                                .GetTimedEventsLazy(false)
                                .SkipWhile(e => e.Time < times[0])
                                .TakeWhile(e => e.Time == times[0])
                                .ToList();

                            var eventsToRemove = new List<MidiEvent>();

                            foreach (var notesDescriptor in notesToSplitDescriptorsEnumerator.Current)
                            {
                                var timedEventsToRemove = timedEvents
                                    .Where(e =>
                                    {
                                        var noteEvent = e.Event as NoteEvent;
                                        if (noteEvent == null)
                                            return false;

                                        if (!noteEvent.GetNoteId().Equals(notesDescriptor.Item1))
                                            return false;

                                        var noteOnEvent = noteEvent as NoteOnEvent;
                                        if (noteOnEvent != null)
                                            return noteOnEvent.Velocity == notesDescriptor.Item2;

                                        return ((NoteOffEvent) noteEvent).Velocity == notesDescriptor.Item3;
                                    })
                                    .ToArray();

                                foreach (var timedEvent in timedEventsToRemove)
                                {
                                    timedEvents.Remove(timedEvent);
                                    eventsToRemove.Add(timedEvent.Event);
                                }
                            }

                            eventsCollection.RemoveTimedEvents(e => eventsToRemove.Contains(e.Event));
                        }

                        if (!settings.PreserveTrackChunks && !eventsCollection.Any())
                            continue;

                        result.Chunks.Add(newTrackChunk);
                    }
                }
            }

            return result;
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
                midiFile.SplitNotesByGrid(grid, settings.NoteDetectionSettings);
            }

            return midiFile;
        }

        #endregion
    }
}
