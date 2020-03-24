using System.ComponentModel;
using Melanchall.DryWetMidi.Common;
using Melanchall.DryWetMidi.Interaction;

namespace Melanchall.DryWetMidi.Tools
{
    /// <summary>
    /// Settings according to which lengthed objects should be randomized.
    /// </summary>
    public abstract class LengthedObjectsRandomizingSettings<TObject> : RandomizingSettings<TObject>
        where TObject : ILengthedObject
    {
        #region Fields

        private LengthedObjectTarget _randomizingTarget = LengthedObjectTarget.Start;

        #endregion

        #region Properties

        /// <summary>
        /// Gets or sets the side of an object that should be randomized.
        /// The default value is <see cref="LengthedObjectTarget.Start"/>.
        /// </summary>
        /// <exception cref="InvalidEnumArgumentException"><paramref name="value"/> specified an invalid value.</exception>
        public LengthedObjectTarget RandomizingTarget
        {
            get { return _randomizingTarget; }
            set
            {
                ThrowIfArgument.IsInvalidEnumValue(nameof(value), value);

                _randomizingTarget = value;
            }
        }

        /// <summary>
        /// Gets or sets a value indicating whether an opposite side of an object should be fixed or not.
        /// The default value is <c>false</c>.
        /// </summary>
        /// <remarks>
        /// When an object's side is fixed the length can be changed during randomizing.
        /// </remarks>
        public bool FixOppositeEnd { get; set; }

        #endregion
    }
}
