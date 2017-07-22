using System.Diagnostics;

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
        internal static IEventWriter GetWriter(MidiEvent midiEvent)
        {
            Debug.Assert(midiEvent != null);

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
