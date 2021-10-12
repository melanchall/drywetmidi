using Melanchall.DryWetMidi.Core;

namespace Melanchall.DryWetMidi.Interaction
{
    /// <summary>
    /// Defines a context to search chords within.
    /// </summary>
    /// <remarks>
    /// Please see <see href="xref:a_getting_objects#chordsearchcontext">Getting objects
    /// (section GetChords → Settings → ChordSearchContext)</see> article to learn more.
    /// </remarks>
    /// <seealso cref="ChordDetectionSettings"/>
    /// <seealso cref="ChordsManagingUtilities"/>
    public enum ChordSearchContext
    {
        /// <summary>
        /// A chord should be detected only within single events collection (<see cref="EventsCollection"/>
        /// or <see cref="TrackChunk"/>). It means all MIDI events that make up a chord should be present
        /// in one events collection.
        /// </summary>
        SingleEventsCollection = 0,

        /// <summary>
        /// A chord can be detected within all events collection (<see cref="EventsCollection"/>
        /// or <see cref="TrackChunk"/>) of the source (<see cref="MidiFile"/> for example). It means
        /// MIDI events that make up a chord can be present in different events collections.
        /// </summary>
        AllEventsCollections
    }
}
