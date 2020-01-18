using System;
using System.Collections.Generic;
using System.Linq;
using BenchmarkDotNet.Attributes;
using Melanchall.DryWetMidi.Interaction;

namespace Melanchall.DryWetMidi.Benchmarks.Interaction
{
    public abstract class TimeSpanBenchmarks<TTimeSpan>
        where TTimeSpan : ITimeSpan
    {
        #region Constants

        protected const int TimesCount = 2000;
        protected const long TimeOffset = 10;
        protected const long Length = 100;

        #endregion

        #region Fields

        protected readonly IEnumerable<long> _midiTimes =
            Enumerable.Range(0, TimesCount)
                      .Select(i => i * TimeOffset)
                      .ToList();

        private IEnumerable<TTimeSpan> _timeSpanTimes;
        private IEnumerable<Tuple<TTimeSpan, long>> _timeSpanLengths;

        #endregion

        #region Properties

        public abstract TempoMap TempoMap { get; }

        #endregion

        #region Methods

        [GlobalSetup]
        public void Setup()
        {
            _timeSpanTimes = _midiTimes.Select(time => TimeConverter.ConvertTo<TTimeSpan>(time, TempoMap)).ToList();
            _timeSpanLengths = _midiTimes.Select(time => Tuple.Create(LengthConverter.ConvertTo<TTimeSpan>(Length, time, TempoMap), time)).ToList();
        }

        [Benchmark]
        public void ToTimeSpan_Time()
        {
            foreach (var midiTime in _midiTimes)
            {
                var timeSpanTime = TimeConverter.ConvertTo<TTimeSpan>(midiTime, TempoMap);
            }
        }

        [Benchmark]
        public void ToTimeSpan_Length()
        {
            foreach (var midiTime in _midiTimes)
            {
                var timeSpanLength = LengthConverter.ConvertTo<TTimeSpan>(Length, midiTime, TempoMap);
            }
        }

        [Benchmark]
        public void FromTimeSpan_Time()
        {
            foreach (var timeSpan in _timeSpanTimes)
            {
                var midiTime = TimeConverter.ConvertFrom(timeSpan, TempoMap);
            }
        }

        [Benchmark]
        public void FromTimeSpan_Length()
        {
            foreach (var timeSpan in _timeSpanLengths)
            {
                var midiLength = LengthConverter.ConvertFrom(timeSpan.Item1, timeSpan.Item2, TempoMap);
            }
        }

        #endregion
    }
}
