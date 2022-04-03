using Melanchall.DryWetMidi.Common;
using Melanchall.DryWetMidi.Interaction;
using Melanchall.DryWetMidi.MusicTheory;
using System;

namespace Melanchall.DryWetMidi.Composing
{
    public sealed partial class PatternBuilder
    {
        #region Methods

        /// <summary>
        /// Adds a note by the specified interval relative to the current root note using
        /// default length and velocity.
        /// </summary>
        /// <param name="interval">The <see cref="Interval"/> which defines
        /// a number of half steps from the current root note.</param>
        /// <returns>The current <see cref="PatternBuilder"/>.</returns>
        /// <remarks>
        /// To set root note use <see cref="SetRootNote(MusicTheory.Note)"/> method. By default the root note is C4.
        /// To set default note length use <see cref="SetNoteLength(ITimeSpan)"/> method. By default the length
        /// is 1/4. To set default velocity use <see cref="SetVelocity(SevenBitNumber)"/> method. By default the
        /// velocity is 100.
        /// </remarks>
        /// <exception cref="ArgumentNullException"><paramref name="interval"/> is <c>null</c>.</exception>
        /// <exception cref="ArgumentOutOfRangeException">The number of result note is out of valid range.</exception>
        public PatternBuilder Note(Interval interval)
        {
            return Note(interval, NoteLength, Velocity);
        }

        /// <summary>
        /// Adds a note by the specified interval relative to the current root note using
        /// specified length and default velocity.
        /// </summary>
        /// <param name="interval">The <see cref="Interval"/> which defines
        /// a number of half steps from the current root note.</param>
        /// <param name="length">The length of a note.</param>
        /// <returns>The current <see cref="PatternBuilder"/>.</returns>
        /// <remarks>
        /// To set root note use <see cref="SetRootNote(MusicTheory.Note)"/> method. By default the root note is C4.
        /// To set default velocity use <see cref="SetVelocity(SevenBitNumber)"/> method. By default the
        /// velocity is 100.
        /// </remarks>
        /// <exception cref="ArgumentNullException">
        /// <para>One of the following errors occured:</para>
        /// <list type="bullet">
        /// <item>
        /// <description><paramref name="interval"/> is <c>null</c>.</description>
        /// </item>
        /// <item>
        /// <description><paramref name="length"/> is <c>null</c>.</description>
        /// </item>
        /// </list>
        /// </exception>
        /// <exception cref="ArgumentOutOfRangeException">The number of result note is out of valid range.</exception>
        public PatternBuilder Note(Interval interval, ITimeSpan length)
        {
            return Note(interval, length, Velocity);
        }

        /// <summary>
        /// Adds a note by the specified interval relative to the current root note using
        /// default length and specified velocity.
        /// </summary>
        /// <param name="interval">The <see cref="Interval"/> which defines
        /// a number of half steps from the current root note.</param>
        /// <param name="velocity">The velocity of a note.</param>
        /// <returns>The current <see cref="PatternBuilder"/>.</returns>
        /// <remarks>
        /// To set root note use <see cref="SetRootNote(MusicTheory.Note)"/> method. By default the root note is C4.
        /// To set default note length use <see cref="SetNoteLength(ITimeSpan)"/> method. By default the length
        /// is 1/4.
        /// </remarks>
        /// <exception cref="ArgumentNullException"><paramref name="interval"/> is <c>null</c>.</exception>
        /// <exception cref="ArgumentOutOfRangeException">The number of result note is out of valid range.</exception>
        public PatternBuilder Note(Interval interval, SevenBitNumber velocity)
        {
            return Note(interval, NoteLength, velocity);
        }

