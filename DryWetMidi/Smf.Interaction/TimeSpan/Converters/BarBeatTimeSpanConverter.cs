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

                ConvertComponents(timeSignatureChange,
                                  time,
                                  GetBarLength(timeSignature, ticksPerQuarterNote),
                                  ref bars,
                                  ref ticksToCompleteBar,
                                  ref remainingTicks);

                time = timeSignatureChange.Time;

                previousTimeSignature = timeSignature;
                timeSignature = timeSignatureChange.Value;
            }

            var beatLength = GetBeatLength(timeSignature, ticksPerQuarterNote);
            var beats = Math.DivRem(remainingTicks, beatLength, out var ticks);

            return new BarBeatTimeSpan((int)bars, (int)beats/*, ticks*/);
        }

        public long ConvertFrom(ITimeSpan timeSpan, long time, TempoMap tempoMap)
        {
            var ticksPerQuarterNoteTimeDivision = tempoMap.TimeDivision as TicksPerQuarterNoteTimeDivision;
            if (ticksPerQuarterNoteTimeDivision == null)
                throw new ArgumentException("Time division is not supported for time span conversion.", nameof(tempoMap));

            var barBeatTimeSpan = (BarBeatTimeSpan)timeSpan;

            var ticksPerQuarterNote = ticksPerQuarterNoteTimeDivision.TicksPerQuarterNote;
            var timeSignatureLine = tempoMap.TimeSignature;

            var startTime = time;

            time = StepComponents(time, barBeatTimeSpan.Bars, timeSignatureLine, ticksPerQuarterNote, GetBarLength);
            time = StepComponents(time, barBeatTimeSpan.Beats, timeSignatureLine, ticksPerQuarterNote, GetBeatLength);

            return time - startTime;
        }

        #endregion

        #region Methods

        // TODO: improve accuracy (via rational fraction)
        private static long StepComponents(long time,
                                           long componentsCount,
                                           ValueLine<TimeSignature> timeSignatureLine,
                                           short ticksPerQuarterNote,
                                           Func<TimeSignature, short, int> getComponentLength)
        {
            var ticksToCompleteComponent = 0L;
            var remainingTicks = 0L;
            var components = 0L;

            var timeSignature = timeSignatureLine.AtTime(time);
            var previousTimeSignature = timeSignature;

            while (components < componentsCount)
            {
                ticksToCompleteComponent = (long)Math.Round(ticksToCompleteComponent * previousTimeSignature.Denominator / (double)timeSignature.Denominator);

                var componentLength = getComponentLength(timeSignature, ticksPerQuarterNote);
                var remainingComponents = componentsCount - components;
                var endTime = time + ticksToCompleteComponent + (remainingComponents - Math.Sign(ticksToCompleteComponent)) * componentLength;

                var timeSignatureChange = timeSignatureLine.Values.FirstOrDefault(v => v.Time > time && v.Time < endTime)
                    ?? new ValueChange<TimeSignature>(endTime, timeSignature);

                ConvertComponents(timeSignatureChange,
                                  time,
                                  componentLength,
                                  ref components,
                                  ref ticksToCompleteComponent,
                                  ref remainingTicks);

                time = timeSignatureChange.Time;

                previousTimeSignature = timeSignature;
                timeSignature = timeSignatureChange.Value;
            }

            return time;
        }

        // TODO: improve accuracy (via rational fraction)
        private static void ConvertComponents(ValueChange<TimeSignature> timeSignatureChange,
                                              long time,
                                              long componentLength,
                                              ref long components,
                                              ref long ticksToCompleteComponents,
                                              ref long remainingTicks)
        {
            var deltaTime = timeSignatureChange.Time - time;
            deltaTime -= ticksToCompleteComponents;

            if (deltaTime >= 0)
            {
                if (ticksToCompleteComponents > 0)
                    components++;

                ticksToCompleteComponents = 0;
            }

            if (deltaTime < 0)
            {
                // TODO: check this scenario!
                ticksToCompleteComponents = -deltaTime;
                return;
            }

            components += deltaTime / componentLength;
            remainingTicks = deltaTime % componentLength;
            if (remainingTicks > 0)
                ticksToCompleteComponents = componentLength - remainingTicks;
        }

        // TODO: improve accuracy (via rational fraction)
        private static int GetBarLength(TimeSignature timeSignature, short ticksPerQuarterNote)
        {
            var beatLength = GetBeatLength(timeSignature, ticksPerQuarterNote);
            return timeSignature.Numerator * beatLength;
        }

        // TODO: improve accuracy (via rational fraction)
        private static int GetBeatLength(TimeSignature timeSignature, short ticksPerQuarterNote)
        {
            return 4 * ticksPerQuarterNote / timeSignature.Denominator;
        }

        #endregion
    }
}
