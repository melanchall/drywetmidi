using Melanchall.DryWetMidi.Common;
using System;
using System.Linq;

namespace Melanchall.DryWetMidi.Interaction
{
    internal static class ObjectIdUtilities
    {
        #region Methods

        public static object GetObjectId(this ITimedObject obj)
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
                return new RestId(rest.Key);

            var registeredParameter = obj as RegisteredParameter;
            if (registeredParameter != null)
                return new RegisteredParameterId(registeredParameter.ParameterType);

            throw new NotSupportedException($"Getting of ID for {obj} is not supported.");
        }

        private static NoteId GetNoteId(Note note)
        {
            return new NoteId(note.Channel, note.NoteNumber);
        }

        #endregion
    }
}
