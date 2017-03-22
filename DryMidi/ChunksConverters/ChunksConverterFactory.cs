using System.Collections.Generic;

namespace Melanchall.DryMidi
{
    public static class ChunksConverterFactory
    {
        #region Fields

        private static readonly Dictionary<MidiFileFormat, IChunksConverter> _converters = new Dictionary<MidiFileFormat, IChunksConverter>
        {
            [MidiFileFormat.SingleTrack] = new SingleTrackChunksConverter(),
            [MidiFileFormat.MultiTrack] = new MultiTrackChunksConverter(),
            [MidiFileFormat.MultiSequence] = new MultiSequenceChunksConverter()
        };

        #endregion

        #region Methods

        public static IChunksConverter GetConverter(MidiFileFormat format)
        {
            IChunksConverter converter;
            return _converters.TryGetValue(format, out converter)
                ? converter
                : null;
        }

        #endregion
    }
}
