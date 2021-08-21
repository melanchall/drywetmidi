namespace Melanchall.DryWetMidi.Core
{
    internal sealed class SysExEventReader : IEventReader
    {
        #region IEventReader

        public MidiEvent Read(MidiReader reader, ReadingSettings settings, byte currentStatusByte)
        {
            var size = reader.ReadVlqNumber();

            //

            SysExEvent sysExEvent = null;

            switch (currentStatusByte)
            {
                case EventStatusBytes.Global.NormalSysEx:
                    sysExEvent = new NormalSysExEvent();
                    break;
                case EventStatusBytes.Global.EscapeSysEx:
                    sysExEvent = new EscapeSysExEvent();
                    break;
            }

            //

            sysExEvent.Read(reader, settings, size);
            return sysExEvent;
        }

        #endregion
    }
}
