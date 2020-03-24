namespace Melanchall.DryWetMidi.Core
{
    /// <summary>
    /// Provides methods to write events of specific types.
    /// </summary>
    internal interface IEventWriter
    {
        /// <summary>
        /// Writes an event to the <see cref="MidiWriter"/>'s underlying stream according to specified
        /// <see cref="WritingSettings"/>.
        /// </summary>
        /// <param name="midiEvent">Event to write.</param>
        /// <param name="writer">Writer to write the event with.</param>
        /// <param name="settings">Settings according to which the event must be written.</param>
        /// <param name="writeStatusByte">True if event must write its status byte, <c>false</c> if it must not.</param>
        void Write(MidiEvent midiEvent, MidiWriter writer, WritingSettings settings, bool writeStatusByte);

        /// <summary>
        /// Calculates size of an event as number of bytes required to write it according to specified
        /// <see cref="WritingSettings"/>.
        /// </summary>
        /// <param name="midiEvent">Event to calculate size of.</param>
        /// <param name="settings">Settings according to which the event will be written.</param>
        /// <param name="writeStatusByte">True if event will write its status byte, <c>false</c> if it will not.</param>
        /// <returns>Count of bytes required to write the event.</returns>
        int CalculateSize(MidiEvent midiEvent, WritingSettings settings, bool writeStatusByte);

        /// <summary>
        /// Gets status byte of the passed event.
        /// </summary>
        /// <param name="midiEvent">Event to get status byte of.</param>
        /// <returns>Status byte of the event.</returns>
        byte GetStatusByte(MidiEvent midiEvent);
    }
}
