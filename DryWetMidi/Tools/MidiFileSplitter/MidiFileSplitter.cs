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

        #endregion
    }
}
