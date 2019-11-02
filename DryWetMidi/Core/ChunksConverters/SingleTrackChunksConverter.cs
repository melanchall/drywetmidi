using Melanchall.DryWetMidi.Common;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Melanchall.DryWetMidi.Core
{
    internal sealed class SingleTrackChunksConverter : IChunksConverter
    {
        #region Nested types

        private sealed class EventDescriptor
        {
            #region Constructor

            public EventDescriptor(MidiEvent midiEvent, long absoluteTime, int channel)
            {
                Event = midiEvent;
                AbsoluteTime = absoluteTime;
                Channel = channel;
            }

            #endregion

            #region Properties

            public MidiEvent Event { get; }

            public long AbsoluteTime { get; }

            public int Channel { get; }

            #endregion
        }

        private sealed class EventDescriptorComparer : IComparer<EventDescriptor>
        {
            #region IComparer<EventDescriptor>

            public int Compare(EventDescriptor x, EventDescriptor y)
            {
                var absoluteTimeDifference = x.AbsoluteTime - y.AbsoluteTime;
                if (absoluteTimeDifference != 0)
                    return Math.Sign(absoluteTimeDifference);

                //

                var xMetaEvent = x.Event as MetaEvent;
                var yMetaEvent = y.Event as MetaEvent;
                if (xMetaEvent != null && yMetaEvent == null)
                    return -1;
                else if (xMetaEvent == null && yMetaEvent != null)
                    return 1;
                else if (xMetaEvent == null)
                    return 0;

                //

                var channelDifference = x.Channel - y.Channel;
                if (channelDifference != 0)
                    return channelDifference;

                //

                var xChannelPrefixEvent = x.Event as ChannelPrefixEvent;
                var yChannelPrefixEvent = y.Event as ChannelPrefixEvent;
                if (xChannelPrefixEvent != null && yChannelPrefixEvent == null)
                    return -1;
                else if (xChannelPrefixEvent == null && yChannelPrefixEvent != null)
                    return 1;

                //

                return 0;
            }

            #endregion
        }

        #endregion

        #region IChunksConverter

        public IEnumerable<MidiChunk> Convert(IEnumerable<MidiChunk> chunks)
        {
            ThrowIfArgument.IsNull(nameof(chunks), chunks);

            //

            var trackChunks = chunks.OfType<TrackChunk>().ToArray();
            if (trackChunks.Length == 1)
                return chunks;

            //

            var eventsDescriptors = trackChunks
                .SelectMany(trackChunk =>
                {
                    var absoluteTime = 0L;
                    var channel = -1;
                    return trackChunk.Events
                                     .Select(midiEvent =>
                                     {
                                         var channelPrefixEvent = midiEvent as ChannelPrefixEvent;
                                         if (channelPrefixEvent != null)
                                             channel = channelPrefixEvent.Channel;

                                         if (!(midiEvent is MetaEvent))
                                             channel = -1;

                                         return new EventDescriptor(midiEvent, (absoluteTime += midiEvent.DeltaTime), channel);
                                     });
                })
                .OrderBy(d => d, new EventDescriptorComparer());

            //

            var resultTrackChunk = new TrackChunk();
            var time = 0L;

            foreach (var eventDescriptor in eventsDescriptors)
            {
                MidiEvent midiEvent = eventDescriptor.Event.Clone();

                midiEvent.DeltaTime = eventDescriptor.AbsoluteTime - time;
                resultTrackChunk.Events.Add(midiEvent);

                time = eventDescriptor.AbsoluteTime;
            }

            //

            return new[] { resultTrackChunk }.Concat(chunks.Where(c => !(c is TrackChunk)));
        }

        #endregion
    }
}
