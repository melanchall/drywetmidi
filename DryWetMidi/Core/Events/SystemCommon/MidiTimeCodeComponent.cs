namespace Melanchall.DryWetMidi.Core
{
    /// <summary>
    /// MIDI time code component.
    /// </summary>
    public enum MidiTimeCodeComponent : byte
    {
        /// <summary>
        /// LSB of frames number.
        /// </summary>
        FramesLsb = 0,

        /// <summary>
        /// MSB of frames number.
        /// </summary>
        FramesMsb = 1,

        /// <summary>
        /// LSB of seconds number.
        /// </summary>
        SecondsLsb = 2,

        /// <summary>
        /// MSB of seconds number.
        /// </summary>
        SecondsMsb = 3,

        /// <summary>
        /// LSB of minutes number.
        /// </summary>
        MinutesLsb = 4,

        /// <summary>
        /// MSB of minutes number.
        /// </summary>
        MinutesMsb = 5,

        /// <summary>
        /// LSB of hours number.
        /// </summary>
        HoursLsb = 6,

        /// <summary>
        /// MSB of hours number and time code type.
        /// </summary>
        HoursMsbAndTimeCodeType = 7
    }
}
