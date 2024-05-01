using System;
using Melanchall.DryWetMidi.Common;

namespace Melanchall.DryWetMidi.Core
{
    /// <summary>
    /// Represents a MIDI file event stored in a track chunk.
    /// </summary>
    /// <remarks>
    /// <para>
    /// See <see href="https://midi.org/standard-midi-files-specification"/> for detailed MIDI file specification.
    /// </para>
    /// </remarks>
    public abstract class MidiEvent
    {
        #region Constants

        /// <summary>
        /// Constant for content's size of events that don't have size information stored.
        /// </summary>
        public const int UnknownContentSize = -1;

        #endregion

        #region Fields

        internal long _deltaTime;

        #endregion

        #region Constructor

        /// <summary>
        /// Initializes a new instance of the <see cref="MidiEvent"/> with the specified event type.
        /// </summary>
        /// <param name="eventType">The type of event.</param>
        protected MidiEvent(MidiEventType eventType)
        {
            EventType = eventType;
        }

        #endregion

        #region Properties

        /// <summary>
        /// Gets the type of the event.
        /// </summary>
        public MidiEventType EventType { get; }

        /// <summary>
        /// Gets or sets delta-time of the event.
        /// </summary>
        /// <remarks>
        /// Delta-time represents the amount of time before the following event. If the first event in a track
        /// occurs at the very beginning of a track, or if two events occur simultaneously, a delta-time of zero is used.
        /// Delta-time is in some fraction of a beat (or a second, for recording a track with SMPTE times), as specified
        /// by the file's time division.
        /// </remarks>
        /// <exception cref="ArgumentOutOfRangeException">Delta-time is negative.</exception>
        public long DeltaTime
        {
            get { return _deltaTime; }
            set
            {
                ThrowIfArgument.IsNegative(nameof(value), value, "Delta-time is negative.");

                _deltaTime = value;
            }
        }

        internal bool Flag { get; set; }

        #endregion

        #region Methods

        /// <summary>
        /// Reads content of a MIDI event.
        /// </summary>
        /// <param name="reader">Reader to read the content with.</param>
        /// <param name="settings">Settings according to which the event's content must be read.</param>
        /// <param name="size">Size of the event's content.</param>
        internal abstract void Read(MidiReader reader, ReadingSettings settings, int size);

        /// <summary>
        /// Writes content of a MIDI event.
        /// </summary>
        /// <param name="writer">Writer to write the content with.</param>
        /// <param name="settings">Settings according to which the event's content must be written.</param>
        internal abstract void Write(MidiWriter writer, WritingSettings settings);

        /// <summary>
        /// Gets the size of the content of a MIDI event.
        /// </summary>
        /// <param name="settings">Settings according to which the event's content must be written.</param>
        /// <returns>Size of the event's content.</returns>
        internal abstract int GetSize(WritingSettings settings);

        /// <summary>
        /// Clones event by creating a copy of it.
        /// </summary>
        /// <returns>Copy of the event.</returns>
        protected abstract MidiEvent CloneEvent();

        /// <summary>
        /// Clones event by creating a copy of it.
        /// </summary>
        /// <returns>Copy of the event.</returns>
        public MidiEvent Clone()
        {
            var midiEvent = CloneEvent();
            midiEvent._deltaTime = _deltaTime;
            return midiEvent;
        }

        /// <summary>
        /// Determines whether two specified <see cref="MidiEvent"/> objects have the same content.
        /// </summary>
        /// <param name="midiEvent1">The first event to compare, or <c>null</c>.</param>
        /// <param name="midiEvent2">The second event to compare, or <c>null</c>.</param>
        /// <returns><c>true</c> if the <paramref name="midiEvent1"/> is equal to the <paramref name="midiEvent2"/>;
        /// otherwise, <c>false</c>.</returns>
        public static bool Equals(MidiEvent midiEvent1, MidiEvent midiEvent2)
        {
            string message;
            return Equals(midiEvent1, midiEvent2, out message);
        }

        /// <summary>
        /// Determines whether two specified <see cref="MidiEvent"/> objects have the same content.
        /// </summary>
        /// <param name="midiEvent1">The first event to compare, or <c>null</c>.</param>
        /// <param name="midiEvent2">The second event to compare, or <c>null</c>.</param>
        /// <param name="message">Message containing information about what exactly is different in
        /// <paramref name="midiEvent1"/> and <paramref name="midiEvent2"/>.</param>
        /// <returns><c>true</c> if the <paramref name="midiEvent1"/> is equal to the <paramref name="midiEvent2"/>;
        /// otherwise, <c>false</c>.</returns>
        public static bool Equals(MidiEvent midiEvent1, MidiEvent midiEvent2, out string message)
        {
            return Equals(midiEvent1, midiEvent2, null, out message);
        }

        /// <summary>
        /// Determines whether two specified <see cref="MidiEvent"/> objects have the same content.
        /// </summary>
        /// <param name="midiEvent1">The first event to compare, or <c>null</c>.</param>
        /// <param name="midiEvent2">The second event to compare, or <c>null</c>.</param>
        /// <param name="settings">Settings according to which events should be compared.</param>
        /// <returns><c>true</c> if the <paramref name="midiEvent1"/> is equal to the <paramref name="midiEvent2"/>;
        /// otherwise, <c>false</c>.</returns>
        public static bool Equals(MidiEvent midiEvent1, MidiEvent midiEvent2, MidiEventEqualityCheckSettings settings)
        {
            string message;
            return Equals(midiEvent1, midiEvent2, settings, out message);
        }

        /// <summary>
        /// Determines whether two specified <see cref="MidiEvent"/> objects have the same content using
        /// the specified comparison settings.
        /// </summary>
        /// <param name="midiEvent1">The first event to compare, or <c>null</c>.</param>
        /// <param name="midiEvent2">The second event to compare, or <c>null</c>.</param>
        /// <param name="settings">Settings according to which events should be compared.</param>
        /// <param name="message">Message containing information about what exactly is different in
        /// <paramref name="midiEvent1"/> and <paramref name="midiEvent2"/>.</param>
        /// <returns><c>true</c> if the <paramref name="midiEvent1"/> is equal to the <paramref name="midiEvent2"/>;
        /// otherwise, <c>false</c>.</returns>
        public static bool Equals(MidiEvent midiEvent1, MidiEvent midiEvent2, MidiEventEqualityCheckSettings settings, out string message)
        {
            return MidiEventEquality.Equals(midiEvent1, midiEvent2, settings ?? new MidiEventEqualityCheckSettings(), out message);
        }

        #endregion
    }
}
