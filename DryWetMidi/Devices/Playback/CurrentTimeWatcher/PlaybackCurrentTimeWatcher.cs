using System;
using System.Collections.Generic;
using System.Linq;
using Melanchall.DryWetMidi.Common;
using Melanchall.DryWetMidi.Smf.Interaction;

namespace Melanchall.DryWetMidi.Devices
{
    public sealed class PlaybackCurrentTimeWatcher : IDisposable, IClockDrivenObject
    {
        #region Constants

        private static readonly TimeSpan DefaultPollingInterval = TimeSpan.FromMilliseconds(100);

        #endregion

        #region Events

        public event EventHandler<PlaybackCurrentTimeChangedEventArgs> CurrentTimeChanged;

        #endregion

        #region Fields

        private static readonly Lazy<PlaybackCurrentTimeWatcher> _lazy = new Lazy<PlaybackCurrentTimeWatcher>(() => new PlaybackCurrentTimeWatcher());

        private readonly Dictionary<Playback, TimeSpanType> _playbacks = new Dictionary<Playback, TimeSpanType>();
        private readonly object _playbacksLock = new object();
        private readonly MidiClockSettings _clockSettings;

        private MidiClock _clock;
        private TimeSpan _pollingInterval = DefaultPollingInterval;

        private bool _disposed = false;

        #endregion

        #region Constructor

        private PlaybackCurrentTimeWatcher(MidiClockSettings clockSettings = null)
        {
            _clockSettings = clockSettings ?? new MidiClockSettings();
            PollingInterval = DefaultPollingInterval;
        }

        #endregion

        #region Properties

        public static PlaybackCurrentTimeWatcher Instance { get { return _lazy.Value; } }

        public TimeSpan PollingInterval
        {
            get { return _pollingInterval; }
            set
            {
                _pollingInterval = value;

                RecreateClock();
            }
        }

        public IEnumerable<Playback> Playbacks
        {
            get
            {
                lock (_playbacksLock)
                {
                    return _playbacks.Keys;
                }
            }
        }

        public bool IsWatching => _clock?.IsRunning ?? false;

        #endregion

        #region Methods

        public void Start()
        {
            EnsureIsNotDisposed();

            _clock.Start();
        }

        public void Stop()
        {
            EnsureIsNotDisposed();

            _clock.Stop();
        }

        public void AddPlayback(Playback playback, TimeSpanType timeType)
        {
            ThrowIfArgument.IsNull(nameof(playback), playback);
            ThrowIfArgument.IsInvalidEnumValue(nameof(timeType), timeType);
            EnsureIsNotDisposed();

            lock (_playbacksLock)
            {
                _playbacks[playback] = timeType;
            }
        }

        public void RemovePlayback(Playback playback)
        {
            ThrowIfArgument.IsNull(nameof(playback), playback);
            EnsureIsNotDisposed();

            lock (_playbacksLock)
            {
                _playbacks.Remove(playback);

                if (!_playbacks.Any())
                    RecreateClock();
            }
        }

        public void RemoveAllPlaybacks()
        {
            EnsureIsNotDisposed();

            lock (_playbacksLock)
            {
                _playbacks.Clear();
            }

            RecreateClock();
        }

        private void OnTick(object sender, TickedEventArgs e)
        {
            if (_disposed || !IsWatching)
                return;

            var times = new List<PlaybackCurrentTime>();

            lock (_playbacksLock)
            {
                foreach (var playback in _playbacks)
                {
                    var currentTime = playback.Key.GetCurrentTime(playback.Value);
                    times.Add(new PlaybackCurrentTime(playback.Key, currentTime));
                }
            }

            if (times.Any())
                OnCurrentTimeChanged(times);
        }

        private void OnCurrentTimeChanged(IEnumerable<PlaybackCurrentTime> times)
        {
            CurrentTimeChanged?.Invoke(this, new PlaybackCurrentTimeChangedEventArgs(times));
        }

        private void EnsureIsNotDisposed()
        {
            if (_disposed)
                throw new ObjectDisposedException("Playback current time watcher is disposed.");
        }

        private void DisposeClock()
        {
            if (_clock == null)
                return;

            _clock.Stop();
            _clock.Ticked -= OnTick;
            _clock.Dispose();
        }

        private void CreateClock(TimeSpan pollingInterval)
        {
            _clock = new MidiClock(true, _clockSettings.CreateTickGeneratorCallback(pollingInterval));
            _clock.Ticked += OnTick;
        }

        private void RecreateClock()
        {
            var isWatching = IsWatching;

            DisposeClock();
            CreateClock(PollingInterval);

            if (isWatching)
                Start();
        }

        #endregion

        #region IClockDrivenObject

        public void TickClock()
        {
            EnsureIsNotDisposed();

            _clock?.Tick();
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
                DisposeClock();
            }

            _disposed = true;
        }

        #endregion
    }
}
