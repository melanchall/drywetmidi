namespace Melanchall.DryWetMidi.Core
{
    /// <summary>
    /// MIDI time code type (frames per second).
    /// </summary>
    public enum MidiTimeCodeType : byte
    {
        /// <summary>
        /// 24 frames per second.
        /// </summary>
        TwentyFour = 0,

        /// <summary>
        /// 25 frames per second.
        /// </summary>
        TwentyFive = 1,

        /// <summary>
        /// 29.97 frames per second (also called "30 drop").
        /// </summary>
        ThirtyDrop = 2,

        /// <summary>
        /// 30 frames per second.
        /// </summary>
        Thirty = 3
    }
}
