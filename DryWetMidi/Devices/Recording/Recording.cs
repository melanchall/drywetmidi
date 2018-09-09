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

            _inputDevice.Start();
            _stopwatch.Start();

            IsRunning = true;
        }

        public void Stop()
        {
            if (!IsRunning)
                return;

            // TODO: what if device was started from outside
            _inputDevice.Stop();
            _stopwatch.Stop();

            IsRunning = false;
        }

        private void OnEventReceived(object sender, MidiEventReceivedEventArgs e)
        {
            _events.Add(new RecordingEvent(e.Event, _stopwatch.Elapsed));
        }

        #endregion

        #region IDisposable

        void Dispose(bool disposing)
        {
            if (_disposed)
                return;

            if (disposing)
            {
                Stop();
                _inputDevice.EventReceived -= OnEventReceived;
            }

            // TODO: free unmanaged resources (unmanaged objects) and override a finalizer below.
            // TODO: set large fields to null.

            _disposed = true;
        }

        // TODO: override a finalizer only if Dispose(bool disposing) above has code to free unmanaged resources.
        // ~Recording() {
        //   // Do not change this code. Put cleanup code in Dispose(bool disposing) above.
        //   Dispose(false);
        // }

        // This code added to correctly implement the disposable pattern.
        public void Dispose()
        {
            // Do not change this code. Put cleanup code in Dispose(bool disposing) above.
            Dispose(true);
            // TODO: uncomment the following line if the finalizer is overridden above.
            // GC.SuppressFinalize(this);
        }

        #endregion
    }
}
