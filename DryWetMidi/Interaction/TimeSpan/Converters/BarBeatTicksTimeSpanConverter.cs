using System;
using System.Linq;
using Melanchall.DryWetMidi.Common;
using Melanchall.DryWetMidi.Core;

namespace Melanchall.DryWetMidi.Interaction
{
    internal sealed class BarBeatTicksTimeSpanConverter : ITimeSpanConverter
    {
        #region ITimeSpanConverter

        public ITimeSpan ConvertTo(long timeSpan, long time, TempoMap tempoMap)
        {
            var ticksPerQuarterNoteTimeDivision = tempoMap.TimeDivision as TicksPerQuarterNoteTimeDivision;
            if (ticksPerQuarterNoteTimeDivision == null)
                throw new ArgumentException("Time division is not supported for time span conversion.", nameof(tempoMap));

            if (timeSpan == 0)
                return new BarBeatTicksTimeSpan();

            var ticksPerQuarterNote = ticksPerQuarterNoteTimeDivision.TicksPerQuarterNote;
            var endTime = time + timeSpan;

            //

            var timeSignatureLine = tempoMap.TimeSignatureLine;

            var bars = 0d;
            var beats = 0d;
            var ticks = 0L;

            var timeSignatureChanges = timeSignatureLine.GetValueChanges(time, endTime);
            if (timeSignatureChanges.Count == 0)
            {
                var timeSignatureAtTime = timeSignatureLine.GetValueAtTime(time);
                CalculateComponents(
                    endTime - time,
                    timeSignatureAtTime,
                    ticksPerQuarterNote,
                    out bars,
                    out beats,
                    out ticks);

                return new BarBeatTicksTimeSpan(bars, beats, ticks);
            }

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
                out var ticksBefore);

            CalculateComponents(
                endTime - lastTime,
                lastTimeSignature,
                ticksPerQuarterNote,
                out var barsAfter,
                out var beatsAfter,
                out var ticksAfter);

            bars += barsBefore + barsAfter;

            // Try to complete a bar

            beats = beatsBefore + beatsAfter;
            if (beats > 0 && beatsBefore > 0 && beats >= firstTimeSignature.Numerator)
            {
                bars++;
                beats -= firstTimeSignature.Numerator;
            }

            // Try to complete a beat

            ticks = ticksBefore + ticksAfter;
            if (ticks > 0)
            {
                var beatLength = BarBeatTimeSpanUtilities.GetBeatLength(firstTimeSignature, ticksPerQuarterNote);
                if (ticksBefore > 0 && ticks >= beatLength)
                {
                    beats++;
                    ticks -= beatLength;
                }
            }

            //

            return new BarBeatTicksTimeSpan(bars, beats, ticks);
        }

        public long ConvertFrom(ITimeSpan timeSpan, long time, TempoMap tempoMap)
        {
            var ticksPerQuarterNoteTimeDivision = tempoMap.TimeDivision as TicksPerQuarterNoteTimeDivision;
            if (ticksPerQuarterNoteTimeDivision == null)
                throw new ArgumentException("Time division is not supported for time span conversion.", nameof(tempoMap));

            var barBeatTicksTimeSpan = (BarBeatTicksTimeSpan)timeSpan;
            if (barBeatTicksTimeSpan.Bars == 0 && barBeatTicksTimeSpan.Beats == 0 && barBeatTicksTimeSpan.Ticks == 0)
                return 0;

            var ticksPerQuarterNote = ticksPerQuarterNoteTimeDivision.TicksPerQuarterNote;
            var timeSignatureLine = tempoMap.TimeSignatureLine;

            //

            var bars = barBeatTicksTimeSpan.Bars;
            var beats = barBeatTicksTimeSpan.Beats;
            var ticks = barBeatTicksTimeSpan.Ticks;

            if (bars + beats + ticks > long.MaxValue)
                throw new InvalidOperationException("Time span is too big.");

            var startTimeSignature = timeSignatureLine.GetValueAtTime(time);
            var startBarLength = BarBeatTimeSpanUtilities.GetBarLength(startTimeSignature, ticksPerQuarterNote);
            var startBeatLength = BarBeatTimeSpanUtilities.GetBeatLength(startTimeSignature, ticksPerQuarterNote);

            var totalTicks = bars * startBarLength + beats * startBeatLength + ticks;
            var timeSignatureChanges = timeSignatureLine.Where(v => v.Time > time && v.Time < time + totalTicks).ToList();

            var lastBarLength = 0L;
            var lastBeatLength = 0L;

            var firstTimeSignatureChange = timeSignatureChanges.FirstOrDefault();
            var lastTimeSignature = firstTimeSignatureChange?.Value ?? startTimeSignature;
            var lastTime = firstTimeSignatureChange?.Time ?? time;

            CalculateComponents(
                lastTime - time,
                startTimeSignature,
                ticksPerQuarterNote,
                out var barsBefore,
                out var beatsBefore,
                out var ticksBefore);

            bars -= barsBefore;

            // Balance bars

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

            if (beats == beatsBefore && ticks == ticksBefore)
                return MathUtilities.RoundToLong(lastTime - time);

            // Balance beats

            if (beatsBefore > beats && lastBarLength > 0)
            {
                lastTime += MathUtilities.RoundToLong(-lastBarLength + (startTimeSignature.Numerator - beatsBefore) * lastBeatLength);
                beatsBefore = 0;
            }

            if (beatsBefore < beats)
            {
                lastBeatLength = BarBeatTimeSpanUtilities.GetBeatLength(timeSignatureLine.GetValueAtTime(MathUtilities.RoundToLong(lastTime)), ticksPerQuarterNote);
                lastTime += MathUtilities.RoundToLong((beats - beatsBefore) * lastBeatLength);
            }

            // Balance ticks

            if (ticksBefore > ticks && lastBeatLength > 0)
            {
                lastTime += MathUtilities.RoundToLong(-lastBeatLength + startBeatLength - ticksBefore);
                ticksBefore = 0;
            }

            if (ticksBefore < ticks)
                lastTime += MathUtilities.RoundToLong(ticks - ticksBefore);

            //

            return MathUtilities.RoundToLong(lastTime - time);
        }

        #endregion

        #region Methods

        private static void CalculateComponents(
            long totalTicks,
            TimeSignature timeSignature,
            short ticksPerQuarterNote,
            out double bars,
            out double beats,
            out long ticks)
        {
            var barLength = BarBeatTimeSpanUtilities.GetBarLength(timeSignature, ticksPerQuarterNote);
            bars = totalTicks / barLength;
            ticks = totalTicks % barLength;

            var beatLength = BarBeatTimeSpanUtilities.GetBeatLength(timeSignature, ticksPerQuarterNote);
            beats = ticks / beatLength;
            ticks = ticks % beatLength;
        }

        #endregion
    }
}
