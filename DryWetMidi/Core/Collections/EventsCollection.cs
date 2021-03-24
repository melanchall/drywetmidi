using Melanchall.DryWetMidi.Common;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace Melanchall.DryWetMidi.Core
{
    /// <summary>
    /// Collection of <see cref="MidiEvent"/> objects.
    /// </summary>
    public sealed class EventsCollection : ICollection<MidiEvent>
    {
        #region Fields

        internal readonly List<MidiEvent> _events = new List<MidiEvent>();

        #endregion

        #region Constructor

        /// <summary>
        /// Initializes a new instance of the <see cref="EventsCollection"/>.
        /// </summary>
        internal EventsCollection()
        {
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
        /// <exception cref="ArgumentNullException">value is <c>null</c>.</exception>
        public MidiEvent this[int index]
        {
            get
            {
                ThrowIfArgument.IsInvalidIndex(nameof(index), index, _events.Count);

                return _events[index];
            }
            set
            {
                ThrowIfArgument.IsNull(nameof(value), value);
                ThrowIfArgument.IsInvalidIndex(nameof(index), index, _events.Count);

                _events[index] = value;
            }
        }

        /// <summary>
        /// Gets the number of events contained in the collection.
        /// </summary>
        public int Count => _events.Count;

        /// <summary>
        /// Gets a value indicating whether the <see cref="EventsCollection"/> is read-only.
        /// </summary>
        public bool IsReadOnly { get; } = false;

        #endregion

        #region Methods

        /// <summary>
        /// Adds an event to the end of collection.
        /// </summary>
        /// <param name="midiEvent">The event to be added to the end of the collection.</param>
        /// <remarks>
        /// Note that End Of Track events cannot be added into the collection since it may cause inconsistence in a
        /// track chunk structure. End Of Track event will be written to the track chunk automatically on
        /// <see cref="MidiFile.Write(string, bool, MidiFileFormat, WritingSettings)"/>.
        /// </remarks>
        /// <exception cref="ArgumentNullException"><paramref name="midiEvent"/> is <c>null</c>.</exception>
        public void Add(MidiEvent midiEvent)
        {
            ThrowIfArgument.IsNull(nameof(midiEvent), midiEvent);

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
        /// <exception cref="ArgumentNullException"><paramref name="events"/> is <c>null</c>.</exception>
        public void AddRange(IEnumerable<MidiEvent> events)
        {
            ThrowIfArgument.IsNull(nameof(events), events);

            _events.AddRange(events.Where(e => e != null));
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
        /// <exception cref="ArgumentNullException"><paramref name="midiEvent"/> is <c>null</c>.</exception>
        /// <exception cref="ArgumentOutOfRangeException">
        /// <para>One of the following errors occured:</para>
        /// <list type="bullet">
        /// <item>
        /// <description><paramref name="index"/> is less than 0.</description>
        /// </item>
        /// <item>
        /// <description><paramref name="index"/> is greater than <see cref="Count"/>.</description>
        /// </item>
        /// </list>
        /// </exception>
        public void Insert(int index, MidiEvent midiEvent)
        {
            ThrowIfArgument.IsNull(nameof(midiEvent), midiEvent);
            ThrowIfArgument.IsInvalidIndex(nameof(index), index, _events.Count);

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
        /// <exception cref="ArgumentNullException"><paramref name="midiEvents"/> is <c>null</c>.</exception>
        /// <exception cref="ArgumentOutOfRangeException">
        /// <para>One of the following errors occured:</para>
        /// <list type="bullet">
        /// <item>
        /// <description><paramref name="index"/> is less than 0.</description>
        /// </item>
        /// <item>
        /// <description><paramref name="index"/> is greater than <see cref="Count"/>.</description>
        /// </item>
        /// </list>
        /// </exception>
        public void InsertRange(int index, IEnumerable<MidiEvent> midiEvents)
        {
            ThrowIfArgument.IsNull(nameof(midiEvents), midiEvents);
            ThrowIfArgument.IsInvalidIndex(nameof(index), index, _events.Count);

            _events.InsertRange(index, midiEvents);
        }

        /// <summary>
        /// Removes the first occurrence of a specific event from the collection.
        /// </summary>
        /// <param name="midiEvent">The event to remove from the collection. The value cannot be <c>null</c>.</param>
        /// <returns><c>true</c> if event is successfully removed; otherwise, <c>false</c>. This method also returns
        /// <c>false</c> if event was not found in the collection.</returns>
        /// <exception cref="ArgumentNullException"><paramref name="midiEvent"/> is <c>null</c>.</exception>
        public bool Remove(MidiEvent midiEvent)
        {
            ThrowIfArgument.IsNull(nameof(midiEvent), midiEvent);

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
            ThrowIfArgument.IsInvalidIndex(nameof(index), index, _events.Count);

            _events.RemoveAt(index);
        }

        /// <summary>
        /// Removes all the events that match the conditions defined by the specified predicate.
        /// </summary>
        /// <param name="match">The <see cref="Predicate{T}"/> delegate that defines the conditions
        /// of the events to remove.</param>
        /// <returns>The number of events removed from the <see cref="EventsCollection"/>.</returns>
        /// <exception cref="ArgumentNullException"><paramref name="match"/> is <c>null</c>.</exception>
        public int RemoveAll(Predicate<MidiEvent> match)
        {
            ThrowIfArgument.IsNull(nameof(match), match);

            return _events.RemoveAll(match);
        }

        /// <summary>
        /// Searches for the specified event and returns the zero-based index of the first
        /// occurrence within the entire <see cref="EventsCollection"/>.
        /// </summary>
        /// <param name="midiEvent">The event to locate in the <see cref="EventsCollection"/>.</param>
        /// <returns>The zero-based index of the first occurrence of event within the entire
        /// <see cref="EventsCollection"/>, if found; otherwise, –1.</returns>
        /// <exception cref="ArgumentNullException"><paramref name="midiEvent"/> is <c>null</c>.</exception>
        public int IndexOf(MidiEvent midiEvent)
        {
            ThrowIfArgument.IsNull(nameof(midiEvent), midiEvent);

            return _events.IndexOf(midiEvent);
        }

        /// <summary>
        /// Removes all events from the <see cref="EventsCollection"/>.
        /// </summary>
        public void Clear()
        {
            _events.Clear();
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

        #region ICollection<MidiEvent>

        /// <summary>
        /// Determines whether the <see cref="EventsCollection"/> contains a specific event.
        /// </summary>
        /// <param name="item">The event to locate in the <see cref="EventsCollection"/>.</param>
        /// <returns><c>true</c> if event is found in the <see cref="EventsCollection"/>; otherwise, <c>false</c>.</returns>
        public bool Contains(MidiEvent item)
        {
            return _events.Contains(item);
        }

        /// <summary>
        /// Copies the elements of the <see cref="EventsCollection"/> to an <see cref="Array"/>,
        /// starting at a particular index.
        /// </summary>
        /// <param name="array">The one-dimensional <see cref="Array"/> that is the destination of
        /// the elements copied from <see cref="EventsCollection"/>. The array must have zero-based
        /// indexing.</param>
        /// <param name="arrayIndex">The zero-based index in array at which copying begins.</param>
        /// <exception cref="ArgumentNullException"><paramref name="array"/> is <c>null</c>.</exception>
        /// <exception cref="ArgumentOutOfRangeException"><paramref name="arrayIndex"/> is less than 0.</exception>
        /// <exception cref="ArgumentException">The number of elements in the source <see cref="EventsCollection"/>
        /// is greater than the available space from <paramref name="arrayIndex"/> to the end of the destination
        /// array.</exception>
        public void CopyTo(MidiEvent[] array, int arrayIndex)
        {
            _events.CopyTo(array, arrayIndex);
        }

        #endregion
    }
}
