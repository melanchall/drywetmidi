namespace Melanchall.DryWetMidi.Core
{
    internal sealed class SystemCommonEventReader : IEventReader
    {
        #region IEventReader

        public MidiEvent Read(MidiReader reader, ReadingSettings settings, byte currentStatusByte)
        {
            SystemCommonEvent systemCommonEvent = null;

            switch (currentStatusByte)
            {
                case EventStatusBytes.SystemCommon.MtcQuarterFrame:
                    systemCommonEvent = new MidiTimeCodeEvent();
                    break;
                case EventStatusBytes.SystemCommon.SongSelect:
                    systemCommonEvent = new SongSelectEvent();
                    break;
                case EventStatusBytes.SystemCommon.SongPositionPointer:
                    systemCommonEvent = new SongPositionPointerEvent();
                    break;
                case EventStatusBytes.SystemCommon.TuneRequest:
                    systemCommonEvent = new TuneRequestEvent();
                    break;
            }

            systemCommonEvent?.Read(reader, settings, MidiEvent.UnknownContentSize);
            return systemCommonEvent;
        }

        #endregion
    }
}
