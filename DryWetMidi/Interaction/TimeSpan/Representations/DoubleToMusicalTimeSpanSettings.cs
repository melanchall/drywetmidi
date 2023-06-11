using Melanchall.DryWetMidi.Common;
using System;

namespace Melanchall.DryWetMidi.Interaction
{
    /// <summary>
    /// Settings which control the process of conversion a double number to an instance
    /// of the <see cref="MusicalTimeSpan"/>.
    /// </summary>
    /// <seealso cref="MusicalTimeSpan.FromDouble(double, DoubleToMusicalTimeSpanSettings)"/>
    public sealed class DoubleToMusicalTimeSpanSettings
    {
        #region Constants

        /// <summary>
        /// Represents the default maximum count of iterations the rationalization process
        /// should take before stop (see <see cref="MaxIterationsCount"/>).
        /// </summary>
        public const int DefaultMaxIterationsCount = 20;

        /// <summary>
        /// Represents the default maximum fractional part value to consider a number as integer during the
        /// rationalization process (see <see cref="FractionalPartEpsilon"/>).
        /// </summary>
        public const double DefaultFractionalPartEpsilon = 0.0000001;

        /// <summary>
        /// Represents the default maximum difference between an input double number and the calculated
        /// <see cref="MusicalTimeSpan"/> (see <see cref="Precision"/>).
        /// </summary>
        public const double DefaultPrecision = 0.00001;

        #endregion

        #region Fields

        private int _maxIterationsCount = DefaultMaxIterationsCount;
        private double _fractionalPartEpsilon = 0.0000001;
        private double _precision = 0.00001;

        #endregion

        #region Properties

        /// <summary>
        /// Gets or sets the maximum count of iterations the rationalization process
        /// should take before stop. The default value is <c>20</c>.
        /// </summary>
        /// <exception cref="ArgumentOutOfRangeException">The value is zero or negative.</exception>
        public int MaxIterationsCount
        {
            get { return _maxIterationsCount; }
            set
            {
                ThrowIfArgument.IsNonpositive(nameof(value), value, "Value is zero or negative.");

                _maxIterationsCount = value;
            }
        }

        /// <summary>
        /// Gets or sets a maximum fractional part value to consider a number as integer during the
        /// rationalization process. For example, <c>1.001</c> will be considered as just <c>1.0</c> if
        /// <see cref="FractionalPartEpsilon"/> is <c>0.001</c> or greater.
        /// </summary>
        /// <exception cref="ArgumentOutOfRangeException">The value is zero or negative.</exception>
        public double FractionalPartEpsilon
        {
            get { return _fractionalPartEpsilon; }
            set
            {
                ThrowIfArgument.IsNonpositive(nameof(value), value, "Value is zero or negative.");

                _fractionalPartEpsilon = value;
            }
        }

        /// <summary>
        /// Gets or sets the maximum difference between an input double number and the calculated
        /// <see cref="MusicalTimeSpan"/>. For example, <c>3/2</c> will be returned as the result for
        /// the input number of <c>1.5001</c> if <see cref="Precision"/> is <c>0.0001</c> or greater.
        /// </summary>
        /// <exception cref="ArgumentOutOfRangeException">The value is zero or negative.</exception>
        public double Precision
        {
            get { return _precision; }
            set
            {
                ThrowIfArgument.IsNonpositive(nameof(value), value, "Value is zero or negative.");

                _precision = value;
            }
        }

        #endregion
    }
}
