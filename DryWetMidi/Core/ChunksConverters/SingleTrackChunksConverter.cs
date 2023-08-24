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

            public EventDescriptor(MidiEvent midiEvent, long absoluteTime)
            {
                Event = midiEvent;
                AbsoluteTime = absoluteTime;
            }

            #endregion

            #region Properties

            public MidiEvent Event { get; }

            public long AbsoluteTime { get; }

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

                return 0;
            }

            #endregion
        }

        #endregion

        #region IChunksConverter

        public IEnumerable<MidiChunk> Convert(IEnumerable<MidiChunk> chunks)
        {
            var trackChunks = chunks.OfType<TrackChunk>().ToArray();
            if (trackChunks.Length < 2)
                return chunks;

            //

            var eventsDescriptors = trackChunks
                .SelectMany(trackChunk =>
                {
                    var absoluteTime = 0L;
                    return trackChunk
                        .Events
                        .Select(midiEvent => new EventDescriptor(midiEvent, (absoluteTime += midiEvent.DeltaTime)));
                })
                .OrderBy(d => d, new EventDescriptorComparer());

            //

            var resultTrackChunk = new TrackChunk();
            var time = 0L;

            foreach (var eventDescriptor in eventsDescriptors)
            {
                var midiEvent = eventDescriptor.Event.Clone();

                midiEvent.DeltaTime = eventDescriptor.AbsoluteTime - time;
                resultTrackChunk.Events.Add(midiEvent);

                time = eventDescriptor.AbsoluteTime;
            }

            //

            return (resultTrackChunk.Events.Any() ? new[] { resultTrackChunk } : Enumerable.Empty<MidiChunk>()).Concat(chunks.Where(c => !(c is TrackChunk)));
        }

        #endregion
    }
}
