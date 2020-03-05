using Melanchall.DryWetMidi.Common;

namespace Melanchall.DryWetMidi.Core
{
    public sealed class ReaderSettings
    {
        #region Fields

        private int _nonSeekableStreamBufferSize = 1024;
        private int _nonSeekableStreamIncrementalBytesReadingThreshold = 16384;
        private int _nonSeekableStreamIncrementalBytesReadingStep = 2048;

        #endregion

        #region Constructor

        internal ReaderSettings()
        {
        }

        #endregion

        #region Properties

        public int NonSeekableStreamBufferSize
        {
            get { return _nonSeekableStreamBufferSize; }
            set
            {
                ThrowIfArgument.IsNonpositive(nameof(value), value, "Value is zero or negative.");

                _nonSeekableStreamBufferSize = value;
            }
        }

        public int NonSeekableStreamIncrementalBytesReadingThreshold
        {
            get { return _nonSeekableStreamIncrementalBytesReadingThreshold; }
            set
            {
                ThrowIfArgument.IsNegative(nameof(value), value, "Value is negative.");

                _nonSeekableStreamIncrementalBytesReadingThreshold = value;
            }
        }

        public int NonSeekableStreamIncrementalBytesReadingStep
        {
            get { return _nonSeekableStreamIncrementalBytesReadingStep; }
            set
            {
                ThrowIfArgument.IsNonpositive(nameof(value), value, "Value is zero or negative.");

                _nonSeekableStreamIncrementalBytesReadingStep = value;
            }
        }

        public bool PutDataInMemoryBeforeReading { get; set; }

        #endregion
    }
}
