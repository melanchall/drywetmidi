using Melanchall.DryWetMidi.Common;

namespace Melanchall.DryWetMidi.Tools
{
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

        public int IoBufferSize
        {
            get { return _bufferSize; }
            set
            {
                ThrowIfArgument.IsNonpositive(nameof(value), value, "Buffer size is non-positive.");

                _bufferSize = value;
            }
        }

        #endregion
    }
}
