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

        private readonly Dictionary<Playback, TimeSpanType?> _playbacks = new Dictionary<Playback, TimeSpanType?>();
        private readonly object _playbacksLock = new object();
        private readonly PlaybackCurrentTimeWatcherSettings _settings;

        private MidiClock _clock;
        private TimeSpan _pollingInterval = DefaultPollingInterval;
        private TimeSpanType _timeType = TimeSpanType.Midi;

        private bool _disposed = false;

        #endregion

        #region Constructor

        /// <summary>
        /// Initializes a new instance of the <see cref="PlaybackCurrentTimeWatcher"/>.
        /// </summary>
        /// <param name="settings">Settings for playbacks watching.</param>
        public PlaybackCurrentTimeWatcher(PlaybackCurrentTimeWatcherSettings settings = null)
        {
            _settings = settings ?? new PlaybackCurrentTimeWatcherSettings();
            CreateClock(PollingInterval);
        }

        #endregion

        #region Properties

        /// <summary>
        /// Gets the default instance of <see cref="PlaybackCurrentTimeWatcher"/>.
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

        /// <summary>
        /// Gets or sets the type of a playback's time to convert to in case of playback
        /// was added in the watcher via <see cref="AddPlayback(Playback)"/> method (without
        /// specifying desired time type). The default value is <see cref="TimeSpanType.Midi"/>.
        /// </summary>
        /// <remarks>
        /// The current time of a playback will be converted to this time type only if the playback
        /// was added in the watcher via <see cref="AddPlayback(Playback)"/> method. If
        /// <see cref="AddPlayback(Playback, TimeSpanType)"/> method was used, its second parameter
        /// overrides the global type defined by the <see cref="TimeType"/> property.
        /// </remarks>
        /// <exception cref="InvalidEnumArgumentException"><paramref name="value"/> specified an invalid value.</exception>
        public TimeSpanType TimeType
        {
            get { return _timeType; }
            set
            {
                ThrowIfArgument.IsInvalidEnumValue(nameof(value), value);

                _timeType = value;
            }
        }

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
        /// Adds a playback to the list of ones to watch current times of. The time will be reported
        /// in the specified type.
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
        /// Adds a playback to the list of ones to watch current times of. The time will be reported
        /// in the type defined by <see cref="TimeType"/> property.
        /// </summary>
        /// <param name="playback">Playback to watch current time of.</param>
        /// <exception cref="ArgumentNullException"><paramref name="playback"/> is <c>null</c>.</exception>
        /// <exception cref="ObjectDisposedException">The current <see cref="PlaybackCurrentTimeWatcher"/>
        /// is disposed.</exception>
        public void AddPlayback(Playback playback)
        {
            ThrowIfArgument.IsNull(nameof(playback), playback);
            EnsureIsNotDisposed();

            lock (_playbacksLock)
            {
                _playbacks[playback] = null;
            }
        }

        /// <summary>
        /// Sets the type the current time of the specified playback should be converted to.
        /// </summary>
        /// <param name="playback">Playback to set time type for.</param>
        /// <param name="timeType">Type to convert current time of the <paramref name="playback"/> to.</param>
        /// /// <exception cref="ArgumentNullException"><paramref name="playback"/> is <c>null</c>.</exception>
        /// <exception cref="InvalidEnumArgumentException"><paramref name="timeType"/> specified an invalid value.</exception>
        /// <exception cref="ObjectDisposedException">The current <see cref="PlaybackCurrentTimeWatcher"/>
        /// is disposed.</exception>
        /// <exception cref="ArgumentOutOfRangeException"><paramref name="playback"/> is not added to the current watcher.</exception>
        public void SetPlaybackTimeType(Playback playback, TimeSpanType timeType)
        {
            ThrowIfArgument.IsNull(nameof(playback), playback);
            ThrowIfArgument.IsInvalidEnumValue(nameof(timeType), timeType);
            EnsureIsNotDisposed();

            lock (_playbacksLock)
            {
                ThrowIfArgument.DoesntSatisfyCondition(nameof(playback), playback, p => _playbacks.ContainsKey(p), "Playback is not being watched.");
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
                    if (_settings.WatchOnlyRunningPlaybacks && !playback.Key.IsRunning)
                        continue;

                    var currentTime = playback.Key.GetCurrentTime(playback.Value ?? TimeType);
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
            _clock = new MidiClock(true, (_settings.ClockSettings ?? new MidiClockSettings()).CreateTickGeneratorCallback(), pollingInterval);
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

        private void DetachAllEventHandlers()
        {
            var currentTimeChanged = CurrentTimeChanged;
            if (currentTimeChanged == null)
                return;

            foreach (var d in currentTimeChanged.GetInvocationList())
            {
                currentTimeChanged -= d as EventHandler<PlaybackCurrentTimeChangedEventArgs>;
            }
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
                DetachAllEventHandlers();
            }

            _disposed = true;
        }

        #endregion
    }
}
