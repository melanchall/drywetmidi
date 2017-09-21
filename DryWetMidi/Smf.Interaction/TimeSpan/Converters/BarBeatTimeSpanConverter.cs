using System;
using System.Linq;

namespace Melanchall.DryWetMidi.Smf.Interaction
{
    internal sealed class BarBeatTimeSpanConverter : ITimeSpanConverter
    {
        #region ITimeSpanConverter

        public ITimeSpan ConvertTo(long timeSpan, long time, TempoMap tempoMap)
        {
            var ticksPerQuarterNoteTimeDivision = tempoMap.TimeDivision as TicksPerQuarterNoteTimeDivision;
            if (ticksPerQuarterNoteTimeDivision == null)
                throw new ArgumentException("Time division is not supported for time span conversion.", nameof(tempoMap));

            var ticksPerQuarterNote = ticksPerQuarterNoteTimeDivision.TicksPerQuarterNote;
            var endTime = time + timeSpan;

            var timeSignatureLine = tempoMap.TimeSignature;
            var lastTimeSignature = timeSignatureLine.Values.LastOrDefault(v => v.Time < endTime)?.Value ?? TimeSignature.Default;
            var timeSignatureChanges = timeSignatureLine
                .Values
                .SkipWhile(v => v.Time < time)
                .TakeWhile(v => v.Time < endTime)
                .Concat(new[] { new ValueChange<TimeSignature>(endTime, lastTimeSignature) })
                .ToList();

            var timeSignature = timeSignatureLine.AtTime(time);
            var previousTimeSignature = timeSignature;
            var ticksToCompleteBar = 0L;
            var remainingTicks = 0L;

            var bars = 0L;

            foreach (var timeSignatureChange in timeSignatureChanges)
            {
                ticksToCompleteBar = (long)Math.Round(ticksToCompleteBar * previousTimeSignature.Denominator / (double)timeSignature.Denominator);

                var deltaTime = timeSignatureChange.Time - time;
                deltaTime -= ticksToCompleteBar;

                if (deltaTime >= 0)
                {
                    if (ticksToCompleteBar > 0)
                        bars++;

                    ticksToCompleteBar = 0;
                }

                if (deltaTime < 0)
                {
                    ticksToCompleteBar = -deltaTime;
                }
                else
                {
                    var barLength = GetBarLength(timeSignature, ticksPerQuarterNote);
                    bars += deltaTime / barLength;
                    remainingTicks = deltaTime % barLength;
                    if (remainingTicks > 0)
                        ticksToCompleteBar = barLength - remainingTicks;
                }

                //

                previousTimeSignature = timeSignature;
                timeSignature = timeSignatureChange.Value;
                time = timeSignatureChange.Time;
            }

            var beatLength = GetBeatLength(timeSignature, ticksPerQuarterNote);
            var beats = remainingTicks / beatLength;
            var ticks = remainingTicks % beatLength;

            return new BarBeatTimeSpan((int)bars, (int)beats/*, ticks*/);
        }

        public long ConvertFrom(ITimeSpan timeSpan, long time, TempoMap tempoMap)
        {
            var ticksPerQuarterNoteTimeDivision = tempoMap.TimeDivision as TicksPerQuarterNoteTimeDivision;
            if (ticksPerQuarterNoteTimeDivision == null)
                throw new ArgumentException("Time division is not supported for time span conversion.", nameof(tempoMap));

            var ticksPerQuarterNote = ticksPerQuarterNoteTimeDivision.TicksPerQuarterNote;

            var barBeatTimeSpan = (BarBeatTimeSpan)timeSpan;

            return 0;
        }

        #endregion

        #region Methods

        private static int GetBarsCount(long time, TimeSignature timeSignature, short ticksPerQuarterNote)
        {
            if (time == 0)
                return 0;

            var barLength = GetBarLength(timeSignature, ticksPerQuarterNote);
            return (int)(time / barLength);
        }

        private static int GetBarLength(TimeSignature timeSignature, short ticksPerQuarterNote)
        {
            var beatLength = GetBeatLength(timeSignature, ticksPerQuarterNote);
            return timeSignature.Numerator * beatLength;
        }

        private static int GetBeatLength(TimeSignature timeSignature, short ticksPerQuarterNote)
        {
            return 4 * ticksPerQuarterNote / timeSignature.Denominator;
        }

        #endregion
    }
}
