using System;
using System.Linq;
using Melanchall.DryWetMidi.Common;
using Melanchall.DryWetMidi.Core;

namespace Melanchall.DryWetMidi.Interaction
{
    internal sealed class BarBeatFractionTimeSpanConverter : ITimeSpanConverter
    {
        #region Constants

        private const double FractionalBeatsEpsilon = 0.000001;

        #endregion

        #region ITimeSpanConverter

        public ITimeSpan ConvertTo(long timeSpan, long time, TempoMap tempoMap)
        {
            var ticksPerQuarterNoteTimeDivision = tempoMap.TimeDivision as TicksPerQuarterNoteTimeDivision;
            if (ticksPerQuarterNoteTimeDivision == null)
                throw new ArgumentException("Time division is not supported for time span conversion.", nameof(tempoMap));

            if (timeSpan == 0)
                return new BarBeatFractionTimeSpan();

            var ticksPerQuarterNote = ticksPerQuarterNoteTimeDivision.TicksPerQuarterNote;
            var endTime = time + timeSpan;

            //

            var timeSignatureLine = tempoMap.TimeSignatureLine;
            var timeSignatureChanges = timeSignatureLine
                .Where(v => v.Time > time && v.Time < endTime)
                .ToList();

            var bars = 0L;

            // Calculate count of complete bars between time signature changes

            for (int i = 0; i < timeSignatureChanges.Count - 1; i++)
            {
                var timeSignatureChange = timeSignatureChanges[i];
                var nextTime = timeSignatureChanges[i + 1].Time;

                var barLength = BarBeatTimeSpanUtilities.GetBarLength(timeSignatureChange.Value, ticksPerQuarterNote);
                bars += (nextTime - timeSignatureChange.Time) / barLength;
            }

            // Calculate components before first time signature change and after last time signature change

            var firstTime = timeSignatureChanges.FirstOrDefault()?.Time ?? time;
            var lastTime = timeSignatureChanges.LastOrDefault()?.Time ?? time;

            var firstTimeSignature = timeSignatureLine.GetValueAtTime(time);
            var lastTimeSignature = timeSignatureLine.GetValueAtTime(lastTime);

            CalculateComponents(
                firstTime - time,
                firstTimeSignature,
                ticksPerQuarterNote,
                out var barsBefore,
                out var beatsBefore,
                out var fractionBefore);

            CalculateComponents(
                time + timeSpan - lastTime,
                lastTimeSignature,
                ticksPerQuarterNote,
                out var barsAfter,
                out var beatsAfter,
                out var fractionAfter);

            bars += barsBefore + barsAfter;

            // Try to complete a bar

            var beats = beatsBefore + beatsAfter;
            if (beats > 0)
            {
                if (beatsBefore > 0 && beats >= firstTimeSignature.Numerator)
                {
                    bars++;
                    beats -= firstTimeSignature.Numerator;
                }
            }

            // Try to complete a beat

            var fraction = fractionBefore + fractionAfter;
            beats += (long)Math.Truncate(fraction);
            fraction -= Math.Truncate(fraction);

            //

            return new BarBeatFractionTimeSpan(bars, beats + fraction);
        }

