namespace Melanchall.DryWetMidi.Smf
{
    /// <summary>
    /// Represents a normal system exclusive event.
    /// </summary>
    /// <remarks>
    /// A MIDI event that carries the MIDI system exclusive message, also known as a "MIDI sysex message",
    /// carries information that is specific to the manufacturer of the MIDI device receiving the message.
    /// The action that this message prompts for can be anything.
    /// Note that although the terminal 0xF7 is redundant (strictly speaking, due to the use of a length
    /// parameter) it must be included.
    /// System exclisive events can be splitted into multiple packets. In this case the first packet uses
    /// the 0xF0 status (such event will be read as <see cref="NormalSysExEvent"/>), whereas the second and
    /// subsequent packets use the 0xF7 status (suzh events will be read as <see cref="EscapeSysExEvent"/>).
    /// This use of the 0xF7 status is referred to as a continuation event.
    /// </remarks>
    public sealed class NormalSysExEvent : SysExEvent
    {
        #region Constructor

        /// <summary>
        /// Initializes a new instance of the <see cref="NormalSysExEvent"/>.
        /// </summary>
        public NormalSysExEvent()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="NormalSysExEvent"/> with the
        /// specified data.
        /// </summary>
        /// <param name="data">Data of the sysex event.</param>
        public NormalSysExEvent(byte[] data)
        {
            Data = data;
        }

        #endregion

        #region Overrides

        /// <summary>
        /// Returns a string that represents the current object.
        /// </summary>
        /// <returns>A string that represents the current object.</returns>
        public override string ToString()
        {
            return "Normal SysEx";
        }

        #endregion
    }
}
