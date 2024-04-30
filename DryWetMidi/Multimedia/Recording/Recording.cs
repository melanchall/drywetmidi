using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using Melanchall.DryWetMidi.Common;
using Melanchall.DryWetMidi.Core;
using Melanchall.DryWetMidi.Interaction;

namespace Melanchall.DryWetMidi.Multimedia
{
    /// <summary>
    /// Provides a way to record MIDI data received by an input MIDI device. More info in the
    /// <see href="xref:a_recording_overview">Recording</see> article.
    /// </summary>
    public sealed class Recording : IDisposable
    {
        #region Events

        /// <summary>
        /// Occurs when recording started via <see cref="Start"/> method.
        /// </summary>
        public event EventHandler Started;

        /// <summary>
        /// Occurs when recording stopped via <see cref="Stop"/> method.
        /// </summary>
        public event EventHandler Stopped;

        /// <summary>
        /// Occurs when a MIDI event is captured by the current recording.
        /// </summary>
        public event EventHandler<MidiEventRecordedEventArgs> EventRecorded;

        #endregion

        #region Fields

        private readonly List<RecordingEvent> _events = new List<RecordingEvent>();
        private readonly Stopwatch _stopwatch = new Stopwatch();

        private bool _disposed = false;

        #endregion

        #region Constructor

        /// <summary>
        /// Initializes a new instance of the <see cref="Recording"/> with the specified
        /// tempo map and input MIDI device to capture MIDI data from.
        /// </summary>
        /// <param name="tempoMap">Tempo map used to calculate events times.</param>
        /// <param name="inputDevice">Input MIDI device to capture MIDI data from.</param>
        /// <exception cref="ArgumentNullException">
        /// <para>One of the following errors occurred:</para>
        /// <list type="bullet">
        /// <item>
        /// <description><paramref name="tempoMap"/> is <c>null</c>.</description>
        /// </item>
        /// <item>
        /// <description><paramref name="inputDevice"/> is <c>null</c>.</description>
        /// </item>
        /// </list>
        /// </exception>
        public Recording(TempoMap tempoMap, IInputDevice inputDevice)
        {
            ThrowIfArgument.IsNull(nameof(tempoMap), tempoMap);
            ThrowIfArgument.IsNull(nameof(inputDevice), inputDevice);

            TempoMap = tempoMap;
            InputDevice = inputDevice;
            InputDevice.EventReceived += OnEventReceived;
        }

        #endregion

        #region Properties

        /// <summary>
        /// Gets the tempo map used to calculate recorded events times.
        /// </summary>
        public TempoMap TempoMap { get; }

        /// <summary>
        /// Gets the input MIDI device to record MIDI data from.
        /// </summary>
        public IInputDevice InputDevice { get; }

        /// <summary>
        /// Gets a value indicating whether recording is currently running or not.
        /// </summary>
        public bool IsRunning => _stopwatch.IsRunning;

        #endregion

        #region Methods

        /// <summary>
        /// Retrieves the duration of the recording in the specified format.
        /// </summary>
        /// <param name="durationType">Type that will represent the duration.</param>
        /// <returns>The duration of the recording as an instance of time span defined by
        /// <paramref name="durationType"/>.</returns>
        /// <exception cref="InvalidEnumArgumentException"><paramref name="durationType"/>
        /// specified an invalid value.</exception>
        public ITimeSpan GetDuration(TimeSpanType durationType)
        {
            ThrowIfArgument.IsInvalidEnumValue(nameof(durationType), durationType);

            return TimeConverter.ConvertTo((MetricTimeSpan)_events.LastOrDefault()?.Time ?? new MetricTimeSpan(), durationType, TempoMap);
        }

        /// <summary>
        /// Retrieves the duration of the recording in the specified format.
        /// </summary>
        /// <typeparam name="TTimeSpan">Type that will represent the duration.</typeparam>
        /// <returns>The duration of the recording as an instance of
        /// <typeparamref name="TTimeSpan"/>.</returns>
        public TTimeSpan GetDuration<TTimeSpan>()
            where TTimeSpan : ITimeSpan
        {
            return TimeConverter.ConvertTo<TTimeSpan>((MetricTimeSpan)_events.LastOrDefault()?.Time ?? new MetricTimeSpan(), TempoMap);
        }

        /// <summary>
        /// Gets MIDI events recorded by the current <see cref="Recording"/>.
        /// </summary>
        /// <returns>MIDI events recorded by the current <see cref="Recording"/>.</returns>
        public ICollection<TimedEvent> GetEvents()
        {
            return _events
                .Select(e => new TimedEvent(e.Event, TimeConverter.ConvertFrom((MetricTimeSpan)e.Time, TempoMap)))
                .ToArray();
        }

        /// <summary>
        /// Starts MIDI data recording.
        /// </summary>
        /// <exception cref="InvalidOperationException">Input device is not listening for MIDI events.</exception>
        public void Start()
        {
            if (IsRunning)
                return;

            if (!InputDevice.IsListeningForEvents)
                throw new InvalidOperationException($"Input device is not listening for MIDI events. Call {nameof(InputDevice.StartEventsListening)} prior to start recording.");

            _stopwatch.Start();
            OnStarted();
        }

        /// <summary>
        /// Stops MIDI data recording. Note that this method doesn't reset the recording time. If you
        /// call <see cref="Start"/>, recording will be resumed from the point where <see cref="Stop"/> was called.
        /// </summary>
        public void Stop()
        {
            if (!IsRunning)
                return;

            _stopwatch.Stop();
            OnStopped();
        }

        private void OnStarted()
        {
            Started?.Invoke(this, EventArgs.Empty);
        }

        private void OnStopped()
        {
            Stopped?.Invoke(this, EventArgs.Empty);
        }

        private void OnEventReceived(object sender, MidiEventReceivedEventArgs e)
        {
            if (!IsRunning)
                return;

            _events.Add(new RecordingEvent(e.Event, _stopwatch.Elapsed));

            OnEventRecorded(e.Event);
        }

        private void OnEventRecorded(MidiEvent midiEvent)
        {
            EventRecorded?.Invoke(this, new MidiEventRecordedEventArgs(midiEvent));
        }

        #endregion

        #region IDisposable

        /// <summary>
        /// Releases all resources used by the current <see cref="Recording"/>.
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
                InputDevice.EventReceived -= OnEventReceived;
            }

            _disposed = true;
        }

        #endregion
    }
}
