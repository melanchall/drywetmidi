using System;
using System.Collections.Generic;
using Melanchall.DryWetMidi.Common;
using Melanchall.DryWetMidi.Smf;
using Melanchall.DryWetMidi.Smf.Interaction;

namespace Melanchall.DryWetMidi.Tools
{
    /// <summary>
    /// Provides methods to quantize notes time.
    /// </summary>
    public static class NotesQuantizerUtilities
    {
        #region Methods

        /// <summary>
        /// Quantizes notes contained in the specified <see cref="TrackChunk"/>.
        /// </summary>
        /// <param name="trackChunk"><see cref="TrackChunk"/> to quantize notes in.</param>
        /// <param name="grid">Grid to quantize objects by.</param>
        /// <param name="tempoMap">Tempo map used to calculate times to quantize by.</param>
        /// <param name="settings">Settings according to which notes should be quantized.</param>
        /// <exception cref="ArgumentNullException"><paramref name="trackChunk"/> is null. -or-
        /// <paramref name="grid"/> is null. -or- <paramref name="tempoMap"/> is null.</exception>
        /// <exception cref="InvalidOperationException">Note is going to be moved beyond zero. -or-
        /// Note's end is going to be moved beyond the note's fixed end.</exception>
        public static void QuantizeNotes(this TrackChunk trackChunk, IGrid grid, TempoMap tempoMap, NotesQuantizingSettings settings = null)
        {
            ThrowIfArgument.IsNull(nameof(trackChunk), trackChunk);
            ThrowIfArgument.IsNull(nameof(grid), grid);
            ThrowIfArgument.IsNull(nameof(tempoMap), tempoMap);

            using (var notesManager = trackChunk.ManageNotes())
            {
                new NotesQuantizer().Quantize(notesManager.Notes, grid, tempoMap, settings);
            }
        }

        /// <summary>
        /// Quantizes notes contained in the specified collection of <see cref="TrackChunk"/>.
        /// </summary>
        /// <param name="trackChunks">Collection of <see cref="TrackChunk"/> to quantize notes in.</param>
        /// <param name="grid">Grid to quantize objects by.</param>
        /// <param name="tempoMap">Tempo map used to calculate times to quantize by.</param>
        /// <param name="settings">Settings according to which notes should be quantized.</param>
        /// <exception cref="ArgumentNullException"><paramref name="trackChunks"/> is null. -or-
        /// <paramref name="grid"/> is null. -or- <paramref name="tempoMap"/> is null.</exception>
        /// <exception cref="InvalidOperationException">Note is going to be moved beyond zero. -or-
        /// Note's end is going to be moved beyond the note's fixed end.</exception>
        public static void QuantizeNotes(this IEnumerable<TrackChunk> trackChunks, IGrid grid, TempoMap tempoMap, NotesQuantizingSettings settings = null)
        {
            ThrowIfArgument.IsNull(nameof(trackChunks), trackChunks);
            ThrowIfArgument.IsNull(nameof(grid), grid);
            ThrowIfArgument.IsNull(nameof(tempoMap), tempoMap);

            foreach (var trackChunk in trackChunks)
            {
                trackChunk.QuantizeNotes(grid, tempoMap, settings);
            }
        }

        /// <summary>
        /// Quantizes notes contained in the specified <see cref="MidiFile"/>.
        /// </summary>
        /// <param name="midiFile"><see cref="MidiFile"/> to quantize notes in.</param>
        /// <param name="grid">Grid to quantize objects by.</param>
        /// <param name="settings">Settings according to which notes should be quantized.</param>
        /// <exception cref="ArgumentNullException"><paramref name="midiFile"/> is null. -or-
        /// <paramref name="grid"/> is null.</exception>
        /// <exception cref="InvalidOperationException">Note is going to be moved beyond zero. -or-
        /// Note's end is going to be moved beyond the note's fixed end.</exception>
        public static void QuantizeNotes(this MidiFile midiFile, IGrid grid, NotesQuantizingSettings settings = null)
        {
            ThrowIfArgument.IsNull(nameof(midiFile), midiFile);
            ThrowIfArgument.IsNull(nameof(grid), grid);

            var tempoMap = midiFile.GetTempoMap();

            midiFile.GetTrackChunks().QuantizeNotes(grid, tempoMap, settings);
        }

        #endregion
    }
}
