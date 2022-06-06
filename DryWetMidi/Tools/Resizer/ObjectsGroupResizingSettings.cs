using Melanchall.DryWetMidi.Common;
using Melanchall.DryWetMidi.Interaction;
using System.ComponentModel;

namespace Melanchall.DryWetMidi.Tools
{
    /// <summary>
    /// Defines how an objects group should be resized. More info in the
    /// <see href="xref:a_resizer">Resizer</see> article.
    /// </summary>
    /// <seealso cref="Resizer"/>
    public sealed class ObjectsGroupResizingSettings
    {
        #region Fields

        private TimeSpanType _distanceCalculationType = TimeSpanType.Midi;

        #endregion

        #region Properties

        /// <summary>
        /// Gets or sets the type of distance calculations. The default value is <see cref="TimeSpanType.Midi"/>.
        /// </summary>
        /// <exception cref="InvalidEnumArgumentException"><paramref name="value"/> specified an invalid value.</exception>
        public TimeSpanType DistanceCalculationType
        {
            get { return _distanceCalculationType; }
            set
            {
                ThrowIfArgument.IsInvalidEnumValue(nameof(value), value);

                _distanceCalculationType = value;
            }
        }

        #endregion
    }
}
