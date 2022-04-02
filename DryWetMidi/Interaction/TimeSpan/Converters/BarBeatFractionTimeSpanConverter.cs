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

                var barLength = BarBeatUtilities.GetBarLength(timeSignatureChange.Value, ticksPerQuarterNote);
                bars += (nextTime - timeSignatureChange.Time) / barLength;
            }

            // Calculate components before first time signature change and after last time signature change

            var firstTime = timeSignatureChanges.FirstOrDefault()?.Time ?? time;
            var lastTime = timeSignatureChanges.LastOrDefault()?.Time ?? time;

            var firstTimeSignature = timeSignatureLine.GetValueAtTime(time);
            var lastTimeSignature = timeSignatureLine.GetValueAtTime(lastTime);

            long barsBefore, beatsBefore;
            double fractionBefore;
            CalculateComponents(firstTime - time,
                                firstTimeSignature,
                                ticksPerQuarterNote,
                                out barsBefore,
                                out beatsBefore,
                                out fractionBefore);

            long barsAfter, beatsAfter;
            double fractionAfter;
            CalculateComponents(time + timeSpan - lastTime,
                                lastTimeSignature,
                                ticksPerQuarterNote,
                                out barsAfter,
                                out beatsAfter,
                                out fractionAfter);

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
            var startBarLength = BarBeatUtilities.GetBarLength(startTimeSignature, ticksPerQuarterNote);
            var startBeatLength = BarBeatUtilities.GetBeatLength(startTimeSignature, ticksPerQuarterNote);

            var totalTicks = bars * startBarLength + beats * startBeatLength + ConvertFractionToTicks(fraction, startBeatLength);
            var timeSignatureChanges = timeSignatureLine.Where(v => v.Time > time && v.Time < time + totalTicks).ToList();

            var lastBarLength = 0L;
            var lastBeatLength = 0L;

            var firstTimeSignatureChange = timeSignatureChanges.FirstOrDefault();
            var lastTimeSignature = firstTimeSignatureChange?.Value ?? startTimeSignature;
            var lastTime = firstTimeSignatureChange?.Time ?? time;

            long barsBefore, beatsBefore;
            double fractionBefore;
            CalculateComponents(lastTime - time,
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

                    lastBarLength = BarBeatUtilities.GetBarLength(lastTimeSignature, ticksPerQuarterNote);
                    lastBeatLength = BarBeatUtilities.GetBeatLength(lastTimeSignature, ticksPerQuarterNote);

                    var currentBars = Math.Min(deltaTime / lastBarLength, bars);
                    bars -= currentBars;
                    lastTime += currentBars * lastBarLength;

                    if (bars == 0)
                        break;

                    lastTimeSignature = timeSignatureChange.Value;
                }

                if (bars > 0)
                {
                    lastBarLength = BarBeatUtilities.GetBarLength(lastTimeSignature, ticksPerQuarterNote);
                    lastBeatLength = BarBeatUtilities.GetBeatLength(lastTimeSignature, ticksPerQuarterNote);
                    lastTime += bars * lastBarLength;
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
                lastBeatLength = BarBeatUtilities.GetBeatLength(timeSignatureLine.GetValueAtTime(lastTime), ticksPerQuarterNote);
                lastTime += (beats - beatsBefore) * lastBeatLength;
            }

            // Balance cents

            if (fractionBefore > fraction && lastBeatLength > 0)
                lastTime += -lastBeatLength + ConvertFractionToTicks(fraction + 1.0 - fractionBefore, lastBeatLength);

            if (fractionBefore < fraction)
            {
                if (lastBeatLength == 0)
                    lastBeatLength = BarBeatUtilities.GetBeatLength(timeSignatureLine.GetValueAtTime(lastTime), ticksPerQuarterNote);

                lastTime += ConvertFractionToTicks(fraction - fractionBefore, lastBeatLength);
            }

            //

            return lastTime - time;
        }

        #endregion

        #region Methods

        private static void CalculateComponents(long totalTicks,
                                                TimeSignature timeSignature,
                                                short ticksPerQuarterNote,
                                                out long bars,
                                                out long beats,
                                                out double fraction)
        {
            long ticks;

            var barLength = BarBeatUtilities.GetBarLength(timeSignature, ticksPerQuarterNote);
            bars = Math.DivRem(totalTicks, barLength, out ticks);

            var beatLength = BarBeatUtilities.GetBeatLength(timeSignature, ticksPerQuarterNote);
            beats = Math.DivRem(ticks, beatLength, out ticks);

            fraction = (double)ticks / beatLength;
        }

        private static long ConvertFractionToTicks(double fraction, long beatLength)
        {
            return MathUtilities.RoundToLong(beatLength * fraction);
        }

        #endregion
    }
}
