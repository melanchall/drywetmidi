using Melanchall.DryWetMidi.Common;
using System;
using System.Collections.Generic;
using System.ComponentModel;

namespace Melanchall.DryWetMidi.Smf
{
    /// <summary>
    /// Provider of an implementation of the <see cref="IChunksConverter"/> interface which
    /// is appropriate for a specified MIDI file format.
    /// </summary>
    internal static class ChunksConverterFactory
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

            IChunksConverter converter;
            if (_converters.TryGetValue(format, out converter))
                return converter;

            throw new NotSupportedException($"Converter for the {format} format is not supported.");
        }

        #endregion
    }
}
