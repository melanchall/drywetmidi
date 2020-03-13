using System;
using System.ComponentModel;
using Melanchall.DryWetMidi.Common;

namespace Melanchall.DryWetMidi.Core
{
    /// <summary>
    /// Provider of an implementation of the <see cref="IChunksConverter"/> interface which
    /// is appropriate for a specified MIDI file format.
    /// </summary>
    internal static class ChunksConverterFactory
    {
        #region Methods

        /// <summary>
        /// Gets chunks converter which is appropriate for a passed MIDI file format.
        /// </summary>
        /// <param name="format">MIDI file format to get <see cref="IChunksConverter"/> for.</param>
        /// <returns>An instance of the <see cref="IChunksConverter"/> appropriate for
        /// <paramref name="format"/>.</returns>
        /// <exception cref="InvalidEnumArgumentException"><paramref name="format"/> specified an invalid value.</exception>
        /// <exception cref="NotSupportedException"><paramref name="format"/> is not supported.</exception>
        public static IChunksConverter GetConverter(MidiFileFormat format)
        {
            ThrowIfArgument.IsInvalidEnumValue(nameof(format), format);

            switch (format)
            {
                case MidiFileFormat.SingleTrack:
                    return new SingleTrackChunksConverter();
                case MidiFileFormat.MultiTrack:
                    return new MultiTrackChunksConverter();
                case MidiFileFormat.MultiSequence:
                    return new MultiSequenceChunksConverter();
            }

            throw new NotSupportedException($"Converter for the {format} format is not supported.");
        }

        #endregion
    }
}