        /// <summary>
        /// Adds a note by the specified interval relative to the current root note using
        /// specified length and velocity.
        /// </summary>
        /// <param name="interval">The <see cref="Interval"/> which defines
        /// a number of half steps from the current root note.</param>
        /// <param name="length">The length of a note.</param>
        /// <param name="velocity">The velocity of a note.</param>
        /// <returns>The current <see cref="PatternBuilder"/>.</returns>
        /// <remarks>
        /// To set root note use <see cref="SetRootNote(MusicTheory.Note)"/> method. By default the root note is C4.
        /// </remarks>
        /// <exception cref="ArgumentNullException">
        /// <para>One of the following errors occured:</para>
        /// <list type="bullet">
        /// <item>
        /// <description><paramref name="interval"/> is <c>null</c>.</description>
        /// </item>
        /// <item>
        /// <description><paramref name="length"/> is <c>null</c>.</description>
        /// </item>
        /// </list>
        /// </exception>
        /// <exception cref="ArgumentOutOfRangeException">The number of result note is out of valid range.</exception>
        public PatternBuilder Note(Interval interval, ITimeSpan length, SevenBitNumber velocity)
        {
            ThrowIfArgument.IsNull(nameof(interval), interval);

            return Note(RootNote.Transpose(interval), length, velocity);
        }

        /// <summary>
        /// Adds a note by the specified note name using default velocity, length and octave.
        /// </summary>
        /// <param name="noteName">The name of a note.</param>
        /// <returns>The current <see cref="PatternBuilder"/>.</returns>
        /// <remarks>
        /// To set default octave use <see cref="SetOctave"/> method. By default the octave number is 4.
        /// To set default note length use <see cref="SetNoteLength(ITimeSpan)"/> method. By default the length
        /// is 1/4. To set default velocity use <see cref="SetVelocity(SevenBitNumber)"/> method. By default the
        /// velocity is 100.
        /// </remarks>
        /// <exception cref="InvalidEnumArgumentException"><paramref name="noteName"/> specified an invalid value.</exception>
        public PatternBuilder Note(NoteName noteName)
        {
            return Note(noteName, NoteLength, Velocity);
        }

        /// <summary>
        /// Adds a note by the specified note name using specified length and default velocity and octave.
        /// </summary>
        /// <param name="noteName">The name of a note.</param>
        /// <param name="length">The length of a note.</param>
        /// <returns>The current <see cref="PatternBuilder"/>.</returns>
        /// <remarks>
        /// To set default octave use <see cref="SetOctave"/> method. By default the octave number is 4.
        /// To set default velocity use <see cref="SetVelocity(SevenBitNumber)"/> method. By default the
        /// velocity is 100.
        /// </remarks>
        /// <exception cref="InvalidEnumArgumentException"><paramref name="noteName"/> specified an invalid value.</exception>
        /// <exception cref="ArgumentNullException"><paramref name="length"/> is <c>null</c>.</exception>
        public PatternBuilder Note(NoteName noteName, ITimeSpan length)
        {
            return Note(noteName, length, Velocity);
        }

        /// <summary>
        /// Adds a note by the specified note name using specified velocity and default length and octave.
        /// </summary>
        /// <param name="noteName">The name of a note.</param>
        /// <param name="velocity">The velocity of a note.</param>
        /// <returns>The current <see cref="PatternBuilder"/>.</returns>
        /// <remarks>
        /// To set default octave use <see cref="SetOctave"/> method. By default the octave number is 4.
        /// To set default note length use <see cref="SetNoteLength(ITimeSpan)"/> method. By default the length
        /// is 1/4.
        /// </remarks>
        /// <exception cref="InvalidEnumArgumentException"><paramref name="noteName"/> specified an invalid value.</exception>
        public PatternBuilder Note(NoteName noteName, SevenBitNumber velocity)
        {
            return Note(noteName, NoteLength, velocity);
        }

