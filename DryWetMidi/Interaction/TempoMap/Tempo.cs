using System;
using Melanchall.DryWetMidi.Common;
using Melanchall.DryWetMidi.Core;

namespace Melanchall.DryWetMidi.Interaction
{
    /// <summary>
    /// Represents tempo expressed in microseconds per quarter note or beats per minute.
    /// </summary>
    /// <seealso cref="TempoMap"/>
    public sealed class Tempo
    {
        #region Constants

        /// <summary>
        /// Default tempo which is 500,000 microseconds per quarter note or 120 beats per minute.
        /// </summary>
        public static readonly Tempo Default = new Tempo(SetTempoEvent.DefaultMicrosecondsPerQuarterNote);

        #endregion

        #region Fields

        private const int MicrosecondsInMinute = 60000000;
        private const int MicrosecondsInMillisecond = 1000;

        #endregion

        #region Constructor

        /// <summary>
        /// Initializes a new instance of the <see cref="Tempo"/> with the specified number of
        /// microseconds per quarter note.
        /// </summary>
        /// <param name="microsecondsPerQuarterNote">Number of microseconds per quarter note.</param>
        /// <exception cref="ArgumentOutOfRangeException"><paramref name="microsecondsPerQuarterNote"/>
        /// is zero or negative.</exception>
        public Tempo(long microsecondsPerQuarterNote)
        {
            ThrowIfArgument.IsNonpositive(nameof(microsecondsPerQuarterNote),
                                          microsecondsPerQuarterNote,
                                          "Number of microseconds per quarter note is zero or negative.");

            MicrosecondsPerQuarterNote = microsecondsPerQuarterNote;
        }

        #endregion

        #region Properties

        /// <summary>
        /// Gets number of microseconds per quarter note.
        /// </summary>
        public long MicrosecondsPerQuarterNote { get; }

        /// <summary>
        /// Gets the number of beats per minute.
        /// </summary>
        public double BeatsPerMinute => (double)MicrosecondsInMinute / MicrosecondsPerQuarterNote;

        #endregion

        #region Methods

        /// <summary>
        /// Creates an instance of the <see cref="Tempo"/> with the specified number of
        /// milliseconds per quarter note.
        /// </summary>
        /// <param name="millisecondsPerQuarterNote">Number of milliseconds per quarter note.</param>
        /// <returns>An instance of the <see cref="Tempo"/> which represents tempo as specified
        /// number of milliseconds per quarter note.</returns>
        /// <exception cref="ArgumentOutOfRangeException"><paramref name="millisecondsPerQuarterNote"/>
        /// is zero or negative.</exception>
        public static Tempo FromMillisecondsPerQuarterNote(long millisecondsPerQuarterNote)
        {
            ThrowIfArgument.IsNonpositive(nameof(millisecondsPerQuarterNote),
                                          millisecondsPerQuarterNote,
                                          "Number of milliseconds per quarter note is zero or negative.");

            return new Tempo(millisecondsPerQuarterNote * MicrosecondsInMillisecond);
        }

        /// <summary>
        /// Creates an instance of the <see cref="Tempo"/> with the specified number of
        /// beats per minute.
        /// </summary>
        /// <param name="beatsPerMinute">Number of beats per minute.</param>
        /// <returns>An instance of the <see cref="Tempo"/> which represents tempo as specified
        /// number of beats per minute.</returns>
        /// <exception cref="ArgumentOutOfRangeException"><paramref name="beatsPerMinute"/>
        /// is zero or negative.</exception>
        public static Tempo FromBeatsPerMinute(double beatsPerMinute)
        {
            ThrowIfArgument.IsNonpositive(nameof(beatsPerMinute),
                                          beatsPerMinute,
                                          "Number of beats per minute is zero or negative.");

            return new Tempo(MathUtilities.RoundToLong(MicrosecondsInMinute / beatsPerMinute));
        }

        #endregion

        #region Operators

        /// <summary>
        /// Determines if two <see cref="Tempo"/> objects are equal.
        /// </summary>
        /// <param name="tempo1">The first <see cref="Tempo"/> to compare.</param>
        /// <param name="tempo2">The second <see cref="Tempo"/> to compare.</param>
        /// <returns><c>true</c> if the tempos are equal, <c>false</c> otherwise.</returns>
        public static bool operator ==(Tempo tempo1, Tempo tempo2)
        {
            if (ReferenceEquals(tempo1, tempo2))
                return true;

            if (ReferenceEquals(null, tempo1) || ReferenceEquals(null, tempo2))
                return false;

            return tempo1.MicrosecondsPerQuarterNote == tempo2.MicrosecondsPerQuarterNote;
        }

        /// <summary>
        /// Determines if two <see cref="Tempo"/> objects are not equal.
        /// </summary>
        /// <param name="tempo1">The first <see cref="Tempo"/> to compare.</param>
        /// <param name="tempo2">The second <see cref="Tempo"/> to compare.</param>
        /// <returns><c>false</c> if the tempos are equal, <c>true</c> otherwise.</returns>
        public static bool operator !=(Tempo tempo1, Tempo tempo2)
        {
            return !(tempo1 == tempo2);
        }

