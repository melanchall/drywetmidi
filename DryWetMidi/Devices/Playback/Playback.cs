using System;
using System.Collections.Generic;
using System.Linq;
using Melanchall.DryWetMidi.Common;
using Melanchall.DryWetMidi.Smf;
using Melanchall.DryWetMidi.Smf.Interaction;

namespace Melanchall.DryWetMidi.Devices
{
    public sealed class Playback : IDisposable
    {
        #region Constants

        private const uint ClockInterval = 1; // ms

        #endregion

        #region Fields

        private readonly IEnumerator<PlaybackEvent> _eventsEnumerator;

        private readonly TempoMap _tempoMap;
        private readonly OutputDevice _outputDevice;

        private readonly MidiClock _clock;
        private readonly Dictionary<NoteId, NoteOnEvent> _noteOnEvents = new Dictionary<NoteId, NoteOnEvent>();

        private bool _disposed = false;

        #endregion

        #region Constructor

        public Playback(IEnumerable<MidiEvent> events, TempoMap tempoMap, OutputDevice outputDevice)
            : this(new[] { events }, tempoMap, outputDevice)
        {
            ThrowIfArgument.IsNull(nameof(events), events);
        }

        public Playback(IEnumerable<IEnumerable<MidiEvent>> events, TempoMap tempoMap, OutputDevice outputDevice)
        {
            ThrowIfArgument.IsNull(nameof(events), events);
            ThrowIfArgument.IsNull(nameof(tempoMap), tempoMap);
            ThrowIfArgument.IsNull(nameof(outputDevice), outputDevice);

            _eventsEnumerator = GetPlaybackEvents(events, tempoMap).GetEnumerator();
            _eventsEnumerator.MoveNext();

            _tempoMap = tempoMap;
            _outputDevice = outputDevice;

            _clock = new MidiClock(ClockInterval);
            _clock.Tick += OnClockTick;
        }

        #endregion

        #region Finalizer

        ~Playback()
        {
            Dispose(false);
        }

        #endregion

        #region Properties

        public bool IsRunning { get; private set; }

        public bool Loop { get; set; }

        public NoteStopPolicy NoteStopPolicy { get; set; }

        public double Speed
        {
            get { return _clock.Speed; }
            set
            {
                ThrowIfArgument.IsNonpositive(nameof(value), value, "Speed is zero or negative.");

                _clock.Speed = value;
            }
        }

        #endregion

        #region Methods

        public ITimeSpan GetCurrentTime(TimeSpanType timeType)
        {
            ThrowIfArgument.IsInvalidEnumValue(nameof(timeType), timeType);

            return TimeConverter.ConvertTo((MetricTimeSpan)_clock.CurrentTime, timeType, _tempoMap);
        }

        public TTimeSpan GetCurrentTime<TTimeSpan>()
            where TTimeSpan : ITimeSpan
        {
            return TimeConverter.ConvertTo<TTimeSpan>((MetricTimeSpan)_clock.CurrentTime, _tempoMap);
        }

        public void Start()
        {
            EnsureIsNotDisposed();

            if (_clock.IsRunning)
                return;

            _outputDevice.PrepareForEventsSending();

            foreach (var noteOnEvent in _noteOnEvents.Values)
            {
                _outputDevice.SendEvent(noteOnEvent);
            }
            _noteOnEvents.Clear();

            _clock.Start();
            IsRunning = true;
        }

        public void Stop()
        {
            EnsureIsNotDisposed();

            if (!_clock.IsRunning)
                return;

            _clock.Stop();
            StopNotes();
            IsRunning = false;
        }

        public void MoveToStart()
        {
            MoveToTime(new MetricTimeSpan());
        }

        public void MoveToTime(ITimeSpan time)
        {
            ThrowIfArgument.IsNull(nameof(time), time);

            var needStart = IsRunning;

            Stop();
            SetStartTime(time);

            if (needStart)
                Start();
        }

