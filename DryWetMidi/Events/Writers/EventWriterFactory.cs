using System;

namespace Melanchall.DryWetMidi.Smf
{
    /// <summary>
    /// Provides a way to get <see cref="IEventWriter"/> for an event.
    /// </summary>
    internal static class EventWriterFactory
    {
        #region Fields

        private static readonly IEventWriter _metaEventWriter = new MetaEventWriter();
        private static readonly IEventWriter _channelEventWriter = new ChannelEventWriter();
        private static readonly IEventWriter _sysExEventWriter = new SysExEventWriter();

        #endregion

        #region Methods

        /// <summary>
        /// Gets <see cref="IEventWriter"/> for an event.
        /// </summary>
        /// <param name="midiEvent">Event to get writer for.</param>
        /// <returns>Writer for the event.</returns>
        /// <exception cref="ArgumentNullException"><paramref name="midiEvent"/> is null.</exception>
        internal static IEventWriter GetWriter(MidiEvent midiEvent)
        {
            if (midiEvent == null)
                throw new ArgumentNullException(nameof(midiEvent));

            //

            if (midiEvent is MetaEvent)
                return _metaEventWriter;

            if (midiEvent is ChannelEvent)
                return _channelEventWriter;

            return _sysExEventWriter;
        }

        #endregion
    }
}
