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
        /// Sets a root note that will be used by next actions of the builder using
        /// <see cref="Interval"/> objects.
        /// </summary>
        /// <param name="rootNote">The root note.</param>
        /// <returns>The current <see cref="PatternBuilder"/>.</returns>
        /// <remarks>
        /// Setting a root note is not an action and thus will not be stored in a pattern.
        /// </remarks>
        /// <exception cref="ArgumentNullException"><paramref name="rootNote"/> is <c>null</c>.</exception>
        public PatternBuilder SetRootNote(MusicTheory.Note rootNote)
        {
            ThrowIfArgument.IsNull(nameof(rootNote), rootNote);

            RootNote = rootNote;
            return this;
        }

        /// <summary>
        /// Sets default velocity that will be used by next actions of the builder.
        /// </summary>
        /// <param name="velocity">New default velocity of a note.</param>
        /// <returns>The current <see cref="PatternBuilder"/>.</returns>
        /// <remarks>
        /// Setting default velocity is not an action and thus will not be stored in a pattern.
        /// </remarks>
        public PatternBuilder SetVelocity(SevenBitNumber velocity)
        {
            Velocity = velocity;
            return this;
        }

        /// <summary>
        /// Sets default note length that will be used by next actions of the builder.
        /// </summary>
        /// <param name="length">New default note length.</param>
        /// <returns>The current <see cref="PatternBuilder"/>.</returns>
        /// <remarks>
        /// Setting default note length is not an action and thus will not be stored in a pattern.
        /// </remarks>
        /// <exception cref="ArgumentNullException"><paramref name="length"/> is <c>null</c>.</exception>
        public PatternBuilder SetNoteLength(ITimeSpan length)
        {
            ThrowIfArgument.IsNull(nameof(length), length);

            NoteLength = length;
            return this;
        }

        /// <summary>
        /// Sets default step for step back and step forward actions of the builder.
        /// </summary>
        /// <param name="step">New default step.</param>
        /// <returns>The current <see cref="PatternBuilder"/>.</returns>
        /// <remarks>
        /// Setting default step is not an action and thus will not be stored in a pattern.
        /// </remarks>
        /// <exception cref="ArgumentNullException"><paramref name="step"/> is <c>null</c>.</exception>
        public PatternBuilder SetStep(ITimeSpan step)
        {
            ThrowIfArgument.IsNull(nameof(step), step);

            Step = step;
            return this;
        }

        /// <summary>
        /// Sets default note octave that will be used by next actions of the builder.
        /// </summary>
        /// <param name="octave">New default octave.</param>
        /// <returns>The current <see cref="PatternBuilder"/>.</returns>
        /// <remarks>
        /// Setting default octave is not an action and thus will not be stored in a pattern.
        /// </remarks>
        /// <exception cref="ArgumentNullException"><paramref name="octave"/> is <c>null</c>.</exception>
        public PatternBuilder SetOctave(Octave octave)
        {
            ThrowIfArgument.IsNull(nameof(octave), octave);

            Octave = octave;
            return this;
        }

        #endregion
    }
}
