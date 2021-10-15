using System;
using Melanchall.DryWetMidi.Core;

namespace Melanchall.DryWetMidi.Multimedia
{
    /// <summary>
    /// Provides data for the <see cref="InputDevice.MidiTimeCodeReceived"/> event.
    /// </summary>
    public sealed class MidiTimeCodeReceivedEventArgs : EventArgs
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

        /// <summary>
        /// Gets format of MIDI time code.
        /// </summary>
        public MidiTimeCodeType Format { get; }

        /// <summary>
        /// Gets the hours component of MIDI time code.
        /// </summary>
        public int Hours { get; }

        /// <summary>
        /// Gets the minutes component of MIDI time code.
        /// </summary>
        public int Minutes { get; }

        /// <summary>
        /// Gets the seconds component of MIDI time code.
        /// </summary>
        public int Seconds { get; }

        /// <summary>
        /// Gets the frames component of MIDI time code.
        /// </summary>
        public int Frames { get; }

        #endregion
    }
}
