using System;
using System.Collections.Generic;
using System.Linq;
using Melanchall.DryWetMidi.Common;
using Melanchall.DryWetMidi.Smf;
using Melanchall.DryWetMidi.Smf.Interaction;

namespace Melanchall.DryWetMidi.Tools
{
    /// <summary>
    /// Provides methods to split a MIDI file.
    /// </summary>
    public static class MidiFileSplitter
    {
        #region Methods

        /// <summary>
        /// Splits <see cref="MidiFile"/> by channel.
        /// </summary>
        /// <remarks>
        /// Channel events will be separated by channel and copied to corresponding new files. All
        /// meta and system exclusive events will be copied to all the new files. Non-track chunks
        /// will not be copied to any of the new files.
        /// </remarks>
        /// <param name="midiFile"><see cref="MidiFile"/> to split.</param>
        /// <returns>Collection of <see cref="MidiFile"/> where each file contains events for single channel.</returns>
        /// <exception cref="ArgumentNullException"><paramref name="midiFile"/> is null.</exception>
        public static IEnumerable<MidiFile> SplitByChannel(this MidiFile midiFile)
        {
            ThrowIfArgument.IsNull(nameof(midiFile), midiFile);

            var timedEvents = Enumerable.Range(0, FourBitNumber.MaxValue + 1)
                                        .Select(i => new List<TimedEvent>())
                                        .ToArray();

            foreach (var timedEvent in midiFile.GetTimedEvents())
            {
                var channelEvent = timedEvent.Event as ChannelEvent;
                if (channelEvent != null)
                {
                    timedEvents[channelEvent.Channel].Add(timedEvent.Clone());
                    continue;
                }

                foreach (var timedEventsByChannel in timedEvents)
                {
                    timedEventsByChannel.Add(timedEvent.Clone());
                }
            }

            return timedEvents
                .Where(events => events.Select(e => e.Event).OfType<ChannelEvent>().Any())
                .Select(events =>
                {
                    var file = events.ToFile();
                    file.TimeDivision = midiFile.TimeDivision.Clone();
                    return file;
                });
        }

        /// <summary>
        /// Splits <see cref="MidiFile"/> by notes.
        /// </summary>
        /// <remarks>
        /// Note events will be separated by note number and copied to corresponding new files. All other
        /// channel events, meta and system exclusive events will be copied to all the new files. Non-track
        /// chunks will not be copied to any of the new files.
        /// </remarks>
        /// <param name="midiFile"><see cref="MidiFile"/> to split.</param>
        /// <returns>Collection of <see cref="MidiFile"/> where each file contains events for single note number.</returns>
        /// <exception cref="ArgumentNullException"><paramref name="midiFile"/> is null.</exception>
        public static IEnumerable<MidiFile> SplitByNotes(this MidiFile midiFile)
        {
            ThrowIfArgument.IsNull(nameof(midiFile), midiFile);

            var notesIds = new HashSet<NoteId>(midiFile.GetTimedEvents()
                                                       .Select(e => e.Event)
                                                       .OfType<NoteEvent>()
                                                       .Select(e => e.GetNoteId()));
            var timedEventsMap = notesIds.ToDictionary(id => id,
                                                       id => new List<TimedEvent>());

            foreach (var timedEvent in midiFile.GetTimedEvents())
            {
                var noteEvent = timedEvent.Event as NoteEvent;
                if (noteEvent != null)
                {
                    timedEventsMap[noteEvent.GetNoteId()].Add(timedEvent);
                    continue;
                }

                foreach (var timedObjects in timedEventsMap.Values)
                {
                    timedObjects.Add(timedEvent);
                }
            }

            foreach (var timedObjects in timedEventsMap.Values)
            {
                var file = timedObjects.ToFile();
                file.TimeDivision = midiFile.TimeDivision.Clone();
                yield return file;
            }
        }

        /// <summary>
        /// Splits <see cref="MidiFile"/> by the specified grid.
        /// </summary>
        /// <remarks>
        /// Non-track chunks will not be copied to any of the new files.
        /// </remarks>
        /// <param name="midiFile"><see cref="MidiFile"/> to split.</param>
        /// <param name="grid">Grid to split <paramref name="midiFile"/> by.</param>
        /// <param name="settings">Settings according to which file should be splitted.</param>
        /// <returns>Collection of <see cref="MidiFile"/> produced during splitting the input file by grid.</returns>
        /// <exception cref="ArgumentNullException"><paramref name="midiFile"/> is null. -or-
        /// <paramref name="grid"/> is null.</exception>
        public static IEnumerable<MidiFile> SplitByGrid(this MidiFile midiFile, IGrid grid, SplittingMidiFileByGridSettings settings = null)
        {
            ThrowIfArgument.IsNull(nameof(midiFile), midiFile);
            ThrowIfArgument.IsNull(nameof(grid), grid);

            if (!midiFile.GetEvents().Any())
                yield break;

            settings = settings ?? new SplittingMidiFileByGridSettings();

            if (settings.SplitNotes)
            {
                midiFile = midiFile.Clone();
                midiFile.SplitNotesByGrid(grid);
            }

            var startTime = 0L;
            var tempoMap = midiFile.GetTempoMap();

            using (var operation = SplitMidiFileByGridOperation.Create(midiFile))
            {
                foreach (var time in grid.GetTimes(tempoMap))
                {
                    if (time == 0)
                        continue;

                    var timedEvents = operation.GetTimedEvents(startTime, time, settings.PreserveTimes);
                    var trackChunks = timedEvents.Select(e => e.ToTrackChunk())
                                                 .Where(c => settings.PreserveTrackChunks || c.Events.Any())
                                                 .ToList();

                    if (trackChunks.Any() || !settings.RemoveEmptyFiles)
                    {
                        var file = new MidiFile(trackChunks)
                        {
                            TimeDivision = midiFile.TimeDivision.Clone()
                        };

                        yield return file;
                    }

                    if (operation.AllEventsProcessed)
                        break;

                    startTime = time;
                }
            }
        }

        #endregion
    }
}
