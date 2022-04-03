using System;
using Melanchall.DryWetMidi.Common;

namespace Melanchall.DryWetMidi.Interaction
{
    /// <summary>
    /// Represents a musical rest.
    /// </summary>
    public sealed class Rest : ILengthedObject, INotifyTimeChanged, INotifyLengthChanged
    {
        #region Events

        /// <summary>
        /// Occurs when the time of an object has been changed.
        /// </summary>
        public event EventHandler<TimeChangedEventArgs> TimeChanged;

        /// <summary>
        /// Occurs when the length of an object has been changed.
        /// </summary>
        public event EventHandler<LengthChangedEventArgs> LengthChanged;

        #endregion

        #region Fields

        private long _time;
        private long _length;

        #endregion

        #region Constructor

        internal Rest(long time, long length, FourBitNumber? channel, SevenBitNumber? noteNumber)
        {
            _time = time;
            _length = length;

            Channel = channel;
            NoteNumber = noteNumber;
        }

        #endregion

        #region Properties

        /// <summary>
        /// Gets start time of an object.
        /// </summary>
        public long Time
        {
            get { return _time; }
            set
            {
                ThrowIfTimeArgument.IsNegative(nameof(value), value);

                var oldTime = Time;
                if (value == oldTime)
                    return;

                _time = value;
                TimeChanged?.Invoke(this, new TimeChangedEventArgs(oldTime, value));
            }
        }

        /// <summary>
        /// Gets length of an object.
        /// </summary>
        public long Length
        {
            get { return _length; }
            set
            {
                ThrowIfLengthArgument.IsNegative(nameof(value), value);

                var oldLength = Length;
                if (value == oldLength)
                    return;

                _length = value;
                LengthChanged?.Invoke(this, new LengthChangedEventArgs(oldLength, value));
            }
        }

        /// <summary>
        /// Gets a channel the rest was constructed for.
        /// </summary>
        public FourBitNumber? Channel { get; }

        /// <summary>
        /// Gets a note number the rest was constructed for.
        /// </summary>
        public SevenBitNumber? NoteNumber { get; }

        #endregion

        #region Operators

        /// <summary>
        /// Determines if two <see cref="Rest"/> objects are equal.
        /// </summary>
        /// <param name="rest1">The first <see cref="Rest"/> to compare.</param>
        /// <param name="rest2">The second <see cref="Rest"/> to compare.</param>
        /// <returns><c>true</c> if the rests are equal, <c>false</c> otherwise.</returns>
        public static bool operator ==(Rest rest1, Rest rest2)
        {
            if (ReferenceEquals(rest1, rest2))
                return true;

            if (ReferenceEquals(null, rest1) || ReferenceEquals(null, rest2))
                return false;

            return rest1.Time == rest2.Time &&
                   rest1.Length == rest2.Length &&
                   rest1.Channel == rest2.Channel &&
                   rest1.NoteNumber == rest2.NoteNumber;
        }

        /// <summary>
        /// Determines if two <see cref="Rest"/> objects are not equal.
        /// </summary>
        /// <param name="rest1">The first <see cref="Rest"/> to compare.</param>
        /// <param name="rest2">The second <see cref="Rest"/> to compare.</param>
        /// <returns><c>false</c> if the rests are equal, <c>true</c> otherwise.</returns>
        public static bool operator !=(Rest rest1, Rest rest2)
        {
            return !(rest1 == rest2);
        }

        #endregion

        #region Methods

        public ITimedObject Clone()
        {
            return new Rest(Time, Length, Channel, NoteNumber);
        }

        #endregion

        #region Overrides

        public SplitLengthedObject Split(long time)
        {
            ThrowIfTimeArgument.IsNegative(nameof(time), time);

            //

            var startTime = Time;
            var endTime = startTime + Length;

            if (time <= startTime)
                return new SplitLengthedObject(null, (Rest)Clone());

            if (time >= endTime)
                return new SplitLengthedObject((Rest)Clone(), null);

            //

            var leftPart = (Rest)Clone();
            leftPart.Length = time - startTime;

            var rightPart = (Rest)Clone();
            rightPart.Time = time;
            rightPart.Length = endTime - time;

            return new SplitLengthedObject(leftPart, rightPart);
        }

        /// <summary>
        /// Returns a string that represents the current object.
        /// </summary>
        /// <returns>A string that represents the current object.</returns>
        public override string ToString()
        {
            return $"Rest (channel = {Channel}, note number = {NoteNumber})";
        }

        /// <summary>
        /// Determines whether the specified object is equal to the current object.
        /// </summary>
        /// <param name="obj">The object to compare with the current object.</param>
        /// <returns><c>true</c> if the specified object is equal to the current object; otherwise, <c>false</c>.</returns>
        public override bool Equals(object obj)
        {
            return this == (obj as Rest);
        }

        /// <summary>
        /// Serves as the default hash function.
        /// </summary>
        /// <returns>A hash code for the current object.</returns>
        public override int GetHashCode()
        {
            unchecked
            {
                var result = 17;
                result = result * 23 + Time.GetHashCode();
                result = result * 23 + Length.GetHashCode();
                result = result * 23 + Channel.GetHashCode();
                result = result * 23 + NoteNumber.GetHashCode();
                return result;
            }
        }

        #endregion
    }
}
