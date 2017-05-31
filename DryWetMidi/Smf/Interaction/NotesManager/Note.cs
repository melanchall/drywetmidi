using System;

namespace Melanchall.DryWetMidi.Smf.Interaction
{
    public sealed class Note
    {
        #region Fields

        private SevenBitNumber _noteNumber;
        private SevenBitNumber _velocity;
        private FourBitNumber _channel;

        #endregion

        #region Constructor

        public Note(SevenBitNumber noteNumber)
            : this(new TimedEvent(new NoteOnEvent()),
                   new TimedEvent(new NoteOffEvent()))
        {
            NoteNumber = noteNumber;
        }

        public Note(SevenBitNumber noteNumber, long length)
            : this(noteNumber)
        {
            Length = length;
        }

        public Note(SevenBitNumber noteNumber, long length, long time)
            : this(noteNumber, length)
        {
            Time = time;
        }

        internal Note(TimedEvent timedNoteOnEvent, TimedEvent timedNoteOffEvent)
        {
            if (timedNoteOnEvent == null)
                throw new ArgumentNullException(nameof(timedNoteOnEvent));

            var noteOnEvent = timedNoteOnEvent.Event as NoteOnEvent;
            if (noteOnEvent == null)
                throw new ArgumentException("Timed event doesn't wrap Note On event.", nameof(timedNoteOnEvent));

            if (timedNoteOffEvent == null)
                throw new ArgumentNullException(nameof(timedNoteOffEvent));

            if (!(timedNoteOffEvent.Event is NoteOffEvent))
                throw new ArgumentException("Timed event doesn't wrap Note Off event.", nameof(timedNoteOffEvent));

            TimedNoteOnEvent = timedNoteOnEvent;
            TimedNoteOffEvent = timedNoteOffEvent;

            NoteNumber = noteOnEvent.NoteNumber;
            Velocity = noteOnEvent.Velocity;
            Channel = noteOnEvent.Channel;
        }

        #endregion

        #region Properties

        public long Time
        {
            get { return TimedNoteOnEvent.Time; }
            set
            {
                if (value < 0)
                    throw new ArgumentOutOfRangeException(nameof(value), value, "Time is negative.");

                TimedNoteOffEvent.Time = value + Length;
                TimedNoteOnEvent.Time = value;
            }
        }

        public long Length
        {
            get { return TimedNoteOffEvent.Time - TimedNoteOnEvent.Time; }
            set
            {
                if (value < 0)
                    throw new ArgumentOutOfRangeException(nameof(value), value, "Length is negative.");

                TimedNoteOffEvent.Time = TimedNoteOnEvent.Time + value;
            }
        }

        public SevenBitNumber NoteNumber { get; set; }

        public SevenBitNumber Velocity { get; set; }

        public FourBitNumber Channel { get; set; }

        internal TimedEvent TimedNoteOnEvent { get; }

        internal TimedEvent TimedNoteOffEvent { get; }

        #endregion
    }
}
