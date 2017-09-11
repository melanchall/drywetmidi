using Melanchall.DryWetMidi.Common;
using System;
using System.Linq;

namespace Melanchall.DryWetMidi.Smf.Interaction
{
    public static class TimeUtilities
    {
        #region Constants

        private static readonly Func<string, Tuple<ParsingResult, ITime>>[] Parsers = new Func<string, Tuple<ParsingResult, ITime>>[]
        {
            input => Tuple.Create(MathTimeParser.TryParse(input, out var time), (ITime)time),
            input => Tuple.Create(MidiTimeParser.TryParse(input, out var time), (ITime)time),
            input => Tuple.Create(MetricTimeParser.TryParse(input, out var time), (ITime)time),
            input => Tuple.Create(MusicalTimeParser.TryParse(input, out var time), (ITime)time),
        };

        private static readonly Func<ITime, ITime, long, long, TempoMap, ILength>[] Subtracters = new Func<ITime, ITime, long, long, TempoMap, ILength>[]
        {
            TryToSubtractFromMetricTime,
            TryToSubtractFromMusicalTime,
            TryToSubtractFromMidiTime,
            TryToSubtractFromMathTime
        };

        #endregion

        #region Methods

        public static ITime Add(this ITime time, ILength length)
        {
            ThrowIfArgument.IsNull(nameof(time), time);
            ThrowIfArgument.IsNull(nameof(length), length);

            return new MathTime(time, length, MathOperation.Add);
        }

        public static ITime Subtract(this ITime time, ILength length)
        {
            ThrowIfArgument.IsNull(nameof(time), time);
            ThrowIfArgument.IsNull(nameof(length), length);

            return new MathTime(time, length, MathOperation.Subtract);
        }

        public static ILength Subtract(this ITime minuend, ITime subtrahend, TempoMap tempoMap)
        {
            ThrowIfArgument.IsNull(nameof(minuend), minuend);
            ThrowIfArgument.IsNull(nameof(subtrahend), subtrahend);
            ThrowIfArgument.IsNull(nameof(tempoMap), tempoMap);

            var convertedMinuend = TimeConverter.ConvertFrom(minuend, tempoMap);
            var convertedSubtrahend = TimeConverter.ConvertFrom(subtrahend, tempoMap);

            var length = convertedMinuend - convertedSubtrahend;
            if (length < 0)
                throw new ArgumentException("First time is less than the second one.", nameof(minuend));

            var result = Subtracters.Select(s => s(minuend, subtrahend, convertedSubtrahend, length, tempoMap))
                                    .FirstOrDefault(l => l != null);

            return result ?? throw new ArgumentException("First time is of unknown type.", nameof(minuend));
        }

        public static bool TryParse(string input, out ITime time)
        {
            time = null;

            if (MathTime.TryParse(input, out var mathTime))
                time = mathTime;
            else if (MidiTime.TryParse(input, out var midiTime))
                time = midiTime;
            else if (MetricTime.TryParse(input, out var metricTime))
                time = metricTime;
            else if (MusicalTime.TryParse(input, out var musicalTime))
                time = musicalTime;

            return time != null;
        }

        public static ITime Parse(string input)
        {
            ThrowIfArgument.IsNullOrWhiteSpaceString(nameof(input), input, "Input string");

            foreach (var parser in Parsers)
            {
                var parsingResult = parser(input);

                var result = parsingResult.Item1;
                var time = parsingResult.Item2;

                if (result.Status == ParsingStatus.Parsed)
                    return time;
                else if (result.Status == ParsingStatus.FormatError)
                    throw result.Exception;
            }

            throw new FormatException("Time has unknown format.");
        }

        private static ILength TryToSubtractFromMetricTime(ITime minuend, ITime subtrahend, long time, long length, TempoMap tempoMap)
        {
            var metricMinuend = minuend as MetricTime;
            if (metricMinuend == null)
                return null;

            var metricSubtrahend = subtrahend as MetricTime;
            return metricSubtrahend != null
                ? new MetricLength(metricMinuend - metricSubtrahend)
                : LengthConverter.ConvertTo<MetricLength>(length, time, tempoMap);
        }

        private static ILength TryToSubtractFromMusicalTime(ITime minuend, ITime subtrahend, long time, long length, TempoMap tempoMap)
        {
            var musicalMinuend = minuend as MusicalTime;
            if (musicalMinuend == null)
                return null;

            var musicalSubtrahend = subtrahend as MusicalTime;
            if (musicalSubtrahend != null)
            {
                if (musicalMinuend.Bars == 0 && musicalMinuend.Beats == 0 &&
                    musicalSubtrahend.Bars == 0 && musicalSubtrahend.Beats == 0)
                    return new MusicalLength(musicalMinuend.Fraction - musicalSubtrahend.Fraction);
            }

            return LengthConverter.ConvertTo<MusicalLength>(length, time, tempoMap);
        }

        private static ILength TryToSubtractFromMidiTime(ITime minuend, ITime subtrahend, long time, long length, TempoMap tempoMap)
        {
            var midiMinuend = minuend as MidiTime;
            if (midiMinuend == null)
                return null;

            var midiSubtrahend = subtrahend as MidiTime;
            return midiSubtrahend != null
                ? new MidiLength(midiMinuend - midiSubtrahend)
                : LengthConverter.ConvertTo<MidiLength>(length, time, tempoMap);
        }

        private static ILength TryToSubtractFromMathTime(ITime minuend, ITime subtrahend, long time, long length, TempoMap tempoMap)
        {
            var mathMinuend = minuend as MathTime;
            if (mathMinuend == null)
                return null;

            return new MathLength(mathMinuend.Time.Subtract(subtrahend, tempoMap),
                                  mathMinuend.Offset,
                                  mathMinuend.Operation);
        }

        #endregion
    }
}
