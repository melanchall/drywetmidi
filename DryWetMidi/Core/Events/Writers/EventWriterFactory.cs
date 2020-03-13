using System.Diagnostics;

namespace Melanchall.DryWetMidi.Core
{
    /// <summary>
    /// Provides a way to get <see cref="IEventWriter"/> for an event.
    /// </summary>
    internal static class EventWriterFactory
    {
        #region Fields

        private static readonly IEventWriter MetaEventWriter = new MetaEventWriter();
        private static readonly IEventWriter ChannelEventWriter = new ChannelEventWriter();
        private static readonly IEventWriter SysExEventWriter = new SysExEventWriter();
        private static readonly IEventWriter SystemRealTimeEventWriter = new SystemRealTimeEventWriter();
        private static readonly IEventWriter SystemCommonEventWriter = new SystemCommonEventWriter();

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
                return MetaEventWriter;

            if (midiEvent is ChannelEvent)
                return ChannelEventWriter;

            if (midiEvent is SystemRealTimeEvent)
                return SystemRealTimeEventWriter;

            if (midiEvent is SystemCommonEvent)
                return SystemCommonEventWriter;

            return SysExEventWriter;
        }

        #endregion
    }
}
