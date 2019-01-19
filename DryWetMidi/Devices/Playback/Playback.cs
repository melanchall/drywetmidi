using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Threading;
using Melanchall.DryWetMidi.Common;
using Melanchall.DryWetMidi.Smf;
using Melanchall.DryWetMidi.Smf.Interaction;

namespace Melanchall.DryWetMidi.Devices
{
    /// <summary>
    /// Provides a way to play MIDI data through the specified output MIDI device.
    /// </summary>
    public sealed class Playback : IDisposable
    {
        #region Constants

        private const uint ClockInterval = 1; // ms

        #endregion

        #region Fields

        private readonly IEnumerator<PlaybackEvent> _eventsEnumerator;

        private readonly MidiClock _clock;
        private readonly Dictionary<NoteId, NoteOnEvent> _noteOnEvents = new Dictionary<NoteId, NoteOnEvent>();

        private bool _disposed = false;

        #endregion

        #region Constructor

        /// <summary>
        /// Initializes a new instance of the <see cref="Playback"/> with the specified
        /// collection of MIDI events, tempo map and output MIDI device to play events through.
        /// </summary>
        /// <param name="events">Collection of MIDI events to play.</param>
        /// <param name="tempoMap">Tempo map used to calculate events times.</param>
        /// <param name="outputDevice">Output MIDI device to play <paramref name="events"/> through.</param>
        /// <exception cref="ArgumentNullException"><paramref name="events"/> is null. -or-
        /// <paramref name="tempoMap"/> is null. -or- <paramref name="outputDevice"/> is null.</exception>
        public Playback(IEnumerable<MidiEvent> events, TempoMap tempoMap, OutputDevice outputDevice)
            : this(new[] { events }, tempoMap, outputDevice)
        {
            ThrowIfArgument.IsNull(nameof(events), events);
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="Playback"/> with the specified
        /// collection of MIDI events collections, tempo map and output MIDI device to play events through.
        /// </summary>
        /// <param name="events">Collection of MIDI events collections to play.</param>
        /// <param name="tempoMap">Tempo map used to calculate events times.</param>
        /// <param name="outputDevice">Output MIDI device to play <paramref name="events"/> through.</param>
        /// <exception cref="ArgumentNullException"><paramref name="events"/> is null. -or-
        /// <paramref name="tempoMap"/> is null. -or- <paramref name="outputDevice"/> is null.</exception>
        public Playback(IEnumerable<IEnumerable<MidiEvent>> events, TempoMap tempoMap, OutputDevice outputDevice)
        {
            ThrowIfArgument.IsNull(nameof(events), events);
            ThrowIfArgument.IsNull(nameof(tempoMap), tempoMap);
            ThrowIfArgument.IsNull(nameof(outputDevice), outputDevice);

            _eventsEnumerator = GetPlaybackEvents(events, tempoMap).GetEnumerator();
            _eventsEnumerator.MoveNext();

            TempoMap = tempoMap;
            OutputDevice = outputDevice;

            _clock = new MidiClock(ClockInterval);
            _clock.Tick += OnClockTick;
        }

        #endregion

        #region Finalizer

        /// <summary>
        /// Finalizes the current instance of the <see cref="Playback"/>.
        /// </summary>
        ~Playback()
        {
            Dispose(false);
        }

        #endregion

        #region Properties

        /// <summary>
        /// Gets the tempo map used to calculate events times.
        /// </summary>
        public TempoMap TempoMap { get; }

        /// <summary>
        /// Gets the output MIDI device to play MIDI data through.
        /// </summary>
        public OutputDevice OutputDevice { get; }

        /// <summary>
        /// Gets a value indicating whether playing is currently running or not.
        /// </summary>
        public bool IsRunning => _clock.IsRunning;

        /// <summary>
        /// Gets or sets a value indicating whether playing should automatically start
        /// from the first event after the last one played.
        /// </summary>
        public bool Loop { get; set; }

        /// <summary>
        /// Gets or sets a value determining how currently playing notes should react on playback stopped.
        /// The default value is <see cref="NoteStopPolicy.Hold"/>.
        /// </summary>
        public NoteStopPolicy NoteStopPolicy { get; set; }

        /// <summary>
        /// Gets or sets the speed of events playing. 1 means normal speed. For example, to play
        /// events twice slower this property should be set to 0.5.
        /// </summary>
        /// <exception cref="ArgumentOutOfRangeException"><paramref name="value"/> is zero or negative.</exception>
        /// <exception cref="ObjectDisposedException">The current <see cref="Playback"/> is disposed.</exception>
        public double Speed
        {
            get { return _clock.Speed; }
            set
            {
                ThrowIfArgument.IsNonpositive(nameof(value), value, "Speed is zero or negative.");
                EnsureIsNotDisposed();

                _clock.Speed = value;
            }
        }

        #endregion

        #region Methods

        /// <summary>
        /// Retrieves the current time of the playback in the specified format.
        /// </summary>
        /// <param name="timeType">Type that will represent the current time.</param>
        /// <returns>The current time of the playback as an instance of time span defined by
        /// <paramref name="timeType"/>.</returns>
        /// <exception cref="InvalidEnumArgumentException"><paramref name="timeType"/> specified an invalid value.</exception>
        public ITimeSpan GetCurrentTime(TimeSpanType timeType)
        {
            ThrowIfArgument.IsInvalidEnumValue(nameof(timeType), timeType);

            return TimeConverter.ConvertTo((MetricTimeSpan)_clock.CurrentTime, timeType, TempoMap);
        }

        /// <summary>
        /// Retrieves the current time of the playback in the specified format.
        /// </summary>
        /// <typeparam name="TTimeSpan">Type that will represent the current time.</typeparam>
        /// <returns>The current time of the playback as an instance of
        /// <typeparamref name="TTimeSpan"/>.</returns>
        public TTimeSpan GetCurrentTime<TTimeSpan>()
            where TTimeSpan : ITimeSpan
        {
            return TimeConverter.ConvertTo<TTimeSpan>((MetricTimeSpan)_clock.CurrentTime, TempoMap);
        }

        /// <summary>
        /// Starts playing of the MIDI data. This method is non-blocking.
        /// </summary>
        /// <exception cref="ObjectDisposedException">The current <see cref="Playback"/> is disposed.</exception>
        /// <exception cref="MidiDeviceException">An error occurred on device.</exception>
        public void Start()
        {
            EnsureIsNotDisposed();

            if (_clock.IsRunning)
                return;

            OutputDevice.PrepareForEventsSending();

            foreach (var noteOnEvent in _noteOnEvents.Values)
            {
                OutputDevice.SendEvent(noteOnEvent);
            }
            _noteOnEvents.Clear();

            _clock.Start();
        }

        /// <summary>
        /// Stops playing of the MIDI data. Note that this method doesn't reset playback position. If you
        /// call <see cref="Start"/>, playing will be resumed from the point where <see cref="Stop"/> was called.
        /// </summary>
        /// <exception cref="ObjectDisposedException">The current <see cref="Playback"/> is disposed.</exception>
        /// <exception cref="MidiDeviceException">An error occurred on device.</exception>
        public void Stop()
        {
            EnsureIsNotDisposed();

            if (!IsRunning)
                return;

            _clock.Stop();
            StopNotes();
        }

        /// <summary>
        /// Starts playing of the MIDI data. This method will block execution of a program until all
        /// MIDI data is played.
        /// </summary>
        /// <remarks>
        /// If <see cref="Loop"/> is set to true, this method will execute forever.
        /// </remarks>
        /// <exception cref="ObjectDisposedException">The current <see cref="Playback"/> is disposed.</exception>
        /// <exception cref="MidiDeviceException">An error occurred on device.</exception>
        public void Play()
        {
            EnsureIsNotDisposed();

            Start();
            SpinWait.SpinUntil(() => !_clock.IsRunning);
        }

        /// <summary>
        /// Sets playback position to the beginning of the MIDI data.
        /// </summary>
        /// <exception cref="ObjectDisposedException">The current <see cref="Playback"/> is disposed.</exception>
        /// <exception cref="MidiDeviceException">An error occurred on device.</exception>
        public void MoveToStart()
        {
            EnsureIsNotDisposed();

            MoveToTime(new MetricTimeSpan());
        }

        /// <summary>
        /// Sets playback position to the specified time from the beginning of the MIDI data.
        /// </summary>
        /// <param name="time">Time from the beginning of the MIDI data to set playback position to.</param>
        /// <exception cref="ArgumentNullException"><paramref name="time"/> is null.</exception>
        /// <exception cref="ObjectDisposedException">The current <see cref="Playback"/> is disposed.</exception>
        /// <exception cref="MidiDeviceException">An error occurred on device.</exception>
        public void MoveToTime(ITimeSpan time)
        {
            ThrowIfArgument.IsNull(nameof(time), time);
            EnsureIsNotDisposed();

            var needStart = IsRunning;

            _clock.ResetInternalTimer();
            SetStartTime(time);

            if (needStart)
                _clock.StartInternalTimer();
        }

        /// <summary>
        /// Shifts playback position forward by the specified step.
        /// </summary>
        /// <param name="step">Amount of time to shift playback position by.</param>
        /// <exception cref="ArgumentNullException"><paramref name="step"/> is null.</exception>
        /// <exception cref="ObjectDisposedException">The current <see cref="Playback"/> is disposed.</exception>
        /// <exception cref="MidiDeviceException">An error occurred on device.</exception>
        public void MoveForward(ITimeSpan step)
        {
            ThrowIfArgument.IsNull(nameof(step), step);
            EnsureIsNotDisposed();

            var currentTime = (MetricTimeSpan)_clock.CurrentTime;
            MoveToTime(currentTime.Add(step, TimeSpanMode.TimeLength));
        }

        /// <summary>
        /// Shifts playback position back by the specified step.
        /// </summary>
        /// <param name="step">Amount of time to shift playback position by.</param>
        /// <exception cref="ArgumentNullException"><paramref name="step"/> is null.</exception>
        /// <exception cref="ObjectDisposedException">The current <see cref="Playback"/> is disposed.</exception>
        /// <exception cref="MidiDeviceException">An error occurred on device.</exception>
        public void MoveBack(ITimeSpan step)
        {
            ThrowIfArgument.IsNull(nameof(step), step);
            EnsureIsNotDisposed();

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

                if (!IsRunning)
                    return;

                OutputDevice.SendEvent(midiEvent);

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

            _clock.StopInternalTimer();
            _clock.Reset();
            _eventsEnumerator.Reset();
            _eventsEnumerator.MoveNext();
            _clock.StartInternalTimer();
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
                OutputDevice.SendEvent(new NoteOffEvent(noteId.NoteNumber, SevenBitNumber.MinValue) { Channel = noteId.Channel });
            }
        }

        private void SetStartTime(ITimeSpan time)
        {
            _clock.StartTime = _clock.CurrentTime = TimeConverter.ConvertTo<MetricTimeSpan>(time, TempoMap);

            _eventsEnumerator.Reset();
            do { _eventsEnumerator.MoveNext(); }
            while (_eventsEnumerator.Current != null && _eventsEnumerator.Current.Time < _clock.StartTime);
        }

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

        /// <summary>
        /// Releases all resources used by the current <see cref="Playback"/>.
        /// </summary>
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
