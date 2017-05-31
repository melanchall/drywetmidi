using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace Melanchall.DryWetMidi.Smf
{
    /// <summary>
    /// Collection of <see cref="MidiEvent"/> objects.
    /// </summary>
    public sealed class EventsCollection : IEnumerable<MidiEvent>
    {
        #region Fields

        private readonly List<MidiEvent> _events = new List<MidiEvent>();

        #endregion

        #region Constructor

        /// <summary>
        /// Initializes a new instance of the <see cref="EventsCollection"/>.
        /// </summary>
        internal EventsCollection()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="EventsCollection"/> with the specified events.
        /// </summary>
        /// <param name="events">Events to add to the events collection.</param>
        /// <remarks>
        /// Note that End Of Track events cannot be added into the collection since it may cause inconsistence in a
        /// track chunk structure. End Of Track event will be written to the track chunk automatically on
        /// <see cref="MidiFile.Write(string, bool, MidiFileFormat, WritingSettings)"/>.
        /// </remarks>
        /// <exception cref="ArgumentNullException"><paramref name="events"/> is null.</exception>
        /// <exception cref="ArgumentException"><paramref name="events"/> contain an instance of <see cref="EndOfTrackEvent"/>; or
        /// <paramref name="events"/> contain null.
        /// </exception>
        private EventsCollection(IEnumerable<MidiEvent> events)
        {
            if (events == null)
                throw new ArgumentNullException(nameof(events));

            if (events.Any(e => e is EndOfTrackEvent))
                throw new ArgumentException("End Of Track cannot be added to events collection.", nameof(events));

            if (events.Any(e => e == null))
                throw new ArgumentException("Null cannot be added to events collection.", nameof(events));

            AddRange(events);
        }

        #endregion

        #region Properties

        /// <summary>
        /// Gets or sets the event at the specified index.
        /// </summary>
        /// <param name="index">The zero-based index of the event to get or set.</param>
        /// <returns>The event at the specified index.</returns>
        /// <exception cref="ArgumentOutOfRangeException"> <paramref name="index"/> is less than 0;
        /// or <paramref name="index"/> is equal to or greater than <see cref="Count"/>.</exception>
        /// <exception cref="ArgumentNullException">value is null</exception>
        public MidiEvent this[int index]
        {
            get
            {
                if (index < 0 || index >= _events.Count)
                    throw new ArgumentOutOfRangeException(nameof(index), index, "Index is out of range.");

                return _events[index];
            }
            set
            {
                if (value == null)
                    throw new ArgumentNullException(nameof(value));

                if (index < 0 || index >= _events.Count)
                    throw new ArgumentOutOfRangeException(nameof(index), index, "Index is out of range.");

                _events[index] = value;
            }
        }

        /// <summary>
        /// Gets the number of events contained in the collection.
        /// </summary>
        public int Count => _events.Count;

        #endregion

        #region Methods

        /// <summary>
        /// Removes all events from the <see cref="EventsCollection"/>.
        /// </summary>
        public void Clear()
        {
            _events.Clear();
        }

        /// <summary>
        /// Adds an event to the end of collection.
        /// </summary>
        /// <param name="midiEvent">The event to be added to the end of the collection.</param>
        /// <remarks>
        /// Note that End Of Track events cannot be added into the collection since it may cause inconsistence in a
        /// track chunk structure. End Of Track event will be written to the track chunk automatically on
        /// <see cref="MidiFile.Write(string, bool, MidiFileFormat, WritingSettings)"/>.
        /// </remarks>
        /// <exception cref="ArgumentNullException"><paramref name="midiEvent"/> is null.</exception>
        /// <exception cref="ArgumentException"><paramref name="midiEvent"/> is an instance of <see cref="EndOfTrackEvent"/>.
        /// </exception>
        public void Add(MidiEvent midiEvent)
        {
            if (midiEvent == null)
                throw new ArgumentNullException(nameof(midiEvent));

            if (midiEvent is EndOfTrackEvent)
                throw new ArgumentException("End Of Track cannot be added to events collection.", nameof(midiEvent));

            _events.Add(midiEvent);
        }

        /// <summary>
        /// Adds events to the end of collection.
        /// </summary>
        /// <param name="events">Events to be added to the end of the collection.</param>
        /// <remarks>
        /// Note that End Of Track events cannot be added into the collection since it may cause inconsistence in a
        /// track chunk structure. End Of Track event will be written to the track chunk automatically on
        /// <see cref="MidiFile.Write(string, bool, MidiFileFormat, WritingSettings)"/>.
        /// </remarks>
        /// <exception cref="ArgumentNullException"><paramref name="events"/> is null.</exception>
        /// <exception cref="ArgumentException"><paramref name="events"/> contain an instance of <see cref="EndOfTrackEvent"/>; or
        /// <paramref name="events"/> contain null.
        /// </exception>
        public void AddRange(IEnumerable<MidiEvent> events)
        {
            if (events == null)
                throw new ArgumentNullException(nameof(events));

            if (events.OfType<EndOfTrackEvent>().Any())
                throw new ArgumentException("End Of Track cannot be added to events collection.", nameof(events));

            if (events.Any(e => e == null))
                throw new ArgumentException("Null cannot be added to events collection.", nameof(events));

            _events.AddRange(events);
        }

        /// <summary>
        /// Inserts an event into the collection at the specified index.
        /// </summary>
        /// <remarks>
        /// Note that End Of Track events cannot be added into the collection since it may cause inconsistence in a
        /// track chunk structure. End Of Track event will be written to the track chunk automatically on
        /// a MIDI file writing.
        /// </remarks>
        /// <param name="index">The zero-based index at which the event should be inserted.</param>
        /// <param name="midiEvent">The event to insert.</param>
        /// <exception cref="ArgumentNullException"><paramref name="midiEvent"/> is null.</exception>
        /// <exception cref="ArgumentException"><paramref name="midiEvent"/> is an instance of <see cref="EndOfTrackEvent"/>.
        /// </exception>
        /// <exception cref="ArgumentOutOfRangeException"><paramref name="index"/> is less than 0. -or-
        /// <paramref name="index"/> is greater than <see cref="Count"/>.</exception>
        public void Insert(int index, MidiEvent midiEvent)
        {
            if (midiEvent == null)
                throw new ArgumentNullException(nameof(midiEvent));

            if (midiEvent is EndOfTrackEvent)
                throw new ArgumentException("End Of Track cannot be inserted to events collection.", nameof(midiEvent));

            if (index < 0 || index >= _events.Count)
                throw new ArgumentOutOfRangeException(nameof(index), index, "Index is out of range.");

            _events.Insert(index, midiEvent);
        }

        /// <summary>
        /// Inserts a set of events into the collection at the specified index.
        /// </summary>
        /// <remarks>
        /// Note that End Of Track events cannot be added into the collection since it may cause inconsistence in a
        /// track chunk structure. End Of Track event will be written to the track chunk automatically on
        /// a MIDI file writing.
        /// </remarks>
        /// <param name="index">The zero-based index at which the events should be inserted.</param>
        /// <param name="midiEvents">The events to insert.</param>
        /// <exception cref="ArgumentNullException"><paramref name="midiEvents"/> is null.</exception>
        /// <exception cref="ArgumentException"><paramref name="midiEvents"/> contains an instance of
        /// <see cref="EndOfTrackEvent"/>.</exception>
        /// <exception cref="ArgumentOutOfRangeException"><paramref name="index"/> is less than 0. -or-
        /// <paramref name="index"/> is greater than <see cref="Count"/>.</exception>
        public void InsertRange(int index, IEnumerable<MidiEvent> midiEvents)
        {
            if (midiEvents == null)
                throw new ArgumentNullException(nameof(midiEvents));

            if (midiEvents.OfType<EndOfTrackEvent>().Any())
                throw new ArgumentException("End Of Track cannot be inserted to events collection.", nameof(midiEvents));

            if (index < 0 || index >= _events.Count)
                throw new ArgumentOutOfRangeException(nameof(index), index, "Index is out of range.");

            _events.InsertRange(index, midiEvents);
        }

        /// <summary>
        /// Removes the first occurrence of a specific event from the collection.
        /// </summary>
        /// <param name="midiEvent">The event to remove from the collection. The value cannot be null.</param>
        /// <returns>true if event is successfully removed; otherwise, false. This method also returns
        /// false if event was not found in the collection.</returns>
        /// <exception cref="ArgumentNullException"><paramref name="midiEvent"/> is null.</exception>
        public bool Remove(MidiEvent midiEvent)
        {
            if (midiEvent == null)
                throw new ArgumentNullException(nameof(midiEvent));

            return _events.Remove(midiEvent);
        }

        /// <summary>
        /// Removes the event at the specified index of the collection.
        /// </summary>
        /// <param name="index">The zero-based index of the event to remove.</param>
        /// <exception cref="ArgumentOutOfRangeException"><paramref name="index"/> is less than 0; or <paramref name="index"/>
        /// is equal to or greater than <see cref="Count"/>.</exception>
        public void RemoveAt(int index)
        {
            if (index < 0 || index >= _events.Count)
                throw new ArgumentOutOfRangeException(nameof(index), index, "Index is out of range.");

            _events.RemoveAt(index);
        }

        /// <summary>
        /// Removes all the events that match the conditions defined by the specified predicate.
        /// </summary>
        /// <param name="match">The <see cref="Predicate{T}"/> delegate that defines the conditions
        /// of the events to remove.</param>
        /// <returns>The number of events removed from the <see cref="EventsCollection"/>.</returns>
        /// <exception cref="ArgumentNullException"><paramref name="match"/> is null.</exception>
        public int RemoveAll(Predicate<MidiEvent> match)
        {
            if (match == null)
                throw new ArgumentNullException(nameof(match));

            return _events.RemoveAll(match);
        }

        /// <summary>
        /// Searches for the specified event and returns the zero-based index of the first
        /// occurrence within the entire <see cref="EventsCollection"/>.
        /// </summary>
        /// <param name="midiEvent">The event to locate in the <see cref="EventsCollection"/>.</param>
        /// <returns>The zero-based index of the first occurrence of event within the entire
        /// <see cref="EventsCollection"/>, if found; otherwise, –1.</returns>
        /// <exception cref="ArgumentNullException"><paramref name="midiEvent"/> is null.</exception>
        public int IndexOf(MidiEvent midiEvent)
        {
            if (midiEvent == null)
                throw new ArgumentNullException(nameof(midiEvent));

            return _events.IndexOf(midiEvent);
        }

        #endregion

        #region IEnumerable<MidiEvent>

        /// <summary>
        /// Returns an enumerator that iterates through the <see cref="EventsCollection"/>.
        /// </summary>
        /// <returns>An enumerator for the <see cref="EventsCollection"/>.</returns>
        public IEnumerator<MidiEvent> GetEnumerator()
        {
            return _events.GetEnumerator();
        }

        /// <summary>
        /// Returns an enumerator that iterates through the <see cref="EventsCollection"/>.
        /// </summary>
        /// <returns>An enumerator for the <see cref="EventsCollection"/>.</returns>
        IEnumerator IEnumerable.GetEnumerator()
        {
            return _events.GetEnumerator();
        }

        #endregion
    }
}
