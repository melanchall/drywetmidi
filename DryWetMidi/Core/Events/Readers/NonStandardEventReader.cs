namespace Melanchall.DryWetMidi.Core
{
    internal sealed class NonStandardEventReader : IEventReader
    {
        #region IEventReader

        public MidiEvent Read(MidiReader reader, ReadingSettings settings, byte currentStatusByte)
        {
            NonStandardEvent nonStandardEvent = null;

            switch (currentStatusByte)
            {
                case EventStatusBytes.NonStandard.SelectPartGroup:
                    nonStandardEvent = new SelectPartGroupEvent();
                    break;
            }

            nonStandardEvent?.Read(reader, settings, MidiEvent.UnknownContentSize);
            return nonStandardEvent;
        }

        #endregion
    }
}
