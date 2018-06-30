using System.Collections.Generic;
using System.Linq;
using Melanchall.DryWetMidi.Common;
using Melanchall.DryWetMidi.Smf;
using Melanchall.DryWetMidi.Smf.Interaction;

namespace Melanchall.DryWetMidi.Tools
{
    public static class MidiFileSplitter
    {
        #region Methods

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

        public static IEnumerable<MidiFile> SplitByNotes(this MidiFile midiFile)
        {
            ThrowIfArgument.IsNull(nameof(midiFile), midiFile);

            var notesIds = new HashSet<NoteId>(midiFile.GetTimedEvents()
                                                       .Select(e => e.Event)
                                                       .OfType<NoteEvent>()
                                                       .Select(e => e.GetNoteId()));
            var timedEventsMap = notesIds.ToDictionary(cn => cn,
                                                       cn => new List<TimedEvent>());

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
