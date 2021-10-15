using System;
using Melanchall.DryWetMidi.Common;

namespace Melanchall.DryWetMidi.Multimedia
{
    /// <summary>
    /// Holds settings for <see cref="MidiClock"/> used by a clock driven object.
    /// </summary>
    public sealed class MidiClockSettings
    {
        #region Fields

        private Func<TickGenerator> _createTickGeneratorCallback = () => new HighPrecisionTickGenerator();

        #endregion

        #region Properties

        /// <summary>
        /// Gets or sets a callback used to create tick generator for MIDI clock.
        /// </summary>
        /// <exception cref="ArgumentNullException"><paramref name="value"/> is <c>null</c>.</exception>
        public Func<TickGenerator> CreateTickGeneratorCallback
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
