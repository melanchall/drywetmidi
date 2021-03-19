using System;
using System.Collections.Generic;
using System.Linq;
using Melanchall.DryWetMidi.Common;
using Melanchall.DryWetMidi.Core;

namespace Melanchall.DryWetMidi.Interaction
{
    [Obsolete("OBS6")]
    /// <summary>
    /// Provides methods for getting single collection of notes and rests by the specified
    /// collection of notes.
    /// </summary>
    public static class GetNotesAndRestsUtilities
    {
        #region Methods

        [Obsolete("OBS6")]
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

            return notes
                .GetObjects(ObjectType.Note | ObjectType.Rest, new ObjectDetectionSettings
                {
                    RestDetectionSettings = new RestDetectionSettings
                    {
                        RestSeparationPolicy = restSeparationPolicy
                    }
                })
                .OfType<ILengthedObject>();
        }

        [Obsolete("OBS6")]
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

        [Obsolete("OBS6")]
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

        [Obsolete("OBS6")]
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

        #endregion
    }
}
