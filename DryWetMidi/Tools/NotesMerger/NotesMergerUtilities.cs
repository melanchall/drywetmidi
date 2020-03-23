using System;
using System.Collections.Generic;
using System.Linq;
using Melanchall.DryWetMidi.Common;
using Melanchall.DryWetMidi.Core;
using Melanchall.DryWetMidi.Interaction;

namespace Melanchall.DryWetMidi.Tools
{
    /// <summary>
    /// Provides methods to merge nearby notes.
    /// </summary>
    public static class NotesMergerUtilities
    {
        #region Methods

        /// <summary>
        /// Merges nearby notes in the specified <see cref="TrackChunk"/>.
        /// </summary>
        /// <param name="trackChunk"><see cref="TrackChunk"/> to merge nearby notes in.</param>
        /// <param name="tempoMap">Tempo map used to calculate distances between notes.</param>
        /// <param name="settings">Settings according to which notes should be merged.</param>
        /// <param name="filter">Filter for notes to merge.</param>
        /// <exception cref="ArgumentNullException">
        /// <para>One of the following errors occured:</para>
        /// <list type="bullet">
        /// <item>
        /// <description><paramref name="trackChunk"/> is <c>null</c>.</description>
        /// </item>
        /// <item>
        /// <description><paramref name="tempoMap"/> is <c>null</c>.</description>
        /// </item>
        /// </list>
        /// </exception>
        public static void MergeNotes(this TrackChunk trackChunk,
                                      TempoMap tempoMap,
                                      NotesMergingSettings settings = null,
                                      Predicate<Note> filter = null)
        {
            ThrowIfArgument.IsNull(nameof(trackChunk), trackChunk);
            ThrowIfArgument.IsNull(nameof(tempoMap), tempoMap);

            using (var notesManager = trackChunk.ManageNotes())
            {
                var notes = notesManager.Notes;

                var notesMerger = new NotesMerger();
                var newNotes = notesMerger.Merge(notes.Where(n => filter == null || filter(n)), tempoMap, settings)
                                          .ToList();

                notes.Clear();
                notes.Add(newNotes);
            }
        }

        /// <summary>
        /// Merges nearby notes in the specified collection of <see cref="TrackChunk"/>.
        /// </summary>
        /// <param name="trackChunks">Collection of <see cref="TrackChunk"/> to merge nearby notes in.</param>
        /// <param name="tempoMap">Tempo map used to calculate distances between notes.</param>
        /// <param name="settings">Settings according to which notes should be merged.</param>
        /// <param name="filter">Filter for notes to merge.</param>
        /// <exception cref="ArgumentNullException">
        /// <para>One of the following errors occured:</para>
        /// <list type="bullet">
        /// <item>
        /// <description><paramref name="trackChunks"/> is <c>null</c>.</description>
        /// </item>
        /// <item>
        /// <description><paramref name="tempoMap"/> is <c>null</c>.</description>
        /// </item>
        /// </list>
        /// </exception>
        public static void MergeNotes(this IEnumerable<TrackChunk> trackChunks,
                                      TempoMap tempoMap,
                                      NotesMergingSettings settings = null,
                                      Predicate<Note> filter = null)
        {
            ThrowIfArgument.IsNull(nameof(trackChunks), trackChunks);
            ThrowIfArgument.IsNull(nameof(tempoMap), tempoMap);

            foreach (var trackChunk in trackChunks.Where(c => c != null))
            {
                trackChunk.MergeNotes(tempoMap, settings, filter);
            }
        }

        /// <summary>
        /// Merges nearby notes in the specified <see cref="MidiFile"/>.
        /// </summary>
        /// <param name="midiFile"><see cref="MidiFile"/> to merge nearby notes in.</param>
        /// <param name="settings">Settings according to which notes should be merged.</param>
        /// <param name="filter">Filter for notes to merge.</param>
        /// <exception cref="ArgumentNullException"><paramref name="midiFile"/> is <c>null</c>.</exception>
        public static void MergeNotes(this MidiFile midiFile,
                                      NotesMergingSettings settings = null,
                                      Predicate<Note> filter = null)
        {
            ThrowIfArgument.IsNull(nameof(midiFile), midiFile);

            var tempoMap = midiFile.GetTempoMap();

            midiFile.GetTrackChunks().MergeNotes(tempoMap, settings, filter);
        }

        #endregion
    }
}
