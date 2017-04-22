using System;

namespace Melanchall.DryMidi
{
    public sealed class MessageType
    {
        #region Constructor

        public MessageType(Type type, byte statusByte)
        {
            Type = type;
            StatusByte = statusByte;
        }

        #endregion

        #region Properties

        public Type Type { get; }

        public byte StatusByte { get; }

        #endregion
    }
}
