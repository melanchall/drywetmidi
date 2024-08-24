using Melanchall.DryWetMidi.Common;
using Melanchall.DryWetMidi.Core;
using System;

namespace Melanchall.DryWetMidi.Composing
{
    public sealed partial class PatternBuilder
    {
        #region Methods

        /// <summary>
        /// Inserts <see cref="PitchBendEvent"/> to specify a pitch bend that will be used by following notes.
        /// </summary>
        /// <param name="pitchValue">Pitch value.</param>
        /// <returns>The current <see cref="PatternBuilder"/>.</returns>
        /// <exception cref="ArgumentOutOfRangeException"><paramref name="pitchValue"/> is out of
        /// [<see cref="PitchBendEvent.MinPitchValue"/>; <see cref="PitchBendEvent.MaxPitchValue"/>] range.</exception>
        public PatternBuilder PitchBend(ushort pitchValue)
        {
            ThrowIfArgument.IsOutOfRange(
                nameof(pitchValue),
                pitchValue,
                PitchBendEvent.MinPitchValue,
                PitchBendEvent.MaxPitchValue,
                $"Pitch value is out of [{PitchBendEvent.MinPitchValue}; {PitchBendEvent.MaxPitchValue}] range.");

            return AddAction(new AddPitchBendEventAction(pitchValue));
        }

        #endregion
    }
}
