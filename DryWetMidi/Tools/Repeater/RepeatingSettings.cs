using Melanchall.DryWetMidi.Common;
using Melanchall.DryWetMidi.Interaction;
using System.ComponentModel;

namespace Melanchall.DryWetMidi.Tools
{
    /// <summary>
    /// Provides settings for <see cref="Repeater"/> tool. More info in the
    /// <see href="xref:a_repeater">Repeater</see> article.
    /// </summary>
    public sealed class RepeatingSettings
    {
        #region Fields

        private ShiftPolicy _shiftPolicy = ShiftPolicy.ShiftByMaxTime;
        private TimeSpanRoundingPolicy _shiftRoundingPolicy = TimeSpanRoundingPolicy.NoRounding;

        #endregion

        #region Properties

        /// <summary>
        /// Gets or sets a value indicating how shift should be calculated. The default value
        /// is <see cref="ShiftPolicy.ShiftByMaxTime"/>.
        /// </summary>
        /// <exception cref="InvalidEnumArgumentException"><paramref name="value"/> specified an invalid value.</exception>
        public ShiftPolicy ShiftPolicy
        {
            get { return _shiftPolicy; }
            set
            {
                ThrowIfArgument.IsInvalidEnumValue(nameof(value), value);
                
                _shiftPolicy = value;
            }
        }

        /// <summary>
        /// Gets or sets fixed shift that will be used in case of <see cref="ShiftPolicy"/> set
        /// to <see cref="ShiftPolicy.ShiftByFixedValue"/>.
        /// </summary>
        public ITimeSpan Shift { get; set; }

        /// <summary>
        /// Gets or sets a way of rounding final shift value. The default value is
        /// <see cref="TimeSpanRoundingPolicy.NoRounding"/>.
        /// </summary>
        /// <exception cref="InvalidEnumArgumentException"><paramref name="value"/> specified an invalid value.</exception>
        public TimeSpanRoundingPolicy ShiftRoundingPolicy
        {
            get { return _shiftRoundingPolicy; }
            set
            {
                ThrowIfArgument.IsInvalidEnumValue(nameof(value), value);

                _shiftRoundingPolicy = value;
            }
        }

        /// <summary>
        /// Gets or sets a step that should be used to round final shift in case of
        /// <see cref="ShiftRoundingPolicy"/> set to a value other than <see cref="TimeSpanRoundingPolicy.NoRounding"/>.
        /// </summary>
        public ITimeSpan ShiftRoundingStep { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether original tempo map should be preserved or not.
        /// The default value is <c>true</c>.
        /// </summary>
        public bool PreserveTempoMap { get; set; } = true;

        #endregion
    }
}
