using System;
using System.Collections.Generic;
using System.Linq;
using Melanchall.DryWetMidi.Interaction;

namespace Melanchall.DryWetMidi.Tests.Utilities
{
    internal static class TimedObjectEquality
    {
        #region Nested classes

        private sealed class TimedObjectComparer : IEqualityComparer<ITimedObject>
        {
            #region Fields

            private readonly bool _compareDeltaTimes;

            #endregion

            #region Constructor

            public TimedObjectComparer(bool compareDeltaTimes)
            {
                _compareDeltaTimes = compareDeltaTimes;
            }

            #endregion

            #region IEqualityComparer<Chord>

            public bool Equals(ITimedObject timedObject1, ITimedObject timedObject2)
            {
                if (ReferenceEquals(timedObject1, timedObject2))
                    return true;

                if (ReferenceEquals(null, timedObject1) || ReferenceEquals(null, timedObject2))
                    return false;

                var timedEvent = timedObject1 as TimedEvent;
                if (timedEvent != null)
                    return TimedEventEquality.AreEqual(timedEvent, timedObject2 as TimedEvent, _compareDeltaTimes);

                var note = timedObject1 as Note;
                if (note != null)
                    return NoteEquality.AreEqual(note, timedObject2 as Note);

                var chord = timedObject1 as Chord;
                if (chord != null)
                    return ChordEquality.AreEqual(chord, timedObject2 as Chord);

                var rest = timedObject1 as Rest;
                if (rest != null)
                    return rest.Equals(timedObject2 as Rest);

                throw new NotImplementedException($"Comparing of {timedObject1} and {timedObject2} is not implemented.");
            }

            public int GetHashCode(ITimedObject timedObject)
            {
                return timedObject.GetHashCode();
            }

            #endregion
        }

        #endregion

        #region Methods

        public static bool AreEqual(ITimedObject timedObject1, ITimedObject timedObject2, bool compareDeltaTimes)
        {
            return new TimedObjectComparer(compareDeltaTimes).Equals(timedObject1, timedObject2);
        }

        public static bool AreEqual(IEnumerable<ITimedObject> timedObjects1, IEnumerable<ITimedObject> timedObjects2, bool compareDeltaTimes)
        {
            if (ReferenceEquals(timedObjects1, timedObjects2))
                return true;

            if (ReferenceEquals(null, timedObjects1) || ReferenceEquals(null, timedObjects2))
                return false;

            return timedObjects1.SequenceEqual(timedObjects2, new TimedObjectComparer(compareDeltaTimes));
        }

        #endregion
    }
}