        /// <summary>
        /// Adds a note by the specified note name using specified velocity and length, and default octave.
        /// </summary>
        /// <param name="noteName">The name of a note.</param>
        /// <param name="length">The length of a note.</param>
        /// <param name="velocity">The velocity of a note.</param>
        /// <returns>The current <see cref="PatternBuilder"/>.</returns>
        /// <remarks>
        /// To set default octave use <see cref="SetOctave"/> method. By default the octave number is 4.
        /// </remarks>
        /// <exception cref="InvalidEnumArgumentException"><paramref name="noteName"/> specified an invalid value.</exception>
        /// <exception cref="ArgumentNullException"><paramref name="length"/> is <c>null</c>.</exception>
        public PatternBuilder Note(NoteName noteName, ITimeSpan length, SevenBitNumber velocity)
        {
            ThrowIfArgument.IsInvalidEnumValue(nameof(noteName), noteName);

            return Note(Octave.GetNote(noteName), length, velocity);
        }

        /// <summary>
        /// Adds a note using default length and velocity.
        /// </summary>
        /// <param name="note">A note.</param>
        /// <returns>The current <see cref="PatternBuilder"/>.</returns>
        /// <remarks>
        /// To set default note length use <see cref="SetNoteLength(ITimeSpan)"/> method. By default the length
        /// is 1/4. To set default velocity use <see cref="SetVelocity(SevenBitNumber)"/> method. By default the
        /// velocity is 100.
        /// </remarks>
        /// <exception cref="ArgumentNullException"><paramref name="note"/> is <c>null</c>.</exception>
        public PatternBuilder Note(MusicTheory.Note note)
        {
            return Note(note, NoteLength, Velocity);
        }

        /// <summary>
        /// Adds a note using specified length and default velocity.
        /// </summary>
        /// <param name="note">A note.</param>
        /// <param name="length">The length of a note.</param>
        /// <returns>The current <see cref="PatternBuilder"/>.</returns>
        /// <remarks>
        /// To set default velocity use <see cref="SetVelocity(SevenBitNumber)"/> method. By default the
        /// velocity is 100.
        /// </remarks>
        /// <exception cref="ArgumentNullException">
        /// <para>One of the following errors occured:</para>
        /// <list type="bullet">
        /// <item>
        /// <description><paramref name="note"/> is <c>null</c>.</description>
        /// </item>
        /// <item>
        /// <description><paramref name="length"/> is <c>null</c>.</description>
        /// </item>
        /// </list>
        /// </exception>
        public PatternBuilder Note(MusicTheory.Note note, ITimeSpan length)
        {
            return Note(note, length, Velocity);
        }

        /// <summary>
        /// Adds a note using specified velocity and default length.
        /// </summary>
        /// <param name="note">A note.</param>
        /// <param name="velocity">The velocity of a note.</param>
        /// <returns>The current <see cref="PatternBuilder"/>.</returns>
        /// <remarks>
        /// To set default note length use <see cref="SetNoteLength(ITimeSpan)"/> method. By default the length
        /// is 1/4.
        /// </remarks>
        /// <exception cref="ArgumentNullException"><paramref name="note"/> is <c>null</c>.</exception>
        public PatternBuilder Note(MusicTheory.Note note, SevenBitNumber velocity)
        {
            return Note(note, NoteLength, velocity);
        }

        /// <summary>
        /// Adds a note using specified velocity and length.
        /// </summary>
        /// <param name="note">A note.</param>
        /// <param name="length">The length of the note.</param>
        /// <param name="velocity">The velocity of a note.</param>
        /// <returns>The current <see cref="PatternBuilder"/>.</returns>
        /// <exception cref="ArgumentNullException">
        /// <para>One of the following errors occured:</para>
        /// <list type="bullet">
        /// <item>
        /// <description><paramref name="note"/> is <c>null</c>.</description>
        /// </item>
        /// <item>
        /// <description><paramref name="length"/> is <c>null</c>.</description>
        /// </item>
        /// </list>
        /// </exception>
        public PatternBuilder Note(MusicTheory.Note note, ITimeSpan length, SevenBitNumber velocity)
        {
            ThrowIfArgument.IsNull(nameof(note), note);
            ThrowIfArgument.IsNull(nameof(length), length);

            return AddAction(new AddNoteAction(new NoteDescriptor(note, velocity, length)));
        }

