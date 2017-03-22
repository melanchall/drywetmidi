using System;
using System.Collections.Generic;
using System.Linq;

namespace Melanchall.DryMidi
{
    public sealed class MultiTrackChunksConverter : IChunksConverter
    {
        #region Constants

        private const int ChannelsCount = 16;

        #endregion

        #region Nested classes

        private sealed class TrackChunkDescriptor
        {
            #region Properties

            public TrackChunk Chunk { get; } = new TrackChunk();

            public int DeltaTime { get; set; }

            #endregion

            #region Methods

            public void AddMessage(Message message)
            {
                message.DeltaTime = DeltaTime;
                Chunk.Messages.Add(message);

                DeltaTime = 0;
            }

            #endregion
        }

        #endregion

        #region IChunksConverter

        public IEnumerable<Chunk> Convert(IEnumerable<Chunk> chunks)
        {
            if (chunks == null)
                throw new ArgumentNullException(nameof(chunks));

            var trackChunks = chunks.OfType<TrackChunk>().ToArray();
            if (trackChunks.Length != 1)
                return chunks;

            var trackChunksDescriptors = Enumerable.Range(0, ChannelsCount + 1)
                                                   .Select(i => new TrackChunkDescriptor())
                                                   .ToArray();
            FourBitNumber? channel = null;

            foreach (var message in trackChunks.First().Messages.Select(m => (Message)m.Clone()))
            {
                Array.ForEach(
                    trackChunksDescriptors,
                    d => d.DeltaTime += message.DeltaTime);

                var channelMessage = message as ChannelMessage;
                if (channelMessage != null)
                {
                    trackChunksDescriptors[channelMessage.Channel + 1].AddMessage(message);
                    channel = null;
                    continue;
                }

                if (!(message is MetaMessage))
                    channel = null;

                var channelPrefixMessage = message as ChannelPrefixMessage;
                if (channelPrefixMessage != null)
                    channel = (FourBitNumber)channelPrefixMessage.Channel;

                if (channel != null)
                {
                    trackChunksDescriptors[channel.Value + 1].AddMessage(message);
                    continue;
                }

                trackChunksDescriptors[0].AddMessage(message);
            }

            return trackChunksDescriptors.Select(d => d.Chunk)
                                         .Where(c => c.Messages.Any())
                                         .Concat(chunks.Where(c => !(c is TrackChunk)));
        }

        #endregion
    }
}