        public long ConvertFrom(ITimeSpan timeSpan, long time, TempoMap tempoMap)
        {
            var ticksPerQuarterNoteTimeDivision = tempoMap.TimeDivision as TicksPerQuarterNoteTimeDivision;
            if (ticksPerQuarterNoteTimeDivision == null)
                throw new ArgumentException("Time division is not supported for time span conversion.", nameof(tempoMap));

            var barBeatFractionTimeSpan = (BarBeatFractionTimeSpan)timeSpan;
            if (barBeatFractionTimeSpan.Bars == 0 && barBeatFractionTimeSpan.Beats < FractionalBeatsEpsilon)
                return 0;

            var ticksPerQuarterNote = ticksPerQuarterNoteTimeDivision.TicksPerQuarterNote;
            var timeSignatureLine = tempoMap.TimeSignatureLine;

            //

            var fractionalBeats = barBeatFractionTimeSpan.Beats;
            var bars = barBeatFractionTimeSpan.Bars;

            if (bars + fractionalBeats > long.MaxValue)
                throw new InvalidOperationException("Time span is too big.");

            var beats = (long)Math.Truncate(fractionalBeats);
            var fraction = fractionalBeats - Math.Truncate(fractionalBeats);

            var startTimeSignature = timeSignatureLine.GetValueAtTime(time);
            var startBarLength = BarBeatTimeSpanUtilities.GetBarLength(startTimeSignature, ticksPerQuarterNote);
            var startBeatLength = BarBeatTimeSpanUtilities.GetBeatLength(startTimeSignature, ticksPerQuarterNote);

            var totalTicks = bars * startBarLength + beats * startBeatLength + ConvertFractionToTicks(fraction, startBeatLength);
            var timeSignatureChanges = timeSignatureLine.Where(v => v.Time > time && v.Time < time + totalTicks).ToList();

            var lastBarLength = 0L;
            var lastBeatLength = 0L;

            var firstTimeSignatureChange = timeSignatureChanges.FirstOrDefault();
            var lastTimeSignature = firstTimeSignatureChange?.Value ?? startTimeSignature;
            var lastTime = firstTimeSignatureChange?.Time ?? time;

            long barsBefore, beatsBefore;
            double fractionBefore;
            CalculateComponents(
                lastTime - time,
                startTimeSignature,
                ticksPerQuarterNote,
                out barsBefore,
                out beatsBefore,
                out fractionBefore);

            bars -= barsBefore;

            // Balance bars

            if (bars > 0)
            {
                foreach (var timeSignatureChange in timeSignatureLine.Where(v => v.Time > lastTime).ToList())
                {
                    var deltaTime = timeSignatureChange.Time - lastTime;

                    lastBarLength = BarBeatTimeSpanUtilities.GetBarLength(lastTimeSignature, ticksPerQuarterNote);
                    lastBeatLength = BarBeatTimeSpanUtilities.GetBeatLength(lastTimeSignature, ticksPerQuarterNote);

                    var currentBars = Math.Min(deltaTime / lastBarLength, bars);
                    bars -= currentBars;
                    lastTime += MathUtilities.RoundToLong(currentBars * lastBarLength);

                    if (bars == 0)
                        break;

                    lastTimeSignature = timeSignatureChange.Value;
                }

                if (bars > 0)
                {
                    lastBarLength = BarBeatTimeSpanUtilities.GetBarLength(lastTimeSignature, ticksPerQuarterNote);
                    lastBeatLength = BarBeatTimeSpanUtilities.GetBeatLength(lastTimeSignature, ticksPerQuarterNote);
                    lastTime += MathUtilities.RoundToLong(bars * lastBarLength);
                }
            }

            if (beats == beatsBefore && Math.Abs(fraction - fractionBefore) < FractionalBeatsEpsilon)
                return lastTime - time;

            // Balance beats

            if (beatsBefore > beats && lastBarLength > 0)
            {
                lastTime += -lastBarLength + (startTimeSignature.Numerator - beatsBefore) * lastBeatLength;
                beatsBefore = 0;
            }

            if (beatsBefore < beats)
            {
                lastBeatLength = BarBeatTimeSpanUtilities.GetBeatLength(timeSignatureLine.GetValueAtTime(lastTime), ticksPerQuarterNote);
                lastTime += MathUtilities.RoundToLong((beats - beatsBefore) * lastBeatLength);
            }

            // Balance cents

            if (fractionBefore > fraction && lastBeatLength > 0)
                lastTime += MathUtilities.RoundToLong(-lastBeatLength + ConvertFractionToTicks(fraction + 1.0 - fractionBefore, lastBeatLength));

            if (fractionBefore < fraction)
            {
                if (lastBeatLength == 0)
                    lastBeatLength = BarBeatTimeSpanUtilities.GetBeatLength(timeSignatureLine.GetValueAtTime(lastTime), ticksPerQuarterNote);

                lastTime += MathUtilities.RoundToLong(ConvertFractionToTicks(fraction - fractionBefore, lastBeatLength));
            }

            //

            return lastTime - time;
        }

        #endregion

        #region Methods

        private static void CalculateComponents(
            long totalTicks,
            TimeSignature timeSignature,
            short ticksPerQuarterNote,
            out long bars,
            out long beats,
            out double fraction)
        {
            var barLength = BarBeatTimeSpanUtilities.GetBarLength(timeSignature, ticksPerQuarterNote);
            bars = totalTicks / barLength;
            var ticks = totalTicks % barLength;

            var beatLength = BarBeatTimeSpanUtilities.GetBeatLength(timeSignature, ticksPerQuarterNote);
            beats = ticks / beatLength;
            ticks = ticks % beatLength;

            fraction = (double)ticks / beatLength;
        }

        private static long ConvertFractionToTicks(double fraction, long beatLength)
        {
            return MathUtilities.RoundToLong(beatLength * fraction);
        }

        #endregion
    }
}
