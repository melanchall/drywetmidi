using System;
using System.Linq;

namespace Melanchall.DryWetMidi
{
    public sealed class NormalSysExEvent : SysExEvent
    {
        #region Constants

        private const byte EndOfEventByte = 0xF7;

        #endregion

        #region Constructor

        public NormalSysExEvent()
        {
        }

        public NormalSysExEvent(byte[] data)
        {
            Data = data;
        }

        #endregion

        #region Methods

        public bool Equals(NormalSysExEvent normalSysExEvent)
        {
            return Equals(normalSysExEvent, true);
        }

        public bool Equals(NormalSysExEvent normalSysExEvent, bool respectDeltaTime)
        {
            if (ReferenceEquals(null, normalSysExEvent))
                return false;

            if (ReferenceEquals(this, normalSysExEvent))
                return true;

            return base.Equals(normalSysExEvent, respectDeltaTime);
        }

        #endregion

        #region Overrides

        internal override void Read(MidiReader reader, ReadingSettings settings, int size)
        {
            if (size < 0)
                throw new ArgumentOutOfRangeException(
                    nameof(size),
                    size,
                    "Non-negative size have to be specified in order to read Normal SysEx event.");

            var data = reader.ReadBytes(size);

            Completed = data.Last() == EndOfEventByte;

            if (Data == null)
            {
                Data = data;
                return;
            }

            //

            var currentData = Data;
            var currentDataLength = currentData.Length;

            Array.Resize(ref currentData, currentData.Length + data.Length);
            Array.Copy(data, 0, currentData, currentDataLength, data.Length);
        }

        public override string ToString()
        {
            return "Normal SysEx";
        }

        public override bool Equals(object obj)
        {
            return Equals(obj as NormalSysExEvent);
        }

        public override int GetHashCode()
        {
            return base.GetHashCode();
        }

        #endregion
    }
}
