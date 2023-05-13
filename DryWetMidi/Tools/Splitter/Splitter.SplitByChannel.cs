using Melanchall.DryWetMidi.Common;
using Melanchall.DryWetMidi.Core;
using Melanchall.DryWetMidi.Interaction;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Melanchall.DryWetMidi.Tools
{
    public static partial class Splitter
    {
        #region Methods

        /// <summary>
        /// Splits <see cref="MidiFile"/> by channel. More info in the
        /// <see href="xref:a_file_splitting#splitbychannel">MIDI file splitting: SplitByChannel</see> article.
        /// </summary>
        /// <remarks>
        /// All channel events (<see cref="ChannelEvent"/>) will be grouped by channel and then events for each
        /// channel will be placed to separate files. So each new file will contain channel events for single channel.
        /// If <see cref="SplitFileByChannelSettings.CopyNonChannelEventsToEachFile"/> of <paramref name="settings"/>
        /// set to <c>true</c> (default value), each new file will also contain all non-channel events from the original file.
        /// If an input file doesn't contain channel events, result file will be just a copy of the input one.
        /// </remarks>
        /// <param name="settings">Settings according to which MIDI file should be split.</param>
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

            var timedEvents = midiFile.GetTrackChunks().GetTimedEventsLazy(null);

            var filter = settings.Filter;
            if (filter != null)
                timedEvents = timedEvents.Where(e => filter(e.Object));

            foreach (var timedEventTuple in timedEvents)
            {
                var timedEvent = timedEventTuple.Object;

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

        #endregion
    }
}
