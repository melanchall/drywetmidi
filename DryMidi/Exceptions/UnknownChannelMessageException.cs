using System;
using System.Runtime.Serialization;

namespace Melanchall.DryMidi
{
    [Serializable]
    public sealed class UnknownChannelMessageException : MidiException
    {
        #region Constants

        private const string ChannelSerializationPropertyName = "Channel";
        private const string StatusByteSerializationPropertyName = "StatusByte";

        #endregion

        #region Constructors

        public UnknownChannelMessageException()
            : base("Unknown channel message.")
        {
        }

        private UnknownChannelMessageException(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
            StatusByte = (FourBitNumber)info.GetByte(StatusByteSerializationPropertyName);
            Channel = (FourBitNumber)info.GetByte(ChannelSerializationPropertyName);
        }

        public UnknownChannelMessageException(FourBitNumber statusByte, FourBitNumber channel)
            : this()
        {
            StatusByte = statusByte;
            Channel = channel;
        }

        #endregion

        #region Properties

        public FourBitNumber Channel { get; }

        public FourBitNumber StatusByte { get; }

        #endregion

        #region Overrides

        public override void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            if (info == null)
                throw new ArgumentNullException(nameof(info));

            info.AddValue(ChannelSerializationPropertyName, Channel);
            info.AddValue(StatusByteSerializationPropertyName, StatusByte);

            base.GetObjectData(info, context);
        }

        #endregion
    }
}
