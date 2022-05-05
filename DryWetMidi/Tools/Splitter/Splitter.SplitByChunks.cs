using Melanchall.DryWetMidi.Common;
using Melanchall.DryWetMidi.Core;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Melanchall.DryWetMidi.Tools
{
    public static partial class Splitter
    {
        #region Methods

        /// <summary>
        /// Splits <see cref="MidiFile"/> by chunks within it.
        /// </summary>
        /// <param name="settings">Settings accoridng to which MIDI file should be split.</param>
        /// <param name="midiFile"><see cref="MidiFile"/> to split.</param>
        /// <returns>Collection of <see cref="MidiFile"/> where each file contains single chunk from
        /// the original file.</returns>
        /// <exception cref="ArgumentNullException"><paramref name="midiFile"/> is <c>null</c>.</exception>
        public static IEnumerable<MidiFile> SplitByChunks(this MidiFile midiFile, SplitFileByChunksSettings settings = null)
        {
            ThrowIfArgument.IsNull(nameof(midiFile), midiFile);

            settings = settings ?? new SplitFileByChunksSettings();

            foreach (var midiChunk in midiFile.Chunks.Where(c => settings.Filter?.Invoke(c) != false))
            {
                yield return new MidiFile(midiChunk.Clone())
                {
                    TimeDivision = midiFile.TimeDivision.Clone()
                };
            }
        }

        #endregion
    }
}
