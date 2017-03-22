using System;
using System.Linq;

namespace Melanchall.DryMidi
{
    public sealed class NormalSysExMessage : SysExMessage
    {
        #region Constants

        private const byte EndOfMessageByte = 0xF7;

        #endregion

        #region Overrides

        public override void ReadContent(MidiReader reader, ReadingSettings settings, int size = -1)
        {
            if (size < 0)
                throw new ArgumentOutOfRangeException(
                    nameof(size),
                    size,
                    "Non-negative size have to be specified in order to read Normal SysEx message.");

            var data = reader.ReadBytes(size);

            Completed = data.Last() == EndOfMessageByte;

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
