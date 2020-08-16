using Melanchall.DryWetMidi.Common;

namespace Melanchall.DryWetMidi.Core
{
    public sealed class WriterSettings
    {
        #region Fields

        private int _bufferSize = 4096;

        #endregion

        #region Properties

        public bool UseBuffering { get; set; } = true;

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
