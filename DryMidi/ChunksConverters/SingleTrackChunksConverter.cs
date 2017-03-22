using System;
using System.Collections.Generic;
using System.Linq;

namespace Melanchall.DryMidi
{
    public sealed class SingleTrackChunksConverter : IChunksConverter
    {
        #region Nested types

        private sealed class MessageDescriptor
        {
            #region Constructor

            public MessageDescriptor(Message message, int absoluteTime, int channel)
            {
                Message = message;
                AbsoluteTime = absoluteTime;
                Channel = channel;
            }

            #endregion

            #region Properties

            public Message Message { get; }

            public int AbsoluteTime { get; }

            public int Channel { get; }

            #endregion
        }

        private sealed class MessageDescriptorComparer : IComparer<MessageDescriptor>
        {
            #region IComparer<MessageDescriptor

            public int Compare(MessageDescriptor x, MessageDescriptor y)
            {
                var absoluteTimeDifference = x.AbsoluteTime - y.AbsoluteTime;
                if (absoluteTimeDifference != 0)
                    return absoluteTimeDifference;

                //

                var xMetaMessage = x.Message as MetaMessage;
                var yMetaMessage = y.Message as MetaMessage;
                if (xMetaMessage != null && yMetaMessage == null)
                    return -1;
                else if (xMetaMessage == null && yMetaMessage != null)
                    return 1;
                else if (xMetaMessage == null && yMetaMessage == null)
                    return 0;

                //

                var channelDifference = x.Channel - y.Channel;
                if (channelDifference != 0)
                    return channelDifference;

                //

                var xChannelPrefixMessage = x.Message as ChannelPrefixMessage;
                var yChannelPrefixMessage = y.Message as ChannelPrefixMessage;
                if (xChannelPrefixMessage != null && yChannelPrefixMessage == null)
                    return -1;
                else if (xChannelPrefixMessage == null && yChannelPrefixMessage != null)
                    return 1;

                //

                return 0;
            }

            #endregion
        }

        #endregion

        #region IChunksConverter

        public IEnumerable<Chunk> Convert(IEnumerable<Chunk> chunks)
        {
            if (chunks == null)
                throw new ArgumentNullException(nameof(chunks));

            //

            var trackChunks = chunks.OfType<TrackChunk>().ToArray();
            if (trackChunks.Length == 1)
                return chunks;

            //

            var messagesDescriptors = trackChunks
                .SelectMany(trackChunk =>
                {
                    var absoluteTime = 0;
                    var channel = -1;
                    return trackChunk.Messages
                                     .Select(message =>
                                     {
                                         var channelPrefixMessage = message as ChannelPrefixMessage;
                                         if (channelPrefixMessage != null)
                                             channel = channelPrefixMessage.Channel;

                                         if (!(message is MetaMessage))
                                             channel = -1;

                                         return new MessageDescriptor(message, (absoluteTime += message.DeltaTime), channel);
                                     });
                })
                .OrderBy(d => d, new MessageDescriptorComparer());

            //

            var resultTrackChunk = new TrackChunk();
            var time = 0;

            foreach (var messageDescriptor in messagesDescriptors)
            {
                var message = (Message)messageDescriptor.Message.Clone();
                message.DeltaTime = messageDescriptor.AbsoluteTime - time;
                resultTrackChunk.Messages.Add(message);

                time = messageDescriptor.AbsoluteTime;
            }

            //

            return new[] { resultTrackChunk }.Concat(chunks.Where(c => !(c is TrackChunk)));
        }

        #endregion
    }
}
