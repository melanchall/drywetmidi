using Melanchall.DryWetMidi.Common;
using Melanchall.DryWetMidi.Core;
using Melanchall.DryWetMidi.Interaction;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Melanchall.DryWetMidi.Tools
{
    public class Repeater
    {
        #region Constants

        private static readonly Dictionary<MidiEventType, Func<MidiEvent>> TempoMapEventsCreators = new Dictionary<MidiEventType, Func<MidiEvent>>
        {
            [MidiEventType.SetTempo] = () => new SetTempoEvent(),
            [MidiEventType.TimeSignature] = () => new TimeSignatureEvent(),
        };

        #endregion

        #region Methods

        public MidiFile Repeat(MidiFile midiFile, int repeatsNumber, RepeatingSettings settings = null)
        {
            ThrowIfArgument.IsNull(nameof(midiFile), midiFile);
            ThrowIfArgument.IsNonpositive(nameof(repeatsNumber), repeatsNumber, "Repeats number is zero or negative.");
            CheckSettings(settings);

            settings = settings ?? new RepeatingSettings();

            var tempoMap = midiFile.GetTempoMap();
            var trackChunks = Repeat(midiFile.GetTrackChunks(), repeatsNumber, tempoMap, settings);

            return new MidiFile(trackChunks)
            {
                TimeDivision = midiFile.TimeDivision.Clone()
            };
        }

        public ICollection<TrackChunk> Repeat(IEnumerable<TrackChunk> trackChunks, int repeatsNumber, TempoMap tempoMap, RepeatingSettings settings = null)
        {
            ThrowIfArgument.IsNull(nameof(trackChunks), trackChunks);
            ThrowIfArgument.IsNull(nameof(tempoMap), tempoMap);
            ThrowIfArgument.IsNonpositive(nameof(repeatsNumber), repeatsNumber, "Repeats number is zero or negative.");
            CheckSettings(settings);

            settings = settings ?? new RepeatingSettings();

            var timedEventsCollections = trackChunks.Select(trackChunk => trackChunk.GetTimedEvents()).ToArray();
            var maxTime = timedEventsCollections.Max(events => events.LastOrDefault()?.Time ?? 0);

            var shift = CalculateShift(maxTime, tempoMap, settings);
            return timedEventsCollections
                .Select(events => Repeat(events, shift, repeatsNumber, tempoMap, settings).ToTrackChunk())
                .ToArray();
        }

        public TrackChunk Repeat(TrackChunk trackChunk, int repeatsNumber, TempoMap tempoMap, RepeatingSettings settings = null)
        {
            ThrowIfArgument.IsNull(nameof(trackChunk), trackChunk);
            ThrowIfArgument.IsNull(nameof(tempoMap), tempoMap);
            ThrowIfArgument.IsNonpositive(nameof(repeatsNumber), repeatsNumber, "Repeats number is zero or negative.");
            CheckSettings(settings);

            settings = settings ?? new RepeatingSettings();

            var timedObjects = trackChunk.GetTimedEvents();
            var maxTime = timedObjects.LastOrDefault()?.Time ?? 0;
            var shift = CalculateShift(maxTime, tempoMap, settings);

            return Repeat(timedObjects, shift, repeatsNumber, tempoMap, settings).ToTrackChunk();
        }

        public ICollection<ITimedObject> Repeat(IEnumerable<ITimedObject> timedObjects, int repeatsNumber, TempoMap tempoMap, RepeatingSettings settings = null)
        {
            ThrowIfArgument.IsNull(nameof(timedObjects), timedObjects);
            ThrowIfArgument.IsNull(nameof(tempoMap), tempoMap);
            ThrowIfArgument.IsNonpositive(nameof(repeatsNumber), repeatsNumber, "Repeats number is zero or negative.");
            CheckSettings(settings);

            settings = settings ?? new RepeatingSettings();

            var maxTime = timedObjects.Select(obj => (obj?.Time ?? 0) + ((obj as ILengthedObject)?.Length ?? 0)).DefaultIfEmpty(0).Max();
            var shift = CalculateShift(maxTime, tempoMap, settings);

            return Repeat(timedObjects, shift, repeatsNumber, tempoMap, settings);
        }

        protected virtual void ProcessPart(PartProcessingContext context)
        {
            if (context.Settings.SaveTempoMap)
            {
                if (context.SourceFirstSetTempoEvent?.Time > 0)
                    context.PartObjects.Insert(0, new TimedEvent(new SetTempoEvent(), 0));
                if (context.SourceFirstTimeSignatureEvent?.Time > 0)
                    context.PartObjects.Insert(0, new TimedEvent(new TimeSignatureEvent(), 0));
            }

            foreach (var obj in context.PartObjects)
            {
                obj.Time += context.PartIndex * context.Shift;
            }
        }

        private void CheckSettings(RepeatingSettings settings)
        {
            if (settings == null)
                return;

            if (settings.ShiftPolicy == ShiftPolicy.ShiftByFixedValue && settings.Shift == null)
                throw new InvalidOperationException("Shift value is null for fixed-value shift.");
        }

        private ICollection<ITimedObject> Repeat(
            IEnumerable<ITimedObject> timedObjects,
            long shift,
            int repeatsNumber,
            TempoMap tempoMap,
            RepeatingSettings settings)
        {
            settings = settings ?? new RepeatingSettings();

            var source = timedObjects.Where(obj => obj != null).Select(obj => obj.Clone()).ToArray();
            var result = new List<ITimedObject>(source);

            for (var i = 1; i <= repeatsNumber; i++)
            {
                var part = GetPart(source, shift, i, tempoMap, settings);
                result.AddRange(part);
            }

            return result;
        }

        private ICollection<ITimedObject> GetPart(IEnumerable<ITimedObject> sourceObjects, long shift, int partIndex, TempoMap tempoMap, RepeatingSettings settings)
        {
            var result = new List<ITimedObject>(sourceObjects.Where(o => o != null).Select(o => o.Clone()));

            TimedEvent firstSetTempoEvent = null;
            TimedEvent firstTimeSignatureEvent = null;

            foreach (var timedEvent in sourceObjects.OfType<TimedEvent>())
            {
                if (firstSetTempoEvent != null && firstTimeSignatureEvent != null)
                    break;

                var eventType = timedEvent.Event.EventType;
                if (firstSetTempoEvent == null && eventType == MidiEventType.SetTempo)
                    firstSetTempoEvent = timedEvent;
                else if (firstTimeSignatureEvent == null && eventType == MidiEventType.TimeSignature)
                    firstTimeSignatureEvent = timedEvent;
            }

            var context = new PartProcessingContext
            {
                SourceObjects = sourceObjects,
                PartObjects = result,
                PartIndex = partIndex,
                Shift = shift,
                SourceTempoMap = tempoMap,
                Settings = settings,
                SourceFirstSetTempoEvent = firstSetTempoEvent,
                SourceFirstTimeSignatureEvent = firstTimeSignatureEvent
            };

            ProcessPart(context);
            return result;
        }

        private static long CalculateShift(long maxTime, TempoMap tempoMap, RepeatingSettings settings)
        {
            var shift = default(ITimeSpan);

            switch (settings.ShiftPolicy)
            {
                case ShiftPolicy.None:
                    return 0;
                case ShiftPolicy.ShiftByFixedValue:
                    shift = settings.Shift;
                    break;
                case ShiftPolicy.ShiftByMaxTime:
                    shift = (MidiTimeSpan)maxTime;
                    break;
            }

            var shiftStep = settings.ShiftStep;
            if (shiftStep != null)
                shift = RoundShift(shift, shiftStep, tempoMap);

            return TimeConverter.ConvertFrom(shift, tempoMap);
        }

        private static ITimeSpan RoundShift(ITimeSpan shift, ITimeSpan shiftStep, TempoMap tempoMap)
        {
            var metricStep = shiftStep as MetricTimeSpan;
            if (metricStep != null)
            {
                if (metricStep.TotalMicroseconds == 0)
                    return shift;

                var metricShift = TimeConverter.ConvertTo<MetricTimeSpan>(shift, tempoMap);
                return RoundShift(
                    shift,
                    metricShift.TotalMicroseconds,
                    metricStep.TotalMicroseconds,
                    quotient => new MetricTimeSpan((quotient + 1) * metricStep.TotalMicroseconds));
            }

            var midiStep = TimeConverter.ConvertTo<MidiTimeSpan>(shiftStep, tempoMap);
            if (midiStep.TimeSpan == 0)
                return shift;

            var midiShift = TimeConverter.ConvertTo<MidiTimeSpan>(shift, tempoMap);
            return RoundShift(
                shift,
                midiShift.TimeSpan,
                midiStep.TimeSpan,
                quotient => new MidiTimeSpan((quotient + 1) * midiStep.TimeSpan));
        }

        private static ITimeSpan RoundShift<TTimeSpan>(
            ITimeSpan shift,
            long x,
            long y,
            Func<long, TTimeSpan> createTimeSpan)
            where TTimeSpan : ITimeSpan
        {
            long reminder;
            var quotient = Math.DivRem(x, y, out reminder);
            return reminder == 0
                ? shift
                : createTimeSpan(quotient);
        }

        #endregion
    }
}
