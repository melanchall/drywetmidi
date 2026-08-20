using System;

namespace Melanchall.DryWetMidi.Core
{
    internal sealed class SysExEventReader : IEventReader
    {
        #region IEventReader

        public MidiEvent? Read(MidiReader reader, ReadingSettings settings, byte currentStatusByte)
        {
            var size = reader.ReadVlqNumber();

            //

            SysExEvent? sysExEvent = null;

            switch (currentStatusByte)
            {
                case EventStatusBytes.Global.NormalSysEx:
                    sysExEvent = new NormalSysExEvent();
                    break;
                case EventStatusBytes.Global.EscapeSysEx:
                    sysExEvent = new EscapeSysExEvent();
                    break;
                default:
                    // TODO: proper exception
                    throw new InvalidOperationException($"Unexpected status byte: {currentStatusByte}");
            }

            //

            sysExEvent.Read(reader, settings, size);
            return sysExEvent;
        }

        #endregion
    }
}
