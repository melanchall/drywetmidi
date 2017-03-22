using System;

namespace Melanchall.DryMidi
{
    public sealed class EscapeSysExMessage : SysExMessage
    {
        #region Overrides

        public override void ReadContent(MidiReader reader, ReadingSettings settings, int size = -1)
        {
            if (size < 0)
                throw new ArgumentOutOfRangeException(
                    nameof(size),
                    size,
                    "Non-negative size have to be specified in order to read Escape SysEx message.");

            Data = reader.ReadBytes(size);
            Completed = true;
        }

        public override string ToString()
        {
            return "Escape SysEx";
        }

        #endregion
    }
}
