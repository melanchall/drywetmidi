using System;
using Melanchall.DryWetMidi.Common;
using Melanchall.DryWetMidi.Interaction;

namespace Melanchall.DryWetMidi.Tools
{
    /// <summary>
    /// Time range which is constant for each object to be processed.
    /// </summary>
    public sealed class ConstantBounds : IBounds
    {
        #region Constructor

        /// <summary>
        /// Initializes a new instance of the <see cref="ConstantBounds"/> with the specified size
        /// so time range will span for equal length to the left and to the right from time
        /// to calculate bounds relative to.
        /// </summary>
        /// <param name="size">Size of the time range. The length of time range will be 2 * <paramref name="size"/>.</param>
        /// <exception cref="ArgumentNullException"><paramref name="size"/> is <c>null</c>.</exception>
        public ConstantBounds(ITimeSpan size)
            : this(size, size)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ConstantBounds"/> with the specified size of left
        /// and right parts so time range will span for different length to the left and to the right from time
        /// to calculate bounds relative to.
        /// </summary>
        /// <param name="leftSize">Left part's size of the time range.</param>
        /// <param name="rightSize">Right part's size of the time range.</param>
        /// <exception cref="ArgumentNullException">
        /// <para>One of the following errors occurred:</para>
        /// <list type="bullet">
        /// <item>
        /// <description><paramref name="leftSize"/> is <c>null</c>.</description>
        /// </item>
        /// <item>
        /// <description><paramref name="rightSize"/> is <c>null</c>.</description>
        /// </item>
        /// </list>
        /// </exception>
        public ConstantBounds(ITimeSpan leftSize, ITimeSpan rightSize)
        {
            ThrowIfArgument.IsNull(nameof(leftSize), leftSize);
            ThrowIfArgument.IsNull(nameof(rightSize), rightSize);

            LeftSize = leftSize;
            RightSize = rightSize;
        }

        #endregion

        #region Properties

        /// <summary>
        /// Gets the size of left part of the time range represented by the current <see cref="ConstantBounds"/>.
        /// </summary>
        public ITimeSpan LeftSize { get; }

        /// <summary>
        /// Gets the size of right part of the time range represented by the current <see cref="ConstantBounds"/>.
        /// </summary>
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

        /// <summary>
        /// Gets minimum and maximum times in MIDI ticks for the current time range.
        /// </summary>
        /// <param name="time">Time bounds should be calculated relative to.</param>
        /// <param name="tempoMap">Tempo map used to calculate bounds.</param>
        /// <returns>Pair where first item is minimum time and the second one is maximum time.</returns>
        public Tuple<long, long> GetBounds(long time, TempoMap tempoMap)
        {
            return Tuple.Create(
                CalculateBoundaryTime(time, LeftSize, MathOperation.Subtract, tempoMap),
                CalculateBoundaryTime(time, RightSize, MathOperation.Add, tempoMap));
        }

        #endregion
    }
}
