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
                                                       .Select(e => e.GetId()));
            var timedEventsMap = notesIds.ToDictionary(cn => cn,
                                                       cn => new List<TimedEvent>());

            foreach (var timedEvent in midiFile.GetTimedEvents())
            {
                var noteEvent = timedEvent.Event as NoteEvent;
                if (noteEvent != null)
                {
                    timedEventsMap[noteEvent.GetId()].Add(timedEvent);
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

        #endregion
    }
}
