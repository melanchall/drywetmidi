using System;
using System.ComponentModel;
using Melanchall.DryWetMidi.Common;

namespace Melanchall.DryWetMidi.Core
{
    /// <summary>
    /// Holds settings according to which <see cref="MidiEvent"/> objects should
    /// be compared for equality.
    /// </summary>
    public sealed class MidiEventEqualityCheckSettings
    {
        #region Fields

        private StringComparison _textComparison = StringComparison.CurrentCulture;

        #endregion

        #region Properties

        /// <summary>
        /// Gets or sets a value indicating whether <see cref="MidiEvent.DeltaTime"/> values
        /// should be compared or not. The default value is <c>true</c>.
        /// </summary>
        public bool CompareDeltaTimes { get; set; } = true;

        /// <summary>
        /// Gets or sets a value that specifies the rules for the comparison of text data (in meta events).
        ///  The default value is <see cref="StringComparison.CurrentCulture"/>.
        /// </summary>
        /// <exception cref="InvalidEnumArgumentException"><paramref name="value"/> specified an invalid value.</exception>
        public StringComparison TextComparison
        {
            get { return _textComparison; }
            set
            {
                ThrowIfArgument.IsInvalidEnumValue(nameof(value), value);

                _textComparison = value;
            }
        }

        #endregion
    }
}
