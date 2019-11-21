using Melanchall.DryWetMidi.Common;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Melanchall.DryWetMidi.Core
{
    internal sealed class MultiTrackChunksConverter : IChunksConverter
    {
        #region Constants

        private const int ChannelsCount = 16;

        #endregion

        #region Nested classes

        private sealed class TrackChunkDescriptor
        {
            #region Properties

            public TrackChunk Chunk { get; } = new TrackChunk();

            public long DeltaTime { get; set; }

            #endregion

            #region Methods

            public void AddEvent(MidiEvent midiEvent)
            {
                midiEvent.DeltaTime = DeltaTime;
                Chunk.Events.Add(midiEvent);

                DeltaTime = 0;
            }

            #endregion
        }

        #endregion

        #region IChunksConverter

        public IEnumerable<MidiChunk> Convert(IEnumerable<MidiChunk> chunks)
        {
            ThrowIfArgument.IsNull(nameof(chunks), chunks);

            var trackChunks = chunks.OfType<TrackChunk>().ToArray();
            if (trackChunks.Length != 1)
                return chunks;

            var trackChunksDescriptors = Enumerable.Range(0, ChannelsCount + 1)
                                                   .Select(i => new TrackChunkDescriptor())
                                                   .ToArray();
            FourBitNumber? channel = null;

            foreach (var midiEvent in trackChunks.First().Events.Select(m => m.Clone()))
            {
                Array.ForEach(trackChunksDescriptors,
                              d => d.DeltaTime += midiEvent.DeltaTime);

                var channelEvent = midiEvent as ChannelEvent;
                if (channelEvent != null)
                {
                    trackChunksDescriptors[channelEvent.Channel + 1].AddEvent(midiEvent.Clone());
                    channel = null;
                    continue;
                }

                if (!(midiEvent is MetaEvent))
                    channel = null;

                var channelPrefixEvent = midiEvent as ChannelPrefixEvent;
                if (channelPrefixEvent != null)
                    channel = (FourBitNumber)channelPrefixEvent.Channel;

                if (channel != null)
                {
                    trackChunksDescriptors[channel.Value + 1].AddEvent(midiEvent);
                    continue;
                }

                trackChunksDescriptors[0].AddEvent(midiEvent);
            }

            return trackChunksDescriptors.Select(d => d.Chunk)
                                         .Where(c => c.Events.Any())
                                         .Concat(chunks.Where(c => !(c is TrackChunk)));
        }

        #endregion
    }
}
