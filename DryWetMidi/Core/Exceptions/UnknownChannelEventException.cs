using Melanchall.DryWetMidi.Common;

namespace Melanchall.DryWetMidi.Core
{
    /// <summary>
    /// The exception that is thrown when the reading engine encountered unknown channel event.
    /// </summary>
    public sealed class UnknownChannelEventException : MidiException
    {
        #region Constructors

        internal UnknownChannelEventException(FourBitNumber statusByte, FourBitNumber channel)
            : base($"Unknown channel event (status byte is {statusByte} and channel is {channel}).")
        {
            StatusByte = statusByte;
            Channel = channel;
        }

        #endregion

        #region Properties

        /// <summary>
        /// Gets the channel of an unknown channel event.
        /// </summary>
        /// <remarks>
        /// Channel is a zero-based number in DryWetMIDI, valid values are from <c>0</c> to <c>15</c>.
        /// Other libraries and software can use one-based channel numbers (i.e.from <c>1</c>
        /// to <c>16</c>) so be aware about that: channel <c>10</c> in such software will be <c>9</c>
        /// in DryWetMIDI.
        /// </remarks>
        public FourBitNumber Channel { get; }

        /// <summary>
        /// Gets the status byte of an unknown channel event.
        /// </summary>
        public FourBitNumber StatusByte { get; }

        #endregion
    }
}
