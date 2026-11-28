using Melanchall.DryWetMidi.Common;
using Melanchall.DryWetMidi.Core;
using Melanchall.DryWetMidi.Interaction;
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
        /// <param name="settings">Settings according to which MIDI file should be split.</param>
        /// <param name="midiFile"><see cref="MidiFile"/> to split.</param>
        /// <returns>Collection of <see cref="MidiFile"/> where each file contains single chunk from
        /// the original file.</returns>
        /// <exception cref="ArgumentNullException"><paramref name="midiFile"/> is <c>null</c>.</exception>
        /// <exception cref="ArgumentOutOfRangeException"><see cref="MidiFile.TimeDivision"/> of <paramref name="midiFile"/>
        /// is <c>null</c> and <see cref="SplitFileByChunksSettings.PreserveTempoMap"/> of <paramref name="settings"/>
        /// is set to <c>true</c>.</exception>
        public static IEnumerable<MidiFile> SplitByChunks(this MidiFile midiFile, SplitFileByChunksSettings? settings = null)
        {
            ThrowIfArgument.IsNull(nameof(midiFile), midiFile);

            settings = settings ?? new SplitFileByChunksSettings();

            if (settings.PreserveTempoMap)
                ThrowIfArgument.HasNullTimeDivision(nameof(midiFile), midiFile);

            var tempoMap = settings.PreserveTempoMap
                ? midiFile.GetTempoMap()
                : null;

            foreach (var midiChunk in midiFile.Chunks.Where(c => settings.Filter?.Invoke(c) != false))
            {
                var newFile = new MidiFile(midiChunk.Clone())
                {
                    _timeDivision = midiFile.TimeDivision?.Clone()
                };

                if (settings.PreserveTempoMap && tempoMap != null)
                    newFile.ReplaceTempoMap(tempoMap);

                yield return newFile;
            }
        }

        #endregion
    }
}