        /// <summary>
        /// Indicates whether a specified <see cref="Tempo"/> is greater than another one.
        /// </summary>
        /// <param name="tempo1">The first <see cref="Tempo"/> to compare.</param>
        /// <param name="tempo2">The second <see cref="Tempo"/> to compare.</param>
        /// <returns><c>true</c> if the value of <paramref name="tempo1"/> is greater than the value of
        /// <paramref name="tempo2"/>; otherwise, <c>false</c>.</returns>
        /// <exception cref="ArgumentNullException">
        /// <para>One of the following errors occurred:</para>
        /// <list type="bullet">
        /// <item>
        /// <description><paramref name="tempo1"/> is <c>null</c>.</description>
        /// </item>
        /// <item>
        /// <description><paramref name="tempo2"/> is <c>null</c>.</description>
        /// </item>
        /// </list>
        /// </exception>
        public static bool operator >(Tempo tempo1, Tempo tempo2)
        {
            ThrowIfArgument.IsNull(nameof(tempo1), tempo1);
            ThrowIfArgument.IsNull(nameof(tempo2), tempo2);

            return tempo1.MicrosecondsPerQuarterNote > tempo2.MicrosecondsPerQuarterNote;
        }

        /// <summary>
        /// Indicates whether a specified <see cref="Tempo"/> is greater than or equal to
        /// another one.
        /// </summary>
        /// <param name="tempo1">The first <see cref="Tempo"/> to compare.</param>
        /// <param name="tempo2">The second <see cref="Tempo"/> to compare.</param>
        /// <returns><c>true</c> if the value of <paramref name="tempo1"/> is greater than or equal to the value of
        /// <paramref name="tempo2"/>; otherwise, <c>false</c>.</returns>
        /// <exception cref="ArgumentNullException">
        /// <para>One of the following errors occurred:</para>
        /// <list type="bullet">
        /// <item>
        /// <description><paramref name="tempo1"/> is <c>null</c>.</description>
        /// </item>
        /// <item>
        /// <description><paramref name="tempo2"/> is <c>null</c>.</description>
        /// </item>
        /// </list>
        /// </exception>
        public static bool operator >=(Tempo tempo1, Tempo tempo2)
        {
            ThrowIfArgument.IsNull(nameof(tempo1), tempo1);
            ThrowIfArgument.IsNull(nameof(tempo2), tempo2);

            return tempo1.MicrosecondsPerQuarterNote >= tempo2.MicrosecondsPerQuarterNote;
        }

        /// <summary>
        /// Indicates whether a specified <see cref="Tempo"/> is less than another one.
        /// </summary>
        /// <param name="tempo1">The first <see cref="Tempo"/> to compare.</param>
        /// <param name="tempo2">The second <see cref="Tempo"/> to compare.</param>
        /// <returns><c>true</c> if the value of <paramref name="tempo1"/> is less than the value of
        /// <paramref name="tempo2"/>; otherwise, <c>false</c>.</returns>
        /// <exception cref="ArgumentNullException">
        /// <para>One of the following errors occurred:</para>
        /// <list type="bullet">
        /// <item>
        /// <description><paramref name="tempo1"/> is <c>null</c>.</description>
        /// </item>
        /// <item>
        /// <description><paramref name="tempo2"/> is <c>null</c>.</description>
        /// </item>
        /// </list>
        /// </exception>
        public static bool operator <(Tempo tempo1, Tempo tempo2)
        {
            ThrowIfArgument.IsNull(nameof(tempo1), tempo1);
            ThrowIfArgument.IsNull(nameof(tempo2), tempo2);

            return tempo1.MicrosecondsPerQuarterNote < tempo2.MicrosecondsPerQuarterNote;
        }

        /// <summary>
        /// Indicates whether a specified <see cref="Tempo"/> is less than or equal to
        /// another one.
        /// </summary>
        /// <param name="tempo1">The first <see cref="Tempo"/> to compare.</param>
        /// <param name="tempo2">The second <see cref="Tempo"/> to compare.</param>
        /// <returns><c>true</c> if the value of <paramref name="tempo1"/> is less than or equal to the value of
        /// <paramref name="tempo2"/>; otherwise, <c>false</c>.</returns>
        /// <exception cref="ArgumentNullException">
        /// <para>One of the following errors occurred:</para>
        /// <list type="bullet">
        /// <item>
        /// <description><paramref name="tempo1"/> is <c>null</c>.</description>
        /// </item>
        /// <item>
        /// <description><paramref name="tempo2"/> is <c>null</c>.</description>
        /// </item>
        /// </list>
        /// </exception>
        public static bool operator <=(Tempo tempo1, Tempo tempo2)
        {
            ThrowIfArgument.IsNull(nameof(tempo1), tempo1);
            ThrowIfArgument.IsNull(nameof(tempo2), tempo2);

            return tempo1.MicrosecondsPerQuarterNote <= tempo2.MicrosecondsPerQuarterNote;
        }

        #endregion

        #region Overrides

        /// <summary>
        /// Returns a string that represents the current object.
        /// </summary>
        /// <returns>A string that represents the current object.</returns>
        public override string ToString()
        {
            return $"{MicrosecondsPerQuarterNote} μs/qnote";
        }

        /// <summary>
        /// Determines whether the specified object is equal to the current object.
        /// </summary>
        /// <param name="obj">The object to compare with the current object.</param>
        /// <returns><c>true</c> if the specified object is equal to the current object; otherwise, <c>false</c>.</returns>
        public override bool Equals(object obj)
        {
            return this == (obj as Tempo);
        }

        /// <summary>
        /// Returns the hash code for this instance.
        /// </summary>
        /// <returns>A 32-bit signed integer hash code.</returns>
        public override int GetHashCode()
        {
            return MicrosecondsPerQuarterNote.GetHashCode();
        }

        #endregion
    }
}
