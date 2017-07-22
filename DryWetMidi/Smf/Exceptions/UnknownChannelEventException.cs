using Melanchall.DryWetMidi.Common;
using System;
using System.Runtime.Serialization;

namespace Melanchall.DryWetMidi.Smf
{
    /// <summary>
    /// The exception that is thrown when the reading engine encountered unknown channel event.
    /// </summary>
    [Serializable]
    public sealed class UnknownChannelEventException : MidiException
    {
        #region Constants

        private const string ChannelSerializationPropertyName = "Channel";
        private const string StatusByteSerializationPropertyName = "StatusByte";

        #endregion

        #region Constructors

        /// <summary>
        /// Initializes a new instance of the <see cref="UnknownChannelEventException"/>.
        /// </summary>
        public UnknownChannelEventException()
            : base("Unknown channel event.")
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="UnknownChannelEventException"/> with the
        /// specified status byte and channel.
        /// </summary>
        /// <param name="statusByte">Status byte of an unknown channel event.</param>
        /// <param name="channel">Channel of an unknown channel event.</param>
        public UnknownChannelEventException(FourBitNumber statusByte, FourBitNumber channel)
            : this()
        {
            StatusByte = statusByte;
            Channel = channel;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="UnknownChannelEventException"/>
        /// with serialized data.
        /// </summary>
        /// <param name="info">The <see cref="SerializationInfo"/> that holds the serialized
        /// object data about the exception being thrown.</param>
        /// <param name="context">The <see cref="StreamingContext"/> that contains contextual information
        /// about the source or destination.</param>
        private UnknownChannelEventException(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
            StatusByte = (FourBitNumber)info.GetByte(StatusByteSerializationPropertyName);
            Channel = (FourBitNumber)info.GetByte(ChannelSerializationPropertyName);
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

        /// <summary>
        /// Sets the <see cref="SerializationInfo"/> with information about the exception.
        /// </summary>
        /// <param name="info">The <see cref="SerializationInfo"/> that holds the serialized
        /// object data about the exception being thrown.</param>
        /// <param name="context">The <see cref="StreamingContext"/> that contains contextual information
        /// about the source or destination.</param>
        public override void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            ThrowIfArgument.IsNull(nameof(info), info);

            info.AddValue(ChannelSerializationPropertyName, Channel);
            info.AddValue(StatusByteSerializationPropertyName, StatusByte);

            base.GetObjectData(info, context);
        }

        #endregion
    }
}
