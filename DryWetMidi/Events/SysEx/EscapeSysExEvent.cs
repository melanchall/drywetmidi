using System;

namespace Melanchall.DryWetMidi
{
    public sealed class EscapeSysExEvent : SysExEvent
    {
        #region Constructor

        public EscapeSysExEvent()
        {
        }

        public EscapeSysExEvent(byte[] data)
        {
            Data = data;
        }

        #endregion

        #region Methods

        public bool Equals(EscapeSysExEvent escapeSysExEvent)
        {
            return Equals(escapeSysExEvent, true);
        }

        public bool Equals(EscapeSysExEvent escapeSysExEvent, bool respectDeltaTime)
        {
            if (ReferenceEquals(null, escapeSysExEvent))
                return false;

            if (ReferenceEquals(this, escapeSysExEvent))
                return true;

            return base.Equals(escapeSysExEvent, respectDeltaTime);
        }

        #endregion

        #region Overrides

        internal override void ReadContent(MidiReader reader, ReadingSettings settings, int size)
        {
            if (size < 0)
                throw new ArgumentOutOfRangeException(
                    nameof(size),
                    size,
                    "Non-negative size have to be specified in order to read Escape SysEx event.");

            Data = reader.ReadBytes(size);
            Completed = true;
        }

        public override string ToString()
        {
            return "Escape SysEx";
        }

        public override bool Equals(object obj)
        {
            return Equals(obj as EscapeSysExEvent);
        }

        public override int GetHashCode()
        {
            return base.GetHashCode();
        }

        #endregion
    }
}
