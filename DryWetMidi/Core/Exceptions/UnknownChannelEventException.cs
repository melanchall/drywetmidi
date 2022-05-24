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
        public FourBitNumber Channel { get; }

        /// <summary>
        /// Gets the status byte of an unknown channel event.
        /// </summary>
        public FourBitNumber StatusByte { get; }

        #endregion

        #region Overrides

        public override void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            base.GetObjectData(info, context);

            info.AddValue(nameof(Channel), Channel);
            info.AddValue(nameof(StatusByte), StatusByte);
        }

        #endregion
    }
}
