using System;

namespace Melanchall.DryMidi
{
    /// <summary>
    /// Provides a way to get <see cref="IMessageWriter"/> for a message.
    /// </summary>
    public static class MessageWriterFactory
    {
        #region Fields

        private static readonly IMessageWriter _metaMessageWriter = new MetaMessageWriter();
        private static readonly IMessageWriter _channelMessageWriter = new ChannelMessageWriter();
        private static readonly IMessageWriter _sysExMessageWriter = new SysExMessageWriter();

        #endregion

        #region Methods

        /// <summary>
        /// Gets <see cref="IMessageWriter"/> for a message.
        /// </summary>
        /// <param name="message">Message to get writer for.</param>
        /// <returns>Writer for the message.</returns>
        /// <exception cref="ArgumentNullException">
        /// <paramref name="message"/> is null.
        /// </exception>
        public static IMessageWriter GetWriter(Message message)
        {
            if (message == null)
                throw new ArgumentNullException(nameof(message));

            //

            if (message is MetaMessage)
                return _metaMessageWriter;

            if (message is ChannelMessage)
                return _channelMessageWriter;

            return _sysExMessageWriter;
        }

        #endregion
    }
}
