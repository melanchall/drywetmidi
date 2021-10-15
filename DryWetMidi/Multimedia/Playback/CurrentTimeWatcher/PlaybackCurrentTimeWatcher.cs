using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using Melanchall.DryWetMidi.Common;
using Melanchall.DryWetMidi.Interaction;

namespace Melanchall.DryWetMidi.Multimedia
{
    /// <summary>
    /// Provides notifications about playback's current time changed.
    /// </summary>
    public sealed class PlaybackCurrentTimeWatcher : IDisposable, IClockDrivenObject
    {
        #region Constants

        private static readonly TimeSpan DefaultPollingInterval = TimeSpan.FromMilliseconds(100);

        #endregion

        #region Events

        /// <summary>
        /// Occurs when current times of playbacks are changed.
        /// </summary>
        public event EventHandler<PlaybackCurrentTimeChangedEventArgs> CurrentTimeChanged;

        #endregion

        #region Fields

        private static readonly Lazy<PlaybackCurrentTimeWatcher> _lazyInstance = new Lazy<PlaybackCurrentTimeWatcher>(() => new PlaybackCurrentTimeWatcher());

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

        /// <summary>
        /// Gets the instance of <see cref="PlaybackCurrentTimeWatcher"/>.
        /// </summary>
        public static PlaybackCurrentTimeWatcher Instance { get { return _lazyInstance.Value; } }

        /// <summary>
        /// Gets or sets the interval of playbacks current times polling.
        /// </summary>
        public TimeSpan PollingInterval
        {
            get { return _pollingInterval; }
            set
            {
                _pollingInterval = value;

                RecreateClock();
            }
        }

        /// <summary>
        /// Gets playbacks the watcher polls current time of.
        /// </summary>
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

        /// <summary>
        /// Gets a value indicating whether the watcher polls playbacks current times or not.
        /// </summary>
        public bool IsWatching => _clock?.IsRunning ?? false;

        #endregion

        #region Methods

        /// <summary>
        /// Starts current times watching.
        /// </summary>
        /// <exception cref="ObjectDisposedException">The current <see cref="PlaybackCurrentTimeWatcher"/>
        /// is disposed.</exception>
        public void Start()
        {
            EnsureIsNotDisposed();

            _clock.Start();
        }

        /// <summary>
        /// Stops current times watching.
        /// </summary>
        /// <exception cref="ObjectDisposedException">The current <see cref="PlaybackCurrentTimeWatcher"/>
        /// is disposed.</exception>
        public void Stop()
        {
            EnsureIsNotDisposed();

            _clock.Stop();
        }

        /// <summary>
        /// Adds a playback to the list of ones to watch current times of.
        /// </summary>
        /// <param name="playback">Playback to watch current time of.</param>
        /// <param name="timeType">Type of current time to convert to.</param>
        /// <exception cref="ArgumentNullException"><paramref name="playback"/> is <c>null</c>.</exception>
        /// <exception cref="InvalidEnumArgumentException"><paramref name="timeType"/> specified an invalid value.</exception>
        /// <exception cref="ObjectDisposedException">The current <see cref="PlaybackCurrentTimeWatcher"/>
        /// is disposed.</exception>
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

        /// <summary>
        /// Removes a playback from the list of ones to watch current times of.
        /// </summary>
        /// <param name="playback">Playback to exclude current time watching of.</param>
        /// <exception cref="ArgumentNullException"><paramref name="playback"/> is <c>null</c>.</exception>
        /// <exception cref="ObjectDisposedException">The current <see cref="PlaybackCurrentTimeWatcher"/>
        /// is disposed.</exception>
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

        /// <summary>
        /// Removes all playbacks from the list of ones to watch current times of.
        /// </summary>
        /// <exception cref="ObjectDisposedException">The current <see cref="PlaybackCurrentTimeWatcher"/>
        /// is disposed.</exception>
        public void RemoveAllPlaybacks()
        {
            EnsureIsNotDisposed();

            lock (_playbacksLock)
            {
                _playbacks.Clear();
            }

            RecreateClock();
        }

        private void OnTick(object sender, EventArgs e)
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
            _clock = new MidiClock(true, _clockSettings.CreateTickGeneratorCallback(), pollingInterval);
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

        /// <summary>
        /// Ticks internal clock.
        /// </summary>
        /// <exception cref="ObjectDisposedException">The current <see cref="PlaybackCurrentTimeWatcher"/>
        /// is disposed.</exception>
        public void TickClock()
        {
            EnsureIsNotDisposed();

            _clock?.Tick();
        }

        #endregion

        #region IDisposable

        /// <summary>
        /// Releases all resources used by the current <see cref="PlaybackCurrentTimeWatcher"/>.
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
                DisposeClock();
            }

            _disposed = true;
        }

        #endregion
    }
}
