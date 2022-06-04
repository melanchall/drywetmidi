using Melanchall.DryWetMidi.Common;
using System;
using System.Linq;

namespace Melanchall.DryWetMidi.Interaction
{
    /// <summary>
    /// Provides methods to get the ID (key) of an object.
    /// </summary>
    public static class ObjectIdUtilities
    {
        #region Methods

        /// <summary>
        /// Gets the ID (key) of the specified object using standard logic.
        /// </summary>
        /// <param name="obj">Object to get ID for.</param>
        /// <returns>An object that represents the ID of the <paramref name="obj"/>.</returns>
        /// <exception cref="NotSupportedException">Getting of ID for <paramref name="obj"/> is not supported.</exception>
        public static IObjectId GetObjectId(this ITimedObject obj)
        {
            ThrowIfArgument.IsNull(nameof(obj), obj);

            var note = obj as Note;
            if (note != null)
                return GetNoteId(note);

            var timedEvent = obj as TimedEvent;
            if (timedEvent != null)
                return new TimedEventId(timedEvent.Event.EventType);

            var chord = obj as Chord;
            if (chord != null)
                return new ChordId(chord.Notes.Select(GetNoteId).ToArray());

            var rest = obj as Rest;
            if (rest != null)
                return new RestId(rest.Channel, rest.NoteNumber);

            var registeredParameter = obj as RegisteredParameter;
            if (registeredParameter != null)
                return new RegisteredParameterId(registeredParameter.ParameterType);

            throw new NotSupportedException($"Getting of ID for {obj} is not supported.");
        }

        /// <summary>
        /// Gets the ID (key) as the specified value.
        /// </summary>
        /// <param name="id">ID to wrap and return.</param>
        /// <returns>An object that holds <paramref name="id"/>.</returns>
        public static IObjectId GetObjectId<TId>(TId id)
        {
            return new ConstantObjectId<TId>(id);
        }

        private static NoteId GetNoteId(Note note)
        {
            return new NoteId(note.Channel, note.NoteNumber);
        }

        #endregion
    }
}
