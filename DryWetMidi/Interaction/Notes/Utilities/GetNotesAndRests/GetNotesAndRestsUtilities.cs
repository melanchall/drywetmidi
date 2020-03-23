using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using Melanchall.DryWetMidi.Common;
using Melanchall.DryWetMidi.Core;

namespace Melanchall.DryWetMidi.Interaction
{
    /// <summary>
    /// Provides methods for getting single collection of notes and rests by the specified
    /// collection of notes.
    /// </summary>
    public static class GetNotesAndRestsUtilities
    {
        #region Constants

        private static readonly object NoSeparationNoteDescriptor = new object();

        #endregion

        #region Methods

        /// <summary>
        /// Iterates through the specified collection of <see cref="Note"/> returning instances of <see cref="Note"/>
        /// and <see cref="Rest"/> where rests calculated using the specified policy.
        /// </summary>
        /// <param name="notes">Collection of <see cref="Note"/> to iterate over.</param>
        /// <param name="restSeparationPolicy">Policy which determines when rests should be returned.</param>
        /// <returns>Collection of <see cref="ITimedObject"/> where an element either <see cref="Note"/>
        /// or <see cref="Rest"/>.</returns>
        /// <exception cref="ArgumentNullException"><paramref name="notes"/> is <c>null</c>.</exception>
        /// <exception cref="InvalidEnumArgumentException"><paramref name="restSeparationPolicy"/> specified an
        /// invalid value.</exception>
        public static IEnumerable<ILengthedObject> GetNotesAndRests(this IEnumerable<Note> notes, RestSeparationPolicy restSeparationPolicy)
        {
            ThrowIfArgument.IsNull(nameof(notes), notes);
            ThrowIfArgument.IsInvalidEnumValue(nameof(restSeparationPolicy), restSeparationPolicy);

            switch (restSeparationPolicy)
            {
                case RestSeparationPolicy.NoSeparation:
                    return GetNotesAndRests(notes,
                                            n => NoSeparationNoteDescriptor,
                                            setRestChannel: false,
                                            setRestNoteNumber: false);
                case RestSeparationPolicy.SeparateByChannel:
                    return GetNotesAndRests(notes,
                                            n => n.Channel,
                                            setRestChannel: true,
                                            setRestNoteNumber: false);
                case RestSeparationPolicy.SeparateByNoteNumber:
                    return GetNotesAndRests(notes,
                                            n => n.NoteNumber,
                                            setRestChannel: false,
                                            setRestNoteNumber: true);
                case RestSeparationPolicy.SeparateByChannelAndNoteNumber:
                    return GetNotesAndRests(notes,
                                            n => n.GetNoteId(),
                                            setRestChannel: true,
                                            setRestNoteNumber: true);
            }

            throw new NotSupportedException($"Rest separation policy {restSeparationPolicy} is not supported.");
        }

        /// <summary>
        /// Iterates through the notes contained in the specified <see cref="TrackChunk"/> returning
        /// instances of <see cref="Note"/> and <see cref="Rest"/> where rests calculated using the specified policy.
        /// </summary>
        /// <param name="trackChunk"><see cref="TrackChunk"/> containing notes to iterate over.</param>
        /// <param name="restSeparationPolicy">Policy which determines when rests should be returned.</param>
        /// <returns>Collection of <see cref="ITimedObject"/> where an element either <see cref="Note"/>
        /// or <see cref="Rest"/>.</returns>
        /// <exception cref="ArgumentNullException"><paramref name="trackChunk"/> is <c>null</c>.</exception>
        /// <exception cref="InvalidEnumArgumentException"><paramref name="restSeparationPolicy"/> specified an
        /// invalid value.</exception>
        public static IEnumerable<ILengthedObject> GetNotesAndRests(this TrackChunk trackChunk, RestSeparationPolicy restSeparationPolicy)
        {
            ThrowIfArgument.IsNull(nameof(trackChunk), trackChunk);
            ThrowIfArgument.IsInvalidEnumValue(nameof(restSeparationPolicy), restSeparationPolicy);

            return trackChunk.GetNotes().GetNotesAndRests(restSeparationPolicy);
        }

