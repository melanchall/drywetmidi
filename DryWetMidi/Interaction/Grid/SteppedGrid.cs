using System;
using System.Collections.Generic;
using System.Linq;
using Melanchall.DryWetMidi.Common;

namespace Melanchall.DryWetMidi.Interaction
{
    /// <summary>
    /// Grid where points in time are distributed with the specified step or
    /// collection of steps.
    /// </summary>
    /// <remarks>
    /// <para>
    /// <see cref="GetTimes"/> returns absolute positions in MIDI ticks, not elapsed time.
    /// With a <see cref="Melanchall.DryWetMidi.Core.TicksPerQuarterNoteTimeDivision"/>,
    /// the number of ticks per quarter note is fixed. A beat-based step such as
    /// <c>new BarBeatTicksTimeSpan(0, 1)</c> therefore does not produce different tick
    /// distances when the tempo changes.
    /// </para>
    /// <para>
    /// To draw beat grid lines on an elapsed-time timeline, convert each position to
    /// <see cref="MetricTimeSpan"/> using
    /// <see cref="TimeConverter.ConvertTo{TTimeSpan}(long, TempoMap)"/> and the same tempo map.
    /// Subtract adjacent converted positions to get cell widths that reflect tempo changes,
    /// including changes within a beat. Do not convert a tick difference as an absolute time.
    /// A metric step instead produces equal elapsed-time intervals, not beat-aligned grid lines.
    /// </para>
    /// </remarks>
    /// <example>
    /// Calculate the elapsed time between beats of a MIDI file:
    /// <code language="csharp">
    /// var tempoMap = midiFile.GetTempoMap();
    /// var duration = midiFile.GetDuration&lt;MidiTimeSpan&gt;().TimeSpan;
    /// var grid = new SteppedGrid(new BarBeatTicksTimeSpan(0, 1));
    /// long previousMicroseconds = 0;
    ///
    /// foreach (var tick in grid.GetTimes(tempoMap))
    /// {
    ///     if (tick &gt; duration)
    ///         break;
    ///
    ///     var microseconds = TimeConverter.ConvertTo&lt;MetricTimeSpan&gt;(tick, tempoMap)
    ///         .TotalMicroseconds;
    ///     Console.WriteLine(microseconds - previousMicroseconds);
    ///     previousMicroseconds = microseconds;
    /// }
    /// </code>
    /// The first position is zero, so the first difference is zero. Scale the subsequent
    /// differences by pixels per microsecond to obtain cell widths.
    /// </example>
    public sealed class SteppedGrid : IGrid
    {
        #region Constructor

        /// <summary>
        /// Initializes a new instance of the <see cref="SteppedGrid"/> with the specified
        /// step so all grid's times will be distributed with equal distance between adjacent ones
        /// starting from zero.
        /// </summary>
        /// <param name="step">Distance between adjacent times.</param>
        /// <exception cref="ArgumentNullException"><paramref name="step"/> is <c>null</c>.</exception>
        public SteppedGrid(ITimeSpan step)
            : this((MidiTimeSpan)0, step)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="SteppedGrid"/> with the specified
        /// start time and step so all grid's times will be distributed with equal distance
        /// between adjacent ones starting from the specified start time.
        /// </summary>
        /// <param name="start">Start time of the grid.</param>
        /// <param name="step">Distance between adjacent times.</param>
        /// <exception cref="ArgumentNullException">
        /// <para>One of the following errors occurred:</para>
        /// <list type="bullet">
        /// <item>
        /// <description><paramref name="start"/> is <c>null</c>.</description>
        /// </item>
        /// <item>
        /// <description><paramref name="step"/> is <c>null</c>.</description>
        /// </item>
        /// </list>
        /// </exception>
        public SteppedGrid(ITimeSpan start, ITimeSpan step)
        {
            ThrowIfArgument.IsNull(nameof(start), start);
            ThrowIfArgument.IsNull(nameof(step), step);

            Start = start;
            Steps = new[] { step };
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="SteppedGrid"/> with the specified
        /// steps so all grid's times will be stepped according to those steps.
        /// </summary>
        /// <remarks>
        /// Grid's times will be distributed according to provided steps. So distance between first
        /// adjacent times will be equal to first step, distance between second adjacent times will
        /// be equal to second step and so on. When the last step is reached, steps will go from the first one.
        /// </remarks>
        /// <param name="steps">Collection of grid's steps.</param>
        /// <exception cref="ArgumentNullException"><paramref name="steps"/> is <c>null</c>.</exception>
        /// <exception cref="ArgumentException"><paramref name="steps"/> contains <c>null</c>.</exception>
        public SteppedGrid(IEnumerable<ITimeSpan> steps)
            : this((MidiTimeSpan)0, steps)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="SteppedGrid"/> with the specified
        /// start time and steps so all grid's times will be stepped according to those steps.
        /// </summary>
        /// <remarks>
        /// Grid's times will be distributed according to provided steps. So distance between first
        /// adjacent times will be equal to first step, distance between second adjacent times will
        /// be equal to second step and so on. When the last step is reached, steps will go from the first one.
        /// </remarks>
        /// <param name="start">Start time of the grid.</param>
        /// <param name="steps">Collection of grid's steps.</param>
        /// <exception cref="ArgumentNullException">
        /// <para>One of the following errors occurred:</para>
        /// <list type="bullet">
        /// <item>
        /// <description><paramref name="start"/> is <c>null</c>.</description>
        /// </item>
        /// <item>
        /// <description><paramref name="steps"/> is <c>null</c>.</description>
        /// </item>
        /// </list>
        /// </exception>
        /// <exception cref="ArgumentException"><paramref name="steps"/> contains <c>null</c>.</exception>
        public SteppedGrid(ITimeSpan start, IEnumerable<ITimeSpan> steps)
        {
            ThrowIfArgument.IsNull(nameof(start), start);
            ThrowIfArgument.IsNull(nameof(steps), steps);
            ThrowIfArgument.ContainsNull(nameof(steps), steps);

            Start = start;
            Steps = steps;
        }

        #endregion

        #region Properties

        /// <summary>
        /// Start time of the current grid.
        /// </summary>
        public ITimeSpan Start { get; }

        /// <summary>
        /// Steps of the current grid.
        /// </summary>
        public IEnumerable<ITimeSpan> Steps { get; }

        #endregion

        #region IGrid

        /// <inheritdoc/>
        public IEnumerable<long> GetTimes(TempoMap tempoMap)
        {
            ThrowIfArgument.IsNull(nameof(tempoMap), tempoMap);

            if (!Steps.Any())
                yield break;

            var time = TimeConverter.ConvertFrom(Start, tempoMap);
            yield return time;

            while (true)
            {
                foreach (var step in Steps)
                {
                    time += LengthConverter.ConvertFrom(step, time, tempoMap);
                    yield return time;
                }
            }
        }

        #endregion
    }
}
