using System;
using System.Collections.Generic;

namespace Melanchall.DryWetMidi.Interaction
{
    internal sealed class MathTimeSpanConverter : ITimeSpanConverter
    {
        #region Constants

        private static readonly Dictionary<TimeSpanMode, Func<MathTimeSpan, long, TempoMap, long>> Converters =
            new Dictionary<TimeSpanMode, Func<MathTimeSpan, long, TempoMap, long>>
            {
                [TimeSpanMode.TimeTime] = ConvertFromTimeTime,
                [TimeSpanMode.TimeLength] = ConvertFromTimeLength,
                [TimeSpanMode.LengthLength] = ConvertFromLengthLength
            };

        #endregion

        #region ITimeSpanConverter

        public ITimeSpan ConvertTo(long timeSpan, long time, TempoMap tempoMap)
        {
            throw new NotSupportedException($"Conversion to the {nameof(MathTimeSpan)} is not supported.");
        }

        public long ConvertFrom(ITimeSpan timeSpan, long time, TempoMap tempoMap)
        {
            var mathTimeSpan = (MathTimeSpan)timeSpan;

            Func<MathTimeSpan, long, TempoMap, long> converter;
            if (Converters.TryGetValue(mathTimeSpan.Mode, out converter))
                return converter(mathTimeSpan, time, tempoMap);
            else
                throw new ArgumentException($"{mathTimeSpan.Mode} mode is not supported by the converter.", nameof(timeSpan));
        }

        #endregion

        #region Methods

        private static long ConvertFromLengthLength(MathTimeSpan mathTimeSpan, long time, TempoMap tempoMap)
        {
            var convertedTimeSpan1 = LengthConverter.ConvertFrom(mathTimeSpan.TimeSpan1, time, tempoMap);
            var endTime1 = time + convertedTimeSpan1;

            switch (mathTimeSpan.Operation)
            {
                case MathOperation.Add:
                    return convertedTimeSpan1 + LengthConverter.ConvertFrom(mathTimeSpan.TimeSpan2,
                                                                            endTime1,
                                                                            tempoMap);

                case MathOperation.Subtract:
                    return convertedTimeSpan1 - LengthConverter.ConvertFrom(mathTimeSpan.TimeSpan2,
                                                                            endTime1,
                                                                            tempoMap.Flip(endTime1));

                default:
                    throw new ArgumentException($"{mathTimeSpan.Operation} is not supported by the converter.", nameof(mathTimeSpan));
            }
        }

        private static long ConvertFromTimeLength(MathTimeSpan mathTimeSpan, long time, TempoMap tempoMap)
        {
            var convertedTimeSpan1 = TimeConverter.ConvertFrom(mathTimeSpan.TimeSpan1, tempoMap);

            switch (mathTimeSpan.Operation)
            {
                case MathOperation.Add:
                    return convertedTimeSpan1 + LengthConverter.ConvertFrom(mathTimeSpan.TimeSpan2,
                                                                            convertedTimeSpan1,
                                                                            tempoMap);

                case MathOperation.Subtract:
                    return convertedTimeSpan1 - LengthConverter.ConvertFrom(mathTimeSpan.TimeSpan2,
                                                                            convertedTimeSpan1,
                                                                            tempoMap.Flip(convertedTimeSpan1));

                default:
                    throw new ArgumentException($"{mathTimeSpan.Operation} is not supported by the converter.", nameof(mathTimeSpan));
            }
        }

        private static long ConvertFromTimeTime(MathTimeSpan mathTimeSpan, long time, TempoMap tempoMap)
        {
            var timeSpan1 = mathTimeSpan.TimeSpan1;
            var timeSpan2 = mathTimeSpan.TimeSpan2;

            // Ensure that the first time span is not an instance of the MathTimeSpan. If it is,
            // convert it to the type of its first time span. For example, if we the following
            // time span was passed to the method:
            //
            //     (a1 + b1) + c
            //
            // the time span will be transformed to:
            //
            //     a2 + c

            var timeSpan1AsMath = mathTimeSpan.TimeSpan1 as MathTimeSpan;
            if (timeSpan1AsMath != null)
                timeSpan1 = TimeSpanConverter.ConvertTo(timeSpan1AsMath, timeSpan1AsMath.TimeSpan1.GetType(), time, tempoMap);

            // To subtract one time from another one we need to convert the second time span
            // to the type of the first one. After that result of subtraction will be of the
            // first time span type. And finally we convert result time span according to the
            // specified time. For example, time span shown above will be transformed first to
            //
            //     a3
            //
            // and then will be converted to MIDI time.

            switch (mathTimeSpan.Operation)
            {
                case MathOperation.Subtract:
                    {
                        var convertedTimeSpan2 = TimeConverter.ConvertTo(timeSpan2, timeSpan1.GetType(), tempoMap);
                        return TimeSpanConverter.ConvertFrom(timeSpan1.Subtract(convertedTimeSpan2, TimeSpanMode.TimeTime), time, tempoMap);
                    }

                default:
                    throw new ArgumentException($"{mathTimeSpan.Operation} is not supported by the converter.", nameof(mathTimeSpan));
            }
        }

        #endregion
    }
}
