using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Melanchall.DryWetMidi.Common;
using Melanchall.DryWetMidi.Smf.Interaction;

namespace Melanchall.DryWetMidi.Devices
{
    public sealed class Recording : IDisposable
    {
        #region Fields

        private readonly List<RecordingEvent> _events = new List<RecordingEvent>();
        private readonly InputDevice _inputDevice;
        private readonly TempoMap _tempoMap;
        private readonly Stopwatch _stopwatch = new Stopwatch();

        private bool _disposed = false;

        #endregion

        #region Constructor

        public Recording(TempoMap tempoMap, InputDevice inputDevice)
        {
            ThrowIfArgument.IsNull(nameof(tempoMap), tempoMap);
            ThrowIfArgument.IsNull(nameof(inputDevice), inputDevice);

            _tempoMap = tempoMap;
            _inputDevice = inputDevice;
            _inputDevice.EventReceived += OnEventReceived;
        }

        #endregion

        #region Properties

        public bool IsRunning { get; private set; }

        #endregion

        #region Methods

        public IReadOnlyList<TimedEvent> GetEvents()
        {
            return _events.Select(e => new TimedEvent(e.Event, TimeConverter.ConvertFrom((MetricTimeSpan)e.Time, _tempoMap)))
                          .ToList()
                          .AsReadOnly();
        }

        public void Start()
        {
            if (IsRunning)
                return;

            _inputDevice.StartEventsListening();
            _stopwatch.Start();

            IsRunning = true;
        }

        public void Stop()
        {
            if (!IsRunning)
                return;

            _inputDevice.StopEventsListening();
            _stopwatch.Stop();

            IsRunning = false;
        }

        private void OnEventReceived(object sender, MidiEventReceivedEventArgs e)
        {
            _events.Add(new RecordingEvent(e.Event, _stopwatch.Elapsed));
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
                _inputDevice.EventReceived -= OnEventReceived;
            }

            _disposed = true;
        }

        #endregion
    }
}
