using Melanchall.DryWetMidi.Common;
using System;
using System.Runtime.Serialization;

namespace Melanchall.DryWetMidi.Core
{
    /// <summary>
    /// The exception that is thrown when the reading engine encountered unknown channel event.
    /// </summary>
    [Serializable]
    public sealed class UnknownChannelEventException : MidiException
    {
        #region Constructors

        internal UnknownChannelEventException(FourBitNumber statusByte, FourBitNumber channel)
            : base($"Unknown channel event (status byte is {statusByte} and channel is {channel}).")
        {
            StatusByte = statusByte;
            Channel = channel;
        }

        private UnknownChannelEventException(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
            StatusByte = (FourBitNumber)info.GetValue(nameof(StatusByte), typeof(FourBitNumber));
            Channel = (FourBitNumber)info.GetValue(nameof(Channel), typeof(FourBitNumber));
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

        #region Overrides

        /// <summary>
        /// Sets the <see cref="SerializationInfo"/> with information about the exception.
        /// </summary>
        /// <param name="info">The <see cref="SerializationInfo"/> that holds the serialized object data
        /// about the exception being thrown.</param>
        /// <param name="context">The <see cref="StreamingContext"/> that contains contextual information
        /// about the source or destination.</param>
        /// <exception cref="ArgumentNullException"><paramref name="info"/> is <c>null</c>.</exception>
        public override void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            base.GetObjectData(info, context);

            info.AddValue(nameof(Channel), Channel);
            info.AddValue(nameof(StatusByte), StatusByte);
        }

        #endregion
    }
}
