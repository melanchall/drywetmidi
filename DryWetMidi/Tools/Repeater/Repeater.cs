using Melanchall.DryWetMidi.Common;
using Melanchall.DryWetMidi.Core;
using Melanchall.DryWetMidi.Interaction;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Melanchall.DryWetMidi.Tools
{
    /// <summary>
    /// Provides a way to repeat MIDI data using different options. More info in the
    /// <see href="xref:a_repeater">Repeater</see> article.
    /// </summary>
    public class Repeater
    {
        #region Methods

        /// <summary>
        /// Repeats a MIDI file specified number of times.
        /// </summary>
        /// <param name="midiFile">The file to repeat.</param>
        /// <param name="repeatsNumber">Number of times the <paramref name="midiFile"/> should be repeated.</param>
        /// <param name="settings">Settings according to which the operation should be done.</param>
        /// <returns>A new instance of the <see cref="MidiFile"/> which is the <paramref name="midiFile"/>
        /// repeated <paramref name="repeatsNumber"/> times using <paramref name="settings"/>.</returns>
        /// <exception cref="ArgumentNullException"><paramref name="midiFile"/> is <c>null</c>.</exception>
        /// <exception cref="ArgumentOutOfRangeException"><paramref name="repeatsNumber"/> is negative.</exception>
        /// <exception cref="ArgumentException"><see cref="RepeatingSettings.Shift"/> of the <paramref name="settings"/>
        /// is <c>null</c> for fixed-value shift.</exception>
        public MidiFile Repeat(MidiFile midiFile, int repeatsNumber, RepeatingSettings settings = null)
        {
            ThrowIfArgument.IsNull(nameof(midiFile), midiFile);
            ThrowIfArgument.IsNegative(nameof(repeatsNumber), repeatsNumber, "Repeats number is negative.");
            CheckSettings(settings);

            if (repeatsNumber == 0)
                return midiFile.Clone();

            settings = settings ?? new RepeatingSettings();

            var tempoMap = midiFile.GetTempoMap();
            var trackChunks = Repeat(midiFile.GetTrackChunks(), repeatsNumber, tempoMap, settings);

            return new MidiFile(trackChunks)
            {
                TimeDivision = midiFile.TimeDivision.Clone()
            };
        }

        /// <summary>
        /// Repeats a collection of <see cref="TrackChunk"/> specified number of times.
        /// </summary>
        /// <param name="trackChunks">The collection of <see cref="TrackChunk"/> to repeat.</param>
        /// <param name="repeatsNumber">Number of times the <paramref name="trackChunks"/> should be repeated.</param>
        /// <param name="tempoMap">Tempo map used to perform time spans calculations.</param>
        /// <param name="settings">Settings according to which the operation should be done.</param>
        /// <returns>A collection of new <see cref="TrackChunk"/> instances which are the <paramref name="trackChunks"/>
        /// repeated <paramref name="repeatsNumber"/> times using <paramref name="settings"/>.</returns>
        /// <exception cref="ArgumentNullException">
        /// <para>One of the following errors occurred:</para>
        /// <list type="bullet">
        /// <item>
        /// <description><paramref name="trackChunks"/> is <c>null</c>.</description>
        /// </item>
        /// <item>
        /// <description><paramref name="tempoMap"/> is <c>null</c>.</description>
        /// </item>
        /// </list>
        /// </exception>
        /// <exception cref="ArgumentOutOfRangeException"><paramref name="repeatsNumber"/> is negative.</exception>
        /// <exception cref="ArgumentException"><see cref="RepeatingSettings.Shift"/> of the <paramref name="settings"/>
        /// is <c>null</c> for fixed-value shift.</exception>
        public ICollection<TrackChunk> Repeat(IEnumerable<TrackChunk> trackChunks, int repeatsNumber, TempoMap tempoMap, RepeatingSettings settings = null)
        {
            ThrowIfArgument.IsNull(nameof(trackChunks), trackChunks);
            ThrowIfArgument.IsNull(nameof(tempoMap), tempoMap);
            ThrowIfArgument.IsNegative(nameof(repeatsNumber), repeatsNumber, "Repeats number is negative.");
            CheckSettings(settings);

            if (repeatsNumber == 0)
                return trackChunks.Select(c => (TrackChunk)c.Clone()).ToArray();

            settings = settings ?? new RepeatingSettings();

            var timedEventsCollections = trackChunks.Select(trackChunk => trackChunk.GetTimedEvents()).ToArray();
            var maxTime = timedEventsCollections.Max(events => events.LastOrDefault()?.Time ?? 0);

            var shift = CalculateShift(maxTime, tempoMap, settings);
            return timedEventsCollections
                .Select(events => Repeat(events, shift, repeatsNumber, tempoMap, settings).ToTrackChunk())
                .ToArray();
        }

        /// <summary>
        /// Repeats a <see cref="TrackChunk"/> specified number of times.
        /// </summary>
        /// <param name="trackChunk">The <see cref="TrackChunk"/> to repeat.</param>
        /// <param name="repeatsNumber">Number of times the <paramref name="trackChunk"/> should be repeated.</param>
        /// <param name="tempoMap">Tempo map used to perform time spans calculations.</param>
        /// <param name="settings">Settings according to which the operation should be done.</param>
        /// <returns>A new instance of the <see cref="TrackChunk"/> which is the <paramref name="trackChunk"/>
        /// repeated <paramref name="repeatsNumber"/> times using <paramref name="settings"/>.</returns>
        /// <exception cref="ArgumentNullException">
        /// <para>One of the following errors occurred:</para>
        /// <list type="bullet">
        /// <item>
        /// <description><paramref name="trackChunk"/> is <c>null</c>.</description>
        /// </item>
        /// <item>
        /// <description><paramref name="tempoMap"/> is <c>null</c>.</description>
        /// </item>
        /// </list>
        /// </exception>
        /// <exception cref="ArgumentOutOfRangeException"><paramref name="repeatsNumber"/> is negative.</exception>
        /// <exception cref="ArgumentException"><see cref="RepeatingSettings.Shift"/> of the <paramref name="settings"/>
        /// is <c>null</c> for fixed-value shift.</exception>
        public TrackChunk Repeat(TrackChunk trackChunk, int repeatsNumber, TempoMap tempoMap, RepeatingSettings settings = null)
        {
            ThrowIfArgument.IsNull(nameof(trackChunk), trackChunk);
            ThrowIfArgument.IsNull(nameof(tempoMap), tempoMap);
            ThrowIfArgument.IsNegative(nameof(repeatsNumber), repeatsNumber, "Repeats number is negative.");
            CheckSettings(settings);

            if (repeatsNumber == 0)
                return (TrackChunk)trackChunk.Clone();

            settings = settings ?? new RepeatingSettings();

            var timedObjects = trackChunk.GetTimedEvents();
            var maxTime = timedObjects.LastOrDefault()?.Time ?? 0;
            var shift = CalculateShift(maxTime, tempoMap, settings);

            return Repeat(timedObjects, shift, repeatsNumber, tempoMap, settings).ToTrackChunk();
        }

        /// <summary>
        /// Repeats a collection of timed objects specified number of times.
        /// </summary>
        /// <param name="timedObjects">The collection of timed objects to repeat.</param>
        /// <param name="repeatsNumber">Number of times the <paramref name="timedObjects"/> should be repeated.</param>
        /// <param name="tempoMap">Tempo map used to perform time spans calculations.</param>
        /// <param name="settings">Settings according to which the operation should be done.</param>
        /// <returns>A collection of new <see cref="TrackChunk"/> instances which are the <paramref name="timedObjects"/>
        /// repeated <paramref name="repeatsNumber"/> times using <paramref name="settings"/>.</returns>
        /// <exception cref="ArgumentNullException">
        /// <para>One of the following errors occurred:</para>
        /// <list type="bullet">
        /// <item>
        /// <description><paramref name="timedObjects"/> is <c>null</c>.</description>
        /// </item>
        /// <item>
        /// <description><paramref name="tempoMap"/> is <c>null</c>.</description>
        /// </item>
        /// </list>
        /// </exception>
        /// <exception cref="ArgumentOutOfRangeException"><paramref name="repeatsNumber"/> is negative.</exception>
        /// <exception cref="ArgumentException"><see cref="RepeatingSettings.Shift"/> of the <paramref name="settings"/>
        /// is <c>null</c> for fixed-value shift.</exception>
        public ICollection<ITimedObject> Repeat(IEnumerable<ITimedObject> timedObjects, int repeatsNumber, TempoMap tempoMap, RepeatingSettings settings = null)
        {
            ThrowIfArgument.IsNull(nameof(timedObjects), timedObjects);
            ThrowIfArgument.IsNull(nameof(tempoMap), tempoMap);
            ThrowIfArgument.IsNegative(nameof(repeatsNumber), repeatsNumber, "Repeats number is negative.");
            CheckSettings(settings);

            if (repeatsNumber == 0)
                return timedObjects.Select(o => o.Clone()).ToArray();

            settings = settings ?? new RepeatingSettings();

            var maxTime = timedObjects.Select(obj => (obj?.Time ?? 0) + ((obj as ILengthedObject)?.Length ?? 0)).DefaultIfEmpty(0).Max();
            var shift = CalculateShift(maxTime, tempoMap, settings);

            return Repeat(timedObjects, shift, repeatsNumber, tempoMap, settings);
        }

        /// <summary>
        /// Processes a new part that will be appended to the previous ones.
        /// </summary>
        /// <param name="context">An object holding all the required data to process a part.</param>
        /// <remarks>
        /// By default the method shifts the data and inserts tempo map events if <see cref="RepeatingSettings.PreserveTempoMap"/>
        /// set to <c>true</c> in settings used for the processing.
        /// </remarks>
        protected virtual void ProcessPart(PartProcessingContext context)
        {
            if (context.Settings.PreserveTempoMap)
            {
                if (context.SourceFirstSetTempoEvent?.Time > 0)
                    context.PartObjects.Insert(0, new TimedEvent(new SetTempoEvent(), 0));
                if (context.SourceFirstTimeSignatureEvent?.Time > 0)
                    context.PartObjects.Insert(0, new TimedEvent(new TimeSignatureEvent(), 0));
            }

            foreach (var obj in context.PartObjects)
            {
                obj.Time += (context.PartIndex + 1) * context.Shift;
            }
        }

        private void CheckSettings(RepeatingSettings settings)
        {
            if (settings == null)
                return;

            if (settings.ShiftPolicy == ShiftPolicy.ShiftByFixedValue && settings.Shift == null)
                throw new ArgumentException("Shift value is null for fixed-value shift.", nameof(settings));
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
                var part = GetPart(source, shift, i - 1, tempoMap, settings);
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

            var roundingPolicy = settings.ShiftRoundingPolicy;
            if (roundingPolicy != TimeSpanRoundingPolicy.NoRounding)
            {
                var shiftStep = settings.ShiftRoundingStep;
                if (shiftStep != null)
                    shift = shift.Round(roundingPolicy, 0, shiftStep, tempoMap);
            }

            return TimeConverter.ConvertFrom(shift, tempoMap);
        }

        #endregion
    }
}
