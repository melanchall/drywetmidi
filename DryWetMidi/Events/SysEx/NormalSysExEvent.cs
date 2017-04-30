using System;
using System.Linq;

namespace Melanchall.DryWetMidi
{
    public sealed class NormalSysExEvent : SysExEvent
    {
        #region Constants

        private const byte EndOfEventByte = 0xF7;

        #endregion

        #region Overrides

        internal override void ReadContent(MidiReader reader, ReadingSettings settings, int size)
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

        #endregion
    }
}
