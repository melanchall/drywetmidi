namespace Melanchall.DryWetMidi.Smf
{
    public enum MidiTimeCodeComponent : byte
    {
        FramesLsb = 0,
        FramesMsb = 1,
        SecondsLsb = 2,
        SecondsMsb = 3,
        MinutesLsb = 4,
        MinutesMsb = 5,
        HoursLsb = 6,
        HoursMsbAndTimeCodeType = 7
    }
}
