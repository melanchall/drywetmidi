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

        public TimedObjectsCollection<Chord> Chords => Objects;

        #endregion
    }
}