        /// <summary>
        /// Iterates through the notes contained in the specified collection of <see cref="TrackChunk"/>
        /// returning instances of <see cref="Note"/> and <see cref="Rest"/> where rests calculated
        /// using the specified policy.
        /// </summary>
        /// <param name="trackChunks">Collection of <see cref="TrackChunk"/> containing notes to iterate over.</param>
        /// <param name="restSeparationPolicy">Policy which determines when rests should be returned.</param>
        /// <returns>Collection of <see cref="ITimedObject"/> where an element either <see cref="Note"/>
        /// or <see cref="Rest"/>.</returns>
        /// <exception cref="ArgumentNullException"><paramref name="trackChunks"/> is <c>null</c>.</exception>
        /// <exception cref="InvalidEnumArgumentException"><paramref name="restSeparationPolicy"/> specified an
        /// invalid value.</exception>
        public static IEnumerable<ILengthedObject> GetNotesAndRests(this IEnumerable<TrackChunk> trackChunks, RestSeparationPolicy restSeparationPolicy)
        {
            ThrowIfArgument.IsNull(nameof(trackChunks), trackChunks);
            ThrowIfArgument.IsInvalidEnumValue(nameof(restSeparationPolicy), restSeparationPolicy);

            return trackChunks.GetNotes().GetNotesAndRests(restSeparationPolicy);
        }

        /// <summary>
        /// Iterates through the collection of notes contained in the specified <see cref="MidiFile"/>
        /// returning instances of <see cref="Note"/> and <see cref="Rest"/> where rests calculated
        /// using the specified policy.
        /// </summary>
        /// <param name="midiFile"><see cref="MidiFile"/> containing notes to iterate over.</param>
        /// <param name="restSeparationPolicy">Policy which determines when rests should be returned.</param>
        /// <returns>Collection of <see cref="ITimedObject"/> where an element either <see cref="Note"/>
        /// or <see cref="Rest"/>.</returns>
        /// <exception cref="ArgumentNullException"><paramref name="midiFile"/> is <c>null</c>.</exception>
        /// <exception cref="InvalidEnumArgumentException"><paramref name="restSeparationPolicy"/> specified an
        /// invalid value.</exception>
        public static IEnumerable<ILengthedObject> GetNotesAndRests(this MidiFile midiFile, RestSeparationPolicy restSeparationPolicy)
        {
            ThrowIfArgument.IsNull(nameof(midiFile), midiFile);
            ThrowIfArgument.IsInvalidEnumValue(nameof(restSeparationPolicy), restSeparationPolicy);

            return midiFile.GetNotes().GetNotesAndRests(restSeparationPolicy);
        }

        private static IEnumerable<ILengthedObject> GetNotesAndRests<TDescriptor>(
            IEnumerable<Note> notes,
            Func<Note, TDescriptor> noteDescriptorGetter,
            bool setRestChannel,
            bool setRestNoteNumber)
        {
            var lastEndTimes = new Dictionary<TDescriptor, long>();

            foreach (var note in notes.Where(n => n != null).OrderBy(n => n.Time))
            {
                var noteDescriptor = noteDescriptorGetter(note);

                long lastEndTime;
                lastEndTimes.TryGetValue(noteDescriptor, out lastEndTime);

                if (note.Time > lastEndTime)
                    yield return new Rest(lastEndTime,
                                          note.Time - lastEndTime,
                                          setRestChannel ? (FourBitNumber?)note.Channel : null,
                                          setRestNoteNumber ? (SevenBitNumber?)note.NoteNumber : null);

                yield return note.Clone();

                lastEndTimes[noteDescriptor] = Math.Max(lastEndTime, note.Time + note.Length);
            }
        }

        #endregion
    }
}