        public void MoveForward(ITimeSpan step)
        {
            ThrowIfArgument.IsNull(nameof(step), step);

            var currentTime = (MetricTimeSpan)_clock.CurrentTime;
            MoveToTime(currentTime.Add(step, TimeSpanMode.TimeLength));
        }

        public void MoveBack(ITimeSpan step)
        {
            ThrowIfArgument.IsNull(nameof(step), step);

            var currentTime = (MetricTimeSpan)_clock.CurrentTime;
            MoveToTime(currentTime.Subtract(step, TimeSpanMode.TimeLength));
        }

        private void OnClockTick(object sender, TickEventArgs e)
        {
            var time = e.Time;

            do
            {
                var timedEvent = _eventsEnumerator.Current;
                if (timedEvent == null)
                    continue;

                if (timedEvent.Time > time)
                    return;

                var midiEvent = timedEvent.Event;

                if (!_clock.IsRunning)
                    return;

                _outputDevice.SendEvent(midiEvent);

                var noteOnEvent = midiEvent as NoteOnEvent;
                if (noteOnEvent != null)
                    _noteOnEvents[noteOnEvent.GetNoteId()] = noteOnEvent;

                var noteOffEvent = midiEvent as NoteOffEvent;
                if (noteOffEvent != null)
                    _noteOnEvents.Remove(noteOffEvent.GetNoteId());
            }
            while (_eventsEnumerator.MoveNext());

            if (!Loop)
            {
                Stop();
                return;
            }

            _clock.Restart();
            _eventsEnumerator.Reset();
            _eventsEnumerator.MoveNext();
        }

        private void EnsureIsNotDisposed()
        {
            if (_disposed)
                throw new ObjectDisposedException("Playback is disposed.");
        }

        private void StopNotes()
        {
            switch (NoteStopPolicy)
            {
                case NoteStopPolicy.Interrupt:
                    FinishCurrentNotes();
                    _noteOnEvents.Clear();
                    break;
                case NoteStopPolicy.Hold:
                    _noteOnEvents.Clear();
                    break;
                case NoteStopPolicy.Split:
                    FinishCurrentNotes();
                    break;
            }
        }

        private void FinishCurrentNotes()
        {
            foreach (var noteId in _noteOnEvents.Keys)
            {
                _outputDevice.SendEvent(new NoteOffEvent(noteId.NoteNumber, SevenBitNumber.MinValue) { Channel = noteId.Channel });
            }
        }

        private void SetStartTime(ITimeSpan time)
        {
            _clock.StartTime = TimeConverter.ConvertTo<MetricTimeSpan>(time, _tempoMap);

            _eventsEnumerator.Reset();
            do { _eventsEnumerator.MoveNext(); }
            while (_eventsEnumerator.Current != null && _eventsEnumerator.Current.Time < _clock.StartTime);
        }

        // TODO: prepare events in bytes format
        private static IEnumerable<PlaybackEvent> GetPlaybackEvents(IEnumerable<IEnumerable<MidiEvent>> events, TempoMap tempoMap)
        {
            return events.Where(e => e != null)
                         .Select(e => new TrackChunk(e.Where(midiEvent => !(midiEvent is MetaEvent))))
                         .GetTimedEvents()
                         .Select(e => new PlaybackEvent(e.Event, e.TimeAs<MetricTimeSpan>(tempoMap), e.Time))
                         .OrderBy(e => e, new PlaybackEventsComparer())
                         .ToList();
        }

        #endregion

        #region IDisposable

        public void Dispose()
        {
            Dispose(true);
        }

        private void Dispose(bool disposing)
        {
            if (_disposed)
                return;

            if (disposing)
            {
                Stop();

                _clock.Tick -= OnClockTick;
                _clock.Dispose();
                _eventsEnumerator.Dispose();
            }

            _disposed = true;
        }

        #endregion
    }
}
