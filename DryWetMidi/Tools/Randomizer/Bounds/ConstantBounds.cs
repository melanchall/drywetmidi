using System;
using Melanchall.DryWetMidi.Common;
using Melanchall.DryWetMidi.Smf.Interaction;

namespace Melanchall.DryWetMidi.Tools
{
    public class ConstantBounds : IBounds
    {
        #region Constructor

        public ConstantBounds(ITimeSpan size)
            : this(size, size)
        {
        }

        public ConstantBounds(ITimeSpan leftSize, ITimeSpan rightSize)
        {
            ThrowIfArgument.IsNull(nameof(leftSize), leftSize);
            ThrowIfArgument.IsNull(nameof(rightSize), rightSize);

            LeftSize = leftSize;
            RightSize = rightSize;
        }

        #endregion

        #region Properties

        public ITimeSpan LeftSize { get; }

        public ITimeSpan RightSize { get; }

        #endregion

        #region Methods

        private static long CalculateBoundaryTime(long time, ITimeSpan size, MathOperation operation, TempoMap tempoMap)
        {
            ITimeSpan boundaryTime = (MidiTimeSpan)time;

            switch (operation)
            {
                case MathOperation.Add:
                    boundaryTime = boundaryTime.Add(size, TimeSpanMode.TimeLength);
                    break;

                case MathOperation.Subtract:
                    var convertedSize = TimeConverter.ConvertFrom(size, tempoMap);
                    boundaryTime = convertedSize > time
                        ? (MidiTimeSpan)0
                        : boundaryTime.Subtract(size, TimeSpanMode.TimeLength);
                    break;
            }

            return TimeConverter.ConvertFrom(boundaryTime, tempoMap);
        }

        #endregion

        #region IBounds

        public Tuple<long, long> GetBounds(long time, TempoMap tempoMap)
        {
            return Tuple.Create(
                CalculateBoundaryTime(time, LeftSize, MathOperation.Subtract, tempoMap),
                CalculateBoundaryTime(time, RightSize, MathOperation.Add, tempoMap));
        }

        #endregion
    }
}
