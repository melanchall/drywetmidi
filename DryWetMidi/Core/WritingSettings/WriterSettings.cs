using System;
using Melanchall.DryWetMidi.Common;

namespace Melanchall.DryWetMidi.Core
{
    /// <summary>
    /// Settings according to which <see cref="MidiWriter"/> should write MIDI data.
    /// </summary>
    public sealed class WriterSettings
    {
        #region Fields

        private int _bufferSize = 4096;

        #endregion

        #region Properties

        /// <summary>
        /// Gets or sets a value indicating whether <see cref="MidiWriter"/> should use buffer to
        /// write MIDI data or not. The default value is <c>true</c>.
        /// </summary>
        public bool UseBuffering { get; set; } = true;

        /// <summary>
        /// Gets or sets the size of a buffer that will be used by <see cref="MidiWriter"/> in case of
        /// <see cref="UseBuffering"/> set to <c>true</c>.
        /// </summary>
        /// <exception cref="ArgumentOutOfRangeException"><paramref name="value"/> is zero or negative.</exception>
        public int BufferSize
        {
            get { return _bufferSize; }
            set
            {
                ThrowIfArgument.IsNonpositive(nameof(value), value, "Value is zero or negative.");

                _bufferSize = value;
            }
        }

        #endregion
    }
}
