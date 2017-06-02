using Melanchall.DryWetMidi.Common;
using System;

namespace Melanchall.DryWetMidi.Smf.Interaction
{
    public sealed class Note
    {
        #region Constructor

        public Note(NoteName noteName, int octave)
            : this(noteName, octave, 0)
        {
        }

        public Note(NoteName noteName, int octave, long length)
            : this(noteName, octave, 0, length)
        {
        }

        public Note(NoteName noteName, int octave, long length, long time)
            : this(NoteUtilities.GetNoteNumber(noteName, octave))
        {
        }

        public Note(SevenBitNumber noteNumber)
            : this(noteNumber, 0)
        {
        }

        public Note(SevenBitNumber noteNumber, long length)
            : this(noteNumber, length, 0)
        {
        }

        public Note(SevenBitNumber noteNumber, long length, long time)
        {
            NoteNumber = noteNumber;
            Length = length;
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

            //

            TimedNoteOnEvent = timedNoteOnEvent;
            TimedNoteOffEvent = timedNoteOffEvent;

            //

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

        public NoteName NoteName => NoteUtilities.GetNoteName(NoteNumber);

        public int Octave => NoteUtilities.GetNoteOctave(NoteNumber);

        internal TimedEvent TimedNoteOnEvent { get; } = new TimedEvent(new NoteOnEvent());

        internal TimedEvent TimedNoteOffEvent { get; } = new TimedEvent(new NoteOffEvent());

        #endregion

        #region Overrides

        public override string ToString()
        {
            return $"{NoteName.ToString().Replace("Sharp", "#")}{Octave} at {Time}";
        }

        #endregion
    }
}
