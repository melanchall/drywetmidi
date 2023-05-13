using System.Linq;
using Melanchall.DryWetMidi.Common;

namespace Melanchall.DryWetMidi.Core
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
    /// System exclusive events can be split into multiple packets. In this case the first packet uses
    /// the 0xF0 status (such event will be read as <see cref="NormalSysExEvent"/>), whereas the second and
    /// subsequent packets use the 0xF7 status (such events will be read as <see cref="EscapeSysExEvent"/>).
    /// This use of the 0xF7 status is referred to as a continuation event.
    /// </remarks>
    public sealed class NormalSysExEvent : SysExEvent
    {
        #region Constructor

        /// <summary>
        /// Initializes a new instance of the <see cref="NormalSysExEvent"/>.
        /// </summary>
        public NormalSysExEvent()
            : base(MidiEventType.NormalSysEx)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="NormalSysExEvent"/> with the
        /// specified data.
        /// </summary>
        /// <param name="data">Data of the sysex event.</param>
        public NormalSysExEvent(byte[] data)
            : this()
        {
            ThrowIfArgument.StartsWithInvalidValue(
                nameof(data),
                data,
                EventStatusBytes.Global.NormalSysEx,
                $"First data byte mustn't be {EventStatusBytes.Global.NormalSysEx} ({EventStatusBytes.Global.NormalSysEx:X2}) since it will be used automatically.");

            Data = data;
        }

        #endregion

        #region Overrides

        /// <summary>
        /// Clones event by creating a copy of it.
        /// </summary>
        /// <returns>Copy of the event.</returns>
        protected override MidiEvent CloneEvent()
        {
            return new NormalSysExEvent(Data?.ToArray());
        }

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
