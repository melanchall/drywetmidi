namespace Melanchall.DryMidi
{
    /// <summary>
    /// Provides methods to write messages of specific types.
    /// </summary>
    public interface IMessageWriter
    {
        /// <summary>
        /// Writes a message to the <see cref="MidiWriter"/>'s underlying stream according to specified
        /// <see cref="WritingSettings"/>.
        /// </summary>
        /// <param name="message">Message to write.</param>
        /// <param name="writer">Writer to write the message with.</param>
        /// <param name="settings">Settings according to which the message must be written.</param>
        /// <param name="writeStatusByte">True if message must write its status byte, false if it must not.</param>
        void Write(Message message, MidiWriter writer, WritingSettings settings, bool writeStatusByte);

        /// <summary>
        /// Calculates size of a message as number of bytes required to write it according to specified
        /// <see cref="WritingSettings"/>.
        /// </summary>
        /// <param name="message">Message to calculate size of.</param>
        /// <param name="settings">Settings according to which the message will be written.</param>
        /// <param name="writeStatusByte">True if message will write its status byte, false if it will not.</param>
        /// <returns>Count of bytes required to write the message.</returns>
        int CalculateSize(Message message, WritingSettings settings, bool writeStatusByte);

        /// <summary>
        /// Gets status byte of the passed message.
        /// </summary>
        /// <param name="message">Message to get status byte of.</param>
        /// <returns>Status byte of the message.</returns>
        byte GetStatusByte(Message message);
    }
}