        /// <summary>
        /// Adds a note using default length and velocity.
        /// </summary>
        /// <param name="note">A note as a string (like A2, for example).</param>
        /// <returns>The current <see cref="PatternBuilder"/>.</returns>
        /// <remarks>
        /// To set default note length use <see cref="SetNoteLength(ITimeSpan)"/> method. By default the length
        /// is 1/4. To set default velocity use <see cref="SetVelocity(SevenBitNumber)"/> method. By default the
        /// velocity is 100.
        /// </remarks>
        /// <exception cref="ArgumentException"><paramref name="note"/> is <c>null</c> or contains white-spaces only.</exception>
        /// <exception cref="FormatException"><paramref name="note"/> has invalid format.</exception>
        public PatternBuilder Note(string note)
        {
            ThrowIfArgument.IsNullOrWhiteSpaceString(nameof(note), note, "Note");

            return Note(note, NoteLength, Velocity);
        }

        /// <summary>
        /// Adds a note using specified length and default velocity.
        /// </summary>
        /// <param name="note">A note as a string (like A2, for example).</param>
        /// <param name="length">The length of a note.</param>
        /// <returns>The current <see cref="PatternBuilder"/>.</returns>
        /// <remarks>
        /// To set default velocity use <see cref="SetVelocity(SevenBitNumber)"/> method. By default the
        /// velocity is 100.
        /// </remarks>
        /// <exception cref="ArgumentNullException"><paramref name="length"/> is <c>null</c>.</exception>
        /// <exception cref="ArgumentException"><paramref name="note"/> is <c>null</c> or contains white-spaces only.</exception>
        /// <exception cref="FormatException"><paramref name="note"/> has invalid format.</exception>
        public PatternBuilder Note(string note, ITimeSpan length)
        {
            ThrowIfArgument.IsNullOrWhiteSpaceString(nameof(note), note, "Note");
            ThrowIfArgument.IsNull(nameof(length), length);

            return Note(note, length, Velocity);
        }

        /// <summary>
        /// Adds a note using specified velocity and default length.
        /// </summary>
        /// <param name="note">A note as a string (like A2, for example).</param>
        /// <param name="velocity">The velocity of a note.</param>
        /// <returns>The current <see cref="PatternBuilder"/>.</returns>
        /// <remarks>
        /// To set default note length use <see cref="SetNoteLength(ITimeSpan)"/> method. By default the length
        /// is 1/4.
        /// </remarks>
        /// <exception cref="ArgumentException"><paramref name="note"/> is <c>null</c> or contains white-spaces only.</exception>
        /// <exception cref="FormatException"><paramref name="note"/> has invalid format.</exception>
        public PatternBuilder Note(string note, SevenBitNumber velocity)
        {
            ThrowIfArgument.IsNullOrWhiteSpaceString(nameof(note), note, "Note");

            return Note(note, NoteLength, velocity);
        }

        /// <summary>
        /// Adds a note using specified velocity and length.
        /// </summary>
        /// <param name="note">A note as a string (like A2, for example).</param>
        /// <param name="length">The length of the note.</param>
        /// <param name="velocity">The velocity of a note.</param>
        /// <returns>The current <see cref="PatternBuilder"/>.</returns>
        /// <exception cref="ArgumentNullException"><paramref name="length"/> is <c>null</c>.</exception>
        /// <exception cref="ArgumentException"><paramref name="note"/> is <c>null</c> or contains white-spaces only.</exception>
        /// <exception cref="FormatException"><paramref name="note"/> has invalid format.</exception>
        public PatternBuilder Note(string note, ITimeSpan length, SevenBitNumber velocity)
        {
            ThrowIfArgument.IsNullOrWhiteSpaceString(nameof(note), note, "Note");
            ThrowIfArgument.IsNull(nameof(length), length);

            return Note(MusicTheory.Note.Parse(note), length, velocity);
        }

        #endregion
    }
}
