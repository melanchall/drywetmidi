using Melanchall.DryWetMidi.Common;
using System;
using System.Runtime.Serialization;

namespace Melanchall.DryWetMidi.Core
{
    /// <summary>
    /// The exception that is thrown when the reading engine has encountered an invalid MIDI time
    /// code component (i.e. a value that doesn't belong to values of <see cref="Core.MidiTimeCodeComponent"/>)
    /// during reading <see cref="MidiTimeCodeEvent"/>.
    /// </summary>
    [Serializable]
    public sealed class InvalidMidiTimeCodeComponentException : MidiException
    {
        #region Constructors

        internal InvalidMidiTimeCodeComponentException(byte midiTimeCodeComponent)
            : base($"Invalid MIDI Time Code component ({midiTimeCodeComponent}).")
        {
            MidiTimeCodeComponent = midiTimeCodeComponent;
        }

        private InvalidMidiTimeCodeComponentException(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
            MidiTimeCodeComponent = info.GetByte(nameof(MidiTimeCodeComponent));
        }

        #endregion

        #region Properties

        /// <summary>
        /// Gets the value representing MIDI time code component that caused this exception.
        /// </summary>
        public byte MidiTimeCodeComponent { get; }

        #endregion

        #region Overrides

        public override void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            base.GetObjectData(info, context);

            info.AddValue(nameof(MidiTimeCodeComponent), MidiTimeCodeComponent);
        }

        #endregion
    }
}
