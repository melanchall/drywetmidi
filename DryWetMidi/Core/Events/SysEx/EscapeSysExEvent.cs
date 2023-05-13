using System.Linq;
using Melanchall.DryWetMidi.Common;

namespace Melanchall.DryWetMidi.Core
{
    /// <summary>
    /// Represents an "escape" system exclusive event which defines an escape sequence.
    /// </summary>
    /// <remarks>
    /// "Escape" system exclusive events start with 0xF7 byte and don't have a terminal 0xF7
    /// byte that is required for normal sysex events.
    /// When an "escape" sysex event is encountered whilst reading a MIDI file, its interpretation
    /// (SysEx packet or escape sequence) is determined as follows:
    /// - When an event with 0xF0 status but lacking a terminal 0xF7 is encountered, then this is the
    ///   first of a Casio-style multi-packet message, and a flag (boolean variable) should be set to
    ///   indicate this.
    /// - If an event with 0xF7 status is encountered whilst this flag is set, then this is a continuation
    ///   event (a system exclusive packet, one of many). If this event has a terminal 0xF7, then it is
    ///   the last packet and flag should be cleared.
    /// - If an event with 0xF7 status is encountered whilst flag is clear, then this event is an escape sequence.
    /// </remarks>
    public sealed class EscapeSysExEvent : SysExEvent
    {
        #region Constructor

        /// <summary>
        /// Initializes a new instance of the <see cref="EscapeSysExEvent"/>.
        /// </summary>
        public EscapeSysExEvent()
            : base(MidiEventType.EscapeSysEx)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="EscapeSysExEvent"/> with the
        /// specified data.
        /// </summary>
        /// <param name="data">Data of the "escape" sysex event.</param>
        public EscapeSysExEvent(byte[] data)
            : this()
        {
            ThrowIfArgument.StartsWithInvalidValue(
                nameof(data),
                data,
                EventStatusBytes.Global.EscapeSysEx,
                $"First data byte mustn't be {EventStatusBytes.Global.EscapeSysEx} ({EventStatusBytes.Global.EscapeSysEx:X2}) since it will be used automatically.");

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
            return new EscapeSysExEvent(Data?.ToArray());
        }

        /// <summary>
        /// Returns a string that represents the current object.
        /// </summary>
        /// <returns>A string that represents the current object.</returns>
        public override string ToString()
        {
            return "Escape SysEx";
        }

        #endregion
    }
}
