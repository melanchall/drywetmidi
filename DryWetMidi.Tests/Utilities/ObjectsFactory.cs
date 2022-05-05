using Melanchall.DryWetMidi.Common;
using Melanchall.DryWetMidi.Core;
using Melanchall.DryWetMidi.Interaction;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Melanchall.DryWetMidi.Tests
{
    internal sealed class ObjectsFactory
    {
        #region Constants

        public static readonly ObjectsFactory Default = new ObjectsFactory(TempoMap.Default);

        #endregion

        #region Fields

        private readonly Random _random = new Random();

        #endregion

        #region Constructor

        public ObjectsFactory(TempoMap tempoMap)
        {
            TempoMap = tempoMap;
        }

        #endregion

        #region Properties

        public TempoMap TempoMap { get; }

        #endregion

        #region Methods

        public TObject Same<TObject>(TObject obj)
            where TObject : ITimedObject =>
            (TObject)obj.Clone();

        public TObject WithTimeAndLength<TObject>(TObject obj, string time, string length)
            where TObject : ITimedObject =>
            WithLength(WithTime(obj, time), length);

        public TObject WithLength<TObject>(TObject obj, string length)
            where TObject : ITimedObject
        {
            var clone = obj.Clone();
            if (clone is ILengthedObject lengthedObject)
                lengthedObject.Length = GetLength(TimeSpanUtilities.Parse(length), (MidiTimeSpan)obj.Time);

            return (TObject)clone;
        }

        public TObject WithTime<TObject>(TObject obj, string time)
            where TObject : ITimedObject
        {
            var clone = obj.Clone();
            clone.Time = GetTime(TimeSpanUtilities.Parse(time));
            return (TObject)clone;
        }

        public ICollection<ITimedObject> WithTimes(ICollection<ITimedObject> objects, params string[] times) =>
            objects.Zip(times, (obj, time) => WithTime(obj, time)).ToArray();

        public ICollection<ITimedObject> WithTimesAndLengths(ICollection<ITimedObject> objects, params string[] timesAndLengths) =>
            objects.Zip(
                Enumerable.Range(0, timesAndLengths.Length / 2).Select(i => new
                {
                    Time = timesAndLengths[i * 2],
                    Length = timesAndLengths[i * 2 + 1]
                }),
                (obj, timeAndLength) => WithTimeAndLength(obj, timeAndLength.Time, timeAndLength.Length)).ToArray();

        public Note GetNote(string time, string length) => GetNote(
            (SevenBitNumber)_random.Next(SevenBitNumber.MaxValue + 1),
            time,
            length);

        public Note GetNote(int noteNumber, string time, string length) => GetNote(
            (SevenBitNumber)noteNumber,
            TimeSpanUtilities.Parse(time),
            TimeSpanUtilities.Parse(length));

        public Note GetNote(int noteNumber, ITimeSpan time, ITimeSpan length) => new Note(
            (SevenBitNumber)noteNumber,
            GetLength(length, time),
            GetTime(time))
        { Channel = (FourBitNumber)2, Velocity = (SevenBitNumber)60 };

        public Note GetNote(int noteNumber, long time, ITimeSpan length) => GetNote(
            noteNumber,
            (ITimeSpan)(MidiTimeSpan)time,
            length);

        public Chord GetChord(params string[] timesAndLengths) => new Chord(
            Enumerable
                .Range(0, timesAndLengths.Length / 2)
                .Select(i => GetNote(timesAndLengths[i * 2], timesAndLengths[i * 2 + 1])));

        public TimedEvent GetTimedEvent(string time) => new TimedEvent(
            new TextEvent(_random.Next(100).ToString()),
            GetTime(TimeSpanUtilities.Parse(time)));

        private long GetTime(ITimeSpan time) => TimeConverter.ConvertFrom(time, TempoMap);

        private long GetLength(ITimeSpan length, ITimeSpan time) => LengthConverter.ConvertFrom(length, time, TempoMap);

        #endregion
    }
}
