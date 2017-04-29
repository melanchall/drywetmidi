using System;

namespace Melanchall.DryMidi
{
    public sealed class EventType
    {
        #region Constructor

        public EventType(Type type, byte statusByte)
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
