using Melanchall.DryWetMidi.Common;
using Melanchall.DryWetMidi.Core;

namespace Melanchall.DryWetMidi.Composing
{
    public sealed partial class PatternBuilder
    {
        #region Methods

        /// <summary>
        /// Inserts <see cref="ControlChangeEvent"/> to specify a change of a controller that will be used by following notes.
        /// </summary>
        /// <param name="controlNumber">Controller number.</param>
        /// <param name="controlValue">Controller value.</param>
        /// <returns>The current <see cref="PatternBuilder"/>.</returns>
        public PatternBuilder ControlChange(SevenBitNumber controlNumber, SevenBitNumber controlValue)
        {
            return AddAction(new AddControlChangeEventAction(controlNumber, controlValue));
        }

        #endregion
    }
}
