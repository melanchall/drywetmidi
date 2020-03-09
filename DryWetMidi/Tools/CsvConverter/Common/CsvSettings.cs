using System;
using Melanchall.DryWetMidi.Common;

namespace Melanchall.DryWetMidi.Tools
{
    /// <summary>
    /// Common CSV reading/writing settings.
    /// </summary>
    public sealed class CsvSettings
    {
        #region Fields

        private int _bufferSize = 1024;

        #endregion

        #region Properties

        /// <summary>
        /// Gets or sets delimiter used to separate values in CSV representation. The default value is comma.
        /// </summary>
        public char CsvDelimiter { get; set; } = ',';

        /// <summary>
        /// Gets or sets the size of buffer used to read/write CSV data.
        /// </summary>
        /// <exception cref="ArgumentOutOfRangeException">Value is zero or negative.</exception>
        public int IoBufferSize
        {
            get { return _bufferSize; }
            set
            {
                ThrowIfArgument.IsNonpositive(nameof(value), value, "Buffer size is zero or negative.");

                _bufferSize = value;
            }
        }

        #endregion
    }
}
