using Melanchall.DryWetMidi.Smf;

namespace Melanchall.DryWetMidi.Devices
{
    public sealed class MidiTimeCodeReceivedEventArgs
    {
        #region Constructor

        internal MidiTimeCodeReceivedEventArgs(MidiTimeCodeType timeCodeType, int hours, int minutes, int seconds, int frames)
        {
            Format = timeCodeType;
            Hours = hours;
            Minutes = minutes;
            Seconds = seconds;
            Frames = frames;
        }

        #endregion

        #region Properties

        public MidiTimeCodeType Format { get; }

        public int Hours { get; }

        public int Minutes { get; }

        public int Seconds { get; }

        public int Frames { get; }

        #endregion
    }
}
