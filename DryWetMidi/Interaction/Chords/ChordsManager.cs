using System;
using Melanchall.DryWetMidi.Core;

namespace Melanchall.DryWetMidi.Interaction
{
    /// <summary>
    /// Provides a way to manage chords of a MIDI file.
    /// </summary>
    [Obsolete("OBS11")]
    public sealed class ChordsManager : TimedObjectsManager<Chord>
    {
        #region Constructor

        /// <summary>
        /// Initializes a new instance of the <see cref="ChordsManager"/> with the specified events
        /// collection, notes tolerance and comparison delegate for events that have same time.
        /// </summary>
        /// <param name="eventsCollection"><see cref="EventsCollection"/> that holds chord events to manage.</param>
        /// <param name="chordDetectionSettings">Settings accoridng to which chords should be detected and built.</param>
        /// <param name="sameTimeEventsComparison">Delegate to compare events with the same absolute time.</param>
        /// <exception cref="ArgumentNullException"><paramref name="eventsCollection"/> is <c>null</c>.</exception>
        public ChordsManager(
            EventsCollection eventsCollection,
            ChordDetectionSettings chordDetectionSettings = null,
            Comparison<MidiEvent> sameTimeEventsComparison = null)
            : base(
                  eventsCollection,
                  ObjectType.Chord,
                  new ObjectDetectionSettings
                  {
                      ChordDetectionSettings = chordDetectionSettings
                  },
                  new TimedObjectsComparerOnSameEventTime(sameTimeEventsComparison))
        {
        }

        #endregion

        #region Properties

        /// <summary>
        /// Gets the <see cref="TimedObjectsCollection{TObject}"/> with all chords managed by the current
        /// <see cref="ChordsManager"/>.
        /// </summary>
        public TimedObjectsCollection<Chord> Chords => Objects;

        #endregion
    }
}
