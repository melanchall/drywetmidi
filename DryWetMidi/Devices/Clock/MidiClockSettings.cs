using System;
using Melanchall.DryWetMidi.Common;

namespace Melanchall.DryWetMidi.Devices
{
    /// <summary>
    /// Holds settings for <see cref="MidiClock"/> used by a clock driven object.
    /// </summary>
    public sealed class MidiClockSettings
    {
        #region Fields

        private CreateTickGeneratorCallback _createTickGeneratorCallback = interval => new HighPrecisionTickGenerator(interval);

        #endregion

        #region Properties

        /// <summary>
        /// Gets or sets a callback used to create tick generator for MIDI clock.
        /// </summary>
        /// <exception cref="ArgumentNullException"><paramref name="value"/> is null.</exception>
        public CreateTickGeneratorCallback CreateTickGeneratorCallback
        {
            get { return _createTickGeneratorCallback; }
            set
            {
                ThrowIfArgument.IsNull(nameof(value), value);

                _createTickGeneratorCallback = value;
            }
        }

        #endregion
    }
}
