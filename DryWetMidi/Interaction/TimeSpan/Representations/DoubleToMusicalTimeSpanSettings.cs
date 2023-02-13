using Melanchall.DryWetMidi.Common;

namespace Melanchall.DryWetMidi.Interaction
{
    public sealed class DoubleToMusicalTimeSpanSettings
    {
        #region Fields

        private int _maxIterationsCount = 20;
        private double _fractionalPartEpsilon = 0.0000001;
        private double _precision = 0.00001;

        #endregion

        #region Properties

        public int MaxIterationsCount
        {
            get { return _maxIterationsCount; }
            set
            {
                ThrowIfArgument.IsNonpositive(nameof(value), value, "Value is zero or negative.");

                _maxIterationsCount = value;
            }
        }

        public double FractionalPartEpsilon
        {
            get { return _fractionalPartEpsilon; }
            set
            {
                ThrowIfArgument.IsNonpositive(nameof(value), value, "Value is zero or negative.");

                _fractionalPartEpsilon = value;
            }
        }

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
