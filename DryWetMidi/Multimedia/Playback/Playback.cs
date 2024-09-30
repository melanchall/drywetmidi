using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Threading;
using Melanchall.DryWetMidi.Common;
using Melanchall.DryWetMidi.Core;
using Melanchall.DryWetMidi.Interaction;

namespace Melanchall.DryWetMidi.Multimedia
{
    /// <summary>
    /// Provides a way to play MIDI data through the specified output MIDI device. More info in the
    /// <see href="xref:a_playback_overview">Playback</see> article.
    /// </summary>
    /// <remarks>
    /// You can derive from the <see cref="Playback"/> class to create your own playback logic.
    /// Please see <see href="xref:a_playback_custom">Custom playback</see> article to learn more.
    /// </remarks>
    public class Playback : IDisposable, IClockDrivenObject
    {
        #region Constants

        private static readonly TimeSpan ClockInterval = TimeSpan.FromMilliseconds(1);

        private static readonly TimeSpan MinPlaybackTime = TimeSpan.Zero;
        private static readonly TimeSpan MaxPlaybackTime = TimeSpan.MaxValue;

        #endregion

        #region Events

        /// <summary>
        /// Occurs when playback started via <see cref="Start"/> or <see cref="Play"/> methods.
        /// </summary>
        public event EventHandler Started;

        /// <summary>
        /// Occurs when playback stopped via <see cref="Stop"/> method.
        /// </summary>
        public event EventHandler Stopped;

        /// <summary>
        /// Occurs when playback finished, i.e. last event has been played and no
        /// need to restart playback due to value of the <see cref="Loop"/>.
        /// </summary>
        public event EventHandler Finished;

        /// <summary>
        /// Occurs when playback started new cycle of the data playing in case of <see cref="Loop"/> set to <c>true</c>.
        /// </summary>
        public event EventHandler RepeatStarted;

        /// <summary>
        /// Occurs when notes started to play. It will raised if playback's cursor
        /// gets in to notes.
        /// </summary>
        public event EventHandler<NotesEventArgs> NotesPlaybackStarted;

        /// <summary>
        /// Occurs when notes finished to play. It will raised if playback's cursor
        /// gets out from notes.
        /// </summary>
        public event EventHandler<NotesEventArgs> NotesPlaybackFinished;

        /// <summary>
        /// Occurs when MIDI event played.
        /// </summary>
        public event EventHandler<MidiEventPlayedEventArgs> EventPlayed;

        /// <summary>
        /// Occurs when an error got from output device.
        /// </summary>
        public event EventHandler<ErrorOccurredEventArgs> DeviceErrorOccurred;

        #endregion

        #region Fields

        private readonly PlaybackEvent[] _playbackEvents;
        private int _playbackEventsPosition = -1;

        private readonly TimeSpan _duration;
        private readonly long _durationInTicks;

        private ITimeSpan _playbackStart;
        private ITimeSpan _playbackEnd;
        private TimeSpan _playbackEndMetric = MaxPlaybackTime;

        private bool _hasBeenStarted;

        private readonly MidiClock _clock;

        private readonly ConcurrentDictionary<NotePlaybackEventMetadata, byte> _activeNotesMetadata = new ConcurrentDictionary<NotePlaybackEventMetadata, byte>();
        private readonly HashSet<NotePlaybackEventMetadata> _notesMetadataHashSet = new HashSet<NotePlaybackEventMetadata>();
        private readonly NotePlaybackEventMetadata[] _notesMetadata;

        private readonly PlaybackDataTracker _playbackDataTracker;

        private bool _disposed = false;

        #endregion

        #region Constructor

        /// <summary>
        /// Initializes a new instance of the <see cref="Playback"/> with the specified
        /// collection of timed objects and tempo map.
        /// </summary>
        /// <param name="timedObjects">Collection of timed objects to play.</param>
        /// <param name="tempoMap">Tempo map used to calculate events times.</param>
        /// <param name="playbackSettings">Settings according to which a playback should be created.</param>
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
        public Playback(IEnumerable<ITimedObject> timedObjects, TempoMap tempoMap, PlaybackSettings playbackSettings = null)
        {
            ThrowIfArgument.IsNull(nameof(timedObjects), timedObjects);
            ThrowIfArgument.IsNull(nameof(tempoMap), tempoMap);

            playbackSettings = playbackSettings ?? new PlaybackSettings();

            TempoMap = tempoMap;

            var noteDetectionSettings = playbackSettings.NoteDetectionSettings ?? new NoteDetectionSettings();
            _playbackDataTracker = new PlaybackDataTracker(TempoMap);

            _playbackEvents = GetPlaybackEvents(timedObjects.GetNotesAndTimedEventsLazy(noteDetectionSettings, true), tempoMap);
            MoveToNextPlaybackEvent();

            var lastPlaybackEvent = _playbackEvents.LastOrDefault();
            _duration = lastPlaybackEvent?.Time ?? TimeSpan.Zero;
            _durationInTicks = lastPlaybackEvent?.RawTime ?? 0;

            _notesMetadata = _notesMetadataHashSet.ToArray();

            var clockSettings = playbackSettings.ClockSettings ?? new MidiClockSettings();
            _clock = new MidiClock(false, clockSettings.CreateTickGeneratorCallback(), ClockInterval);
            _clock.Ticked += OnClockTicked;

            Snapping = new PlaybackSnapping(_playbackEvents, tempoMap);
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="Playback"/> with the specified
        /// collection of timed objects, tempo map and output MIDI device to play events through.
        /// </summary>
        /// <param name="timedObjects">Collection of timed objects to play.</param>
        /// <param name="tempoMap">Tempo map used to calculate events times.</param>
        /// <param name="outputDevice">Output MIDI device to play <paramref name="timedObjects"/> through.</param>
        /// <param name="playbackSettings">Settings according to which a playback should be created.</param>
        /// <exception cref="ArgumentNullException">
        /// <para>One of the following errors occurred:</para>
        /// <list type="bullet">
        /// <item>
        /// <description><paramref name="timedObjects"/> is <c>null</c>.</description>
        /// </item>
        /// <item>
        /// <description><paramref name="tempoMap"/> is <c>null</c>.</description>
        /// </item>
        /// <item>
        /// <description><paramref name="outputDevice"/> is <c>null</c>.</description>
        /// </item>
        /// </list>
        /// </exception>
        public Playback(IEnumerable<ITimedObject> timedObjects, TempoMap tempoMap, IOutputDevice outputDevice, PlaybackSettings playbackSettings = null)
            : this(timedObjects, tempoMap, playbackSettings)
        {
            ThrowIfArgument.IsNull(nameof(outputDevice), outputDevice);

            OutputDevice = outputDevice;
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
        /// Gets or sets the output MIDI device to play MIDI data through.
        /// </summary>
        public IOutputDevice OutputDevice { get; set; }

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
        /// Gets or sets a value indicating whether currently playing notes must be stopped
        /// on playback stop or not.
        /// </summary>
        public bool InterruptNotesOnStop { get; set; } = true;

        /// <summary>
        /// Gets or sets a value indicating whether notes must be tracked or not. If <c>false</c>, notes
        /// will be treated as just Note On/Note Off events. The default value is <c>false</c>. More info in the
        /// <see href="xref:a_playback_datatrack#notes-tracking">Data tracking: Notes tracking</see> article.
        /// </summary>
        public bool TrackNotes { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether program must be tracked or not. If <c>true</c>, any jump
        /// in time will force playback send <see cref="ProgramChangeEvent"/> corresponding to the program at new time,
        /// if needed. The default value is <c>false</c>. More info in the
        /// <see href="xref:a_playback_datatrack#midi-parameters-values-tracking">Data tracking: MIDI parameters values tracking</see>
        /// article.
        /// </summary>
        public bool TrackProgram
        {
            get { return _playbackDataTracker.TrackProgram; }
            set
            {
                if (_playbackDataTracker.TrackProgram == value)
                    return;

                _playbackDataTracker.TrackProgram = value;

                if (value)
                    SendTrackedData(PlaybackDataTracker.TrackedParameterType.Program);
            }
        }

        /// <summary>
        /// Gets or sets a value indicating whether pitch value must be tracked or not. If <c>true</c>, any jump
        /// in time will force playback send <see cref="PitchBendEvent"/> corresponding to the pitch value at new time,
        /// if needed. The default value is <c>false</c>. More info in the
        /// <see href="xref:a_playback_datatrack#midi-parameters-values-tracking">Data tracking: MIDI parameters values tracking</see>
        /// article.
        /// </summary>
        public bool TrackPitchValue
        {
            get { return _playbackDataTracker.TrackPitchValue; }
            set
            {
                if (_playbackDataTracker.TrackPitchValue == value)
                    return;

                _playbackDataTracker.TrackPitchValue = value;

                if (value)
                    SendTrackedData(PlaybackDataTracker.TrackedParameterType.PitchValue);
            }
        }

        /// <summary>
        /// Gets or sets a value indicating whether controller values must be tracked or not. If <c>true</c>, any jump
        /// in time will force playback send <see cref="ControlChangeEvent"/> corresponding to the controller value at new time,
        /// if needed. The default value is <c>false</c>. More info in the
        /// <see href="xref:a_playback_datatrack#midi-parameters-values-tracking">Data tracking: MIDI parameters values tracking</see>
        /// article.
        /// </summary>
        public bool TrackControlValue
        {
            get { return _playbackDataTracker.TrackControlValue; }
            set
            {
                if (_playbackDataTracker.TrackControlValue == value)
                    return;

                _playbackDataTracker.TrackControlValue = value;

                if (value)
                    SendTrackedData(PlaybackDataTracker.TrackedParameterType.ControlValue);
            }
        }

        /// <summary>
        /// Gets or sets the speed of events playing. <c>1</c> means normal speed. For example, to play
        /// events twice slower this property should be set to <c>0.5</c>. Value of <c>2</c> will make playback
        /// twice faster.
        /// </summary>
        /// <exception cref="ArgumentOutOfRangeException"><paramref name="value"/> is zero or negative.</exception>
        /// <exception cref="ObjectDisposedException">The current <see cref="Playback"/> is disposed.</exception>
        /// <example>
        /// <para>
        /// Example below shows how you can use <c>Speed</c> property to set new BPM (if original data doesn't
        /// have tempo changes):
        /// </para>
        /// <code language="csharp">
        /// var tempoMap = midiFile.GetTempoMap();
        /// var originalBpm = tempoMap.GetTempoAtTime((MidiTimeSpan)0).BeatsPerMinute;
        /// 
        /// var newBpm = 240;
        /// playback.Speed = newBpm / originalBpm;
        /// </code>
        /// <para>
        /// We want to have BPM of <c>240</c> here so we just divide it by original BPM value. If
        /// original BPM was <c>120</c>, we'll get <c>2</c> which is exactly what we want - double the speed.
        /// If original value is <c>480</c>, we'll get <c>0.5</c> which means speed will be slowed down
        /// by two times.
        /// </para>
        /// </example>
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

        /// <summary>
        /// Gets an object to manage playback's snap points.
        /// </summary>
        public PlaybackSnapping Snapping { get; }

        /// <summary>
        /// Gets or sets callback used to process note to be played.
        /// </summary>
        /// <example>
        /// <para>
        /// In the following example every note to be played will be transposed by <c>10</c> half-steps up:
        /// </para>
        /// <code language="csharp">
        /// var playback = midiFile.GetPlayback(outputDevice);
        /// playback.NoteCallback = (rawNoteData, rawTime, rawLength, playbackTime) =>
        ///     new NotePlaybackData(
        ///         (SevenBitNumber)(rawNoteData.NoteNumber + 10),
        ///         rawNoteData.Velocity,     // leave velocity as is
        ///         rawNoteData.OffVelocity,  // leave off velocity as is
        ///         rawNoteData.Channel);     // leave channel as is
        /// 
        /// playback.Start();
        /// </code>
        /// <para>
        /// Next example shows how you can filter out notes with velocity below <c>100</c>:
        /// </para>
        /// <code language="csharp">
        /// playback.NoteCallback = (rawNoteData, rawTime, rawLength, playbackTime) =>
        ///     rawNoteData.Velocity &lt; 100
        ///         ? null
        ///         : rawNoteData;
        /// </code>
        /// </example>
        public NoteCallback NoteCallback { get; set; }

        /// <summary>
        /// Gets or sets callback used to process MIDI event to be played.
        /// </summary>
        /// <remarks>
        /// Note that <see cref="NoteOnEvent"/> and <see cref="NoteOffEvent"/> won't be passed
        /// to <see cref="EventCallback"/>. Since processing of a note requires syncing between pairs
        /// of corresponding events, such pairs are handled by <see cref="NoteCallback"/> to be sure
        /// that a note's integrity is not broken.
        /// </remarks>
        /// <example>
        /// <para>
        /// The following example filters out all <see cref="ProgramChangeEvent">Program Change</see> events:
        /// </para>
        /// <code language="csharp">
        /// playback.EventCallback = (midiEvent, rawTime, playbackTime) =>
        ///     midiEvent.EventType == MidiEventType.ProgramChange
        ///         ? null
        ///         : midiEvent;
        /// </code>
        /// <para>
        /// Next example shows how to replace program <c>1</c> in all Program Change events to program <c>2</c>:
        /// </para>
        /// <code language="csharp">
        /// playback.EventCallback = (midiEvent, rawTime, playbackTime) =>
        ///     ((midiEvent is ProgramChangeEvent programChangeEvent) &amp;&amp; programChangeEvent.ProgramNumber == 1)
        ///         ? new ProgramChangeEvent((SevenBitNumber)2) { Channel = programChangeEvent.Channel }
        ///         : midiEvent;
        /// </code>
        /// </example>
        public EventCallback EventCallback { get; set; }

        /// <summary>
        /// Gets or sets the start time of the current playback. It defines start time of
        /// the region to play back.
        /// </summary>
        /// <remarks>
        /// In conjunction with <see cref="PlaybackEnd"/> you can define a region within the
        /// current playback which will be played. If you set the property to <c>null</c> the
        /// start of playback (zero) will be used.
        /// </remarks>
        public ITimeSpan PlaybackStart
        {
            get { return _playbackStart; }
            set { _playbackStart = value; }
        }

        /// <summary>
        /// Gets or sets the end time of the current playback. It defines end time of
        /// the region to play back.
        /// </summary>
        /// <remarks>
        /// In conjunction with <see cref="PlaybackStart"/> you can define a region within the
        /// current playback which will be played. If you set the property to <c>null</c> the
        /// end of playback will be used.
        /// </remarks>
        public ITimeSpan PlaybackEnd
        {
            get { return _playbackEnd; }
            set
            {
                _playbackEnd = value;
                _playbackEndMetric = _playbackEnd != null
                    ? (TimeSpan)TimeConverter.ConvertTo<MetricTimeSpan>(_playbackEnd, TempoMap)
                    : MaxPlaybackTime;
            }
        }

        #endregion

        #region Methods

        /// <summary>
        /// Retrieves the duration of the playback in the specified format.
        /// </summary>
        /// <param name="durationType">Type that will represent the duration.</param>
        /// <returns>The duration of the playback as an instance of time span defined by
        /// <paramref name="durationType"/>.</returns>
        /// <exception cref="InvalidEnumArgumentException"><paramref name="durationType"/>
        /// specified an invalid value.</exception>
        public ITimeSpan GetDuration(TimeSpanType durationType)
        {
            ThrowIfArgument.IsInvalidEnumValue(nameof(durationType), durationType);

            return TimeConverter.ConvertTo((MetricTimeSpan)_duration, durationType, TempoMap);
        }

        /// <summary>
        /// Retrieves the duration of the playback in the specified format.
        /// </summary>
        /// <typeparam name="TTimeSpan">Type that will represent the duration.</typeparam>
        /// <returns>The duration of the playback as an instance of
        /// <typeparamref name="TTimeSpan"/>.</returns>
        public TTimeSpan GetDuration<TTimeSpan>()
            where TTimeSpan : ITimeSpan
        {
            return TimeConverter.ConvertTo<TTimeSpan>((MetricTimeSpan)_duration, TempoMap);
        }

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

            if (!_hasBeenStarted)
                MoveToStart();

            OutputDevice?.PrepareForEventsSending();
            SendTrackedData();
            StopStartNotes();
            _clock.Start();

            _hasBeenStarted = true;
            OnStarted();
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

            if (InterruptNotesOnStop)
            {
                var currentTime = _clock.CurrentTime;

                var notes = new List<Note>();

                foreach (var noteMetadata in _activeNotesMetadata.Keys)
                {
                    Note note;
                    if (TryPlayNoteEvent(noteMetadata, false, currentTime, out note))
                        notes.Add(note);
                }

                OnNotesPlaybackFinished(notes.ToArray());

                _activeNotesMetadata.Clear();
            }

            OnStopped();
        }

        /// <summary>
        /// Starts playing of the MIDI data. This method will block execution of a program until all
        /// MIDI data is played.
        /// </summary>
        /// <remarks>
        /// If <see cref="Loop"/> is set to <c>true</c>, this method will execute forever.
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
        /// Sets playback position to the time of the specified snap point.
        /// </summary>
        /// <param name="snapPoint">Snap point to move to.</param>
        /// <returns><c>true</c> if playback position successfully changed to the time of <paramref name="snapPoint"/>;
        /// otherwise, <c>false</c>.</returns>
        /// <exception cref="ArgumentNullException"><paramref name="snapPoint"/> is <c>null</c>.</exception>
        /// <exception cref="ObjectDisposedException">The current <see cref="Playback"/> is disposed.</exception>
        /// <exception cref="MidiDeviceException">An error occurred on device.</exception>
        public bool MoveToSnapPoint(SnapPoint snapPoint)
        {
            ThrowIfArgument.IsNull(nameof(snapPoint), snapPoint);
            EnsureIsNotDisposed();

            if (!snapPoint.IsEnabled)
                return false;

            return TryToMoveToSnapPoint(snapPoint);
        }

        /// <summary>
        /// Sets playback position to the time of first snap point.
        /// </summary>
        /// <returns><c>true</c> if playback position successfully changed to the time of first snap point;
        /// otherwise, <c>false</c>.</returns>
        /// <exception cref="ObjectDisposedException">The current <see cref="Playback"/> is disposed.</exception>
        /// <exception cref="MidiDeviceException">An error occurred on device.</exception>
        public bool MoveToFirstSnapPoint()
        {
            EnsureIsNotDisposed();

            var snapPoint = Snapping.GetNextSnapPoint(TimeSpan.Zero);
            return TryToMoveToSnapPoint(snapPoint);
        }

        /// <summary>
        /// Sets playback position to the time of first snap point holding the specified data.
        /// </summary>
        /// <param name="data">Data of a snap point to move to.</param>
        /// <returns><c>true</c> if playback position successfully changed to the time of first snap point
        /// with the specified data; otherwise, <c>false</c>.</returns>
        /// <exception cref="ObjectDisposedException">The current <see cref="Playback"/> is disposed.</exception>
        /// <exception cref="MidiDeviceException">An error occurred on device.</exception>
        public bool MoveToFirstSnapPoint<TData>(TData data)
        {
            EnsureIsNotDisposed();

            var snapPoint = Snapping.GetNextSnapPoint(TimeSpan.Zero, data);
            return TryToMoveToSnapPoint(snapPoint);
        }

        /// <summary>
        /// Sets playback position to the time of the previous snap point (relative to the current
        /// time of playback) that belongs to the specified <see cref="SnapPointsGroup"/>.
        /// </summary>
        /// <param name="snapPointsGroup"><see cref="SnapPointsGroup"/> that defines snap points to
        /// select the one from.</param>
        /// <returns><c>true</c> if playback position successfully changed to the time of a previous snap point
        /// within <paramref name="snapPointsGroup"/>; otherwise, <c>false</c>.</returns>
        /// <exception cref="ArgumentNullException"><paramref name="snapPointsGroup"/> is <c>null</c>.</exception>
        /// <exception cref="ObjectDisposedException">The current <see cref="Playback"/> is disposed.</exception>
        /// <exception cref="MidiDeviceException">An error occurred on device.</exception>
        public bool MoveToPreviousSnapPoint(SnapPointsGroup snapPointsGroup)
        {
            ThrowIfArgument.IsNull(nameof(snapPointsGroup), snapPointsGroup);
            EnsureIsNotDisposed();

            var snapPoint = Snapping.GetPreviousSnapPoint(_clock.CurrentTime, snapPointsGroup);
            return TryToMoveToSnapPoint(snapPoint);
        }

        /// <summary>
        /// Sets playback position to the time of previous snap point (relative to the current
        /// time of playback).
        /// </summary>
        /// <returns><c>true</c> if playback position successfully changed to the time of a previous snap point;
        /// otherwise, <c>false</c>.</returns>
        /// <exception cref="ObjectDisposedException">The current <see cref="Playback"/> is disposed.</exception>
        /// <exception cref="MidiDeviceException">An error occurred on device.</exception>
        public bool MoveToPreviousSnapPoint()
        {
            EnsureIsNotDisposed();

            var snapPoint = Snapping.GetPreviousSnapPoint(_clock.CurrentTime);
            return TryToMoveToSnapPoint(snapPoint);
        }

        /// <summary>
        /// Sets playback position to the time of previous snap point (relative to the current
        /// time of playback) holding the specified data.
        /// </summary>
        /// <param name="data">Data of a snap point to move to.</param>
        /// <returns><c>true</c> if playback position successfully changed to the time of a previous snap point
        /// with the specified data; otherwise, <c>false</c>.</returns>
        /// <exception cref="ObjectDisposedException">The current <see cref="Playback"/> is disposed.</exception>
        /// <exception cref="MidiDeviceException">An error occurred on device.</exception>
        public bool MoveToPreviousSnapPoint<TData>(TData data)
        {
            EnsureIsNotDisposed();

            var snapPoint = Snapping.GetPreviousSnapPoint(_clock.CurrentTime, data);
            return TryToMoveToSnapPoint(snapPoint);
        }

        /// <summary>
        /// Sets playback position to the time of next snap point (relative to the current
        /// time of playback) that belongs to the specified <see cref="SnapPointsGroup"/>.
        /// </summary>
        /// <param name="snapPointsGroup"><see cref="SnapPointsGroup"/> that defines snap points to
        /// select the one from.</param>
        /// <returns><c>true</c> if playback position successfully changed to the time of a next snap point
        /// within <paramref name="snapPointsGroup"/>; otherwise, <c>false</c>.</returns>
        /// <exception cref="ArgumentNullException"><paramref name="snapPointsGroup"/> is <c>null</c>.</exception>
        /// <exception cref="ObjectDisposedException">The current <see cref="Playback"/> is disposed.</exception>
        /// <exception cref="MidiDeviceException">An error occurred on device.</exception>
        public bool MoveToNextSnapPoint(SnapPointsGroup snapPointsGroup)
        {
            ThrowIfArgument.IsNull(nameof(snapPointsGroup), snapPointsGroup);
            EnsureIsNotDisposed();

            var snapPoint = Snapping.GetNextSnapPoint(_clock.CurrentTime, snapPointsGroup);
            return TryToMoveToSnapPoint(snapPoint);
        }

        /// <summary>
        /// Sets playback position to the time of next snap point (relative to the current
        /// time of playback).
        /// </summary>
        /// <returns><c>true</c> if playback position successfully changed to the time of a next snap point;
        /// otherwise, <c>false</c>.</returns>
        /// <exception cref="ObjectDisposedException">The current <see cref="Playback"/> is disposed.</exception>
        /// <exception cref="MidiDeviceException">An error occurred on device.</exception>
        public bool MoveToNextSnapPoint()
        {
            EnsureIsNotDisposed();

            var snapPoint = Snapping.GetNextSnapPoint(_clock.CurrentTime);
            return TryToMoveToSnapPoint(snapPoint);
        }

        /// <summary>
        /// Sets playback position to the time of next snap point (relative to the current
        /// time of playback) holding the specified data.
        /// </summary>
        /// <returns><c>true</c> if playback position successfully changed to the time of a next snap point
        /// with the specified data; otherwise, <c>false</c>.</returns>
        /// <exception cref="ObjectDisposedException">The current <see cref="Playback"/> is disposed.</exception>
        /// <exception cref="MidiDeviceException">An error occurred on device.</exception>
        public bool MoveToNextSnapPoint<TData>(TData data)
        {
            EnsureIsNotDisposed();

            var snapPoint = Snapping.GetNextSnapPoint(_clock.CurrentTime, data);
            return TryToMoveToSnapPoint(snapPoint);
        }

        /// <summary>
        /// Sets playback position to the beginning of the MIDI data.
        /// </summary>
        /// <exception cref="ObjectDisposedException">The current <see cref="Playback"/> is disposed.</exception>
        /// <exception cref="MidiDeviceException">An error occurred on device.</exception>
        public void MoveToStart()
        {
            EnsureIsNotDisposed();

            MoveToTime(PlaybackStart ?? new MetricTimeSpan());
        }

        /// <summary>
        /// Sets playback position to the specified time from the beginning of the MIDI data.
        /// </summary>
        /// <param name="time">Time from the beginning of the MIDI data to set playback position to.</param>
        /// <exception cref="ArgumentNullException"><paramref name="time"/> is <c>null</c>.</exception>
        /// <exception cref="ObjectDisposedException">The current <see cref="Playback"/> is disposed.</exception>
        /// <exception cref="MidiDeviceException">An error occurred on device.</exception>
        public void MoveToTime(ITimeSpan time)
        {
            MoveToTime(time, 0, _playbackEvents.Length - 1);
        }

        /// <summary>
        /// Shifts playback position forward by the specified step.
        /// </summary>
        /// <param name="step">Amount of time to shift playback position by.</param>
        /// <exception cref="ArgumentNullException"><paramref name="step"/> is <c>null</c>.</exception>
        /// <exception cref="ObjectDisposedException">The current <see cref="Playback"/> is disposed.</exception>
        /// <exception cref="MidiDeviceException">An error occurred on device.</exception>
        public void MoveForward(ITimeSpan step)
        {
            ThrowIfArgument.IsNull(nameof(step), step);
            EnsureIsNotDisposed();

            var currentTime = (MetricTimeSpan)_clock.CurrentTime;
            MoveToTime(
                currentTime.Add(step, TimeSpanMode.TimeLength),
                _playbackEventsPosition,
                _playbackEvents.Length - 1);
        }

        /// <summary>
        /// Shifts playback position back by the specified step.
        /// </summary>
        /// <param name="step">Amount of time to shift playback position by.</param>
        /// <exception cref="ArgumentNullException"><paramref name="step"/> is <c>null</c>.</exception>
        /// <exception cref="ObjectDisposedException">The current <see cref="Playback"/> is disposed.</exception>
        /// <exception cref="MidiDeviceException">An error occurred on device.</exception>
        public void MoveBack(ITimeSpan step)
        {
            ThrowIfArgument.IsNull(nameof(step), step);
            EnsureIsNotDisposed();

            var currentTime = (MetricTimeSpan)_clock.CurrentTime;
            MoveToTime(
                TimeConverter.ConvertTo<MetricTimeSpan>(step, TempoMap) > currentTime
                    ? new MetricTimeSpan()
                    : currentTime.Subtract(step, TimeSpanMode.TimeLength),
                0,
                _playbackEventsPosition);
        }

        /// <summary>
        /// Tries to play the specified MIDI event. By default just sends the event to output device
        /// returning <c>true</c>.
        /// </summary>
        /// <remarks>
        /// Please see <see href="xref:a_playback_custom">Custom playback</see> article to learn more
        /// about how to use <paramref name="metadata"/> parameter.
        /// </remarks>
        /// <param name="midiEvent">MIDI event to try to play.</param>
        /// <param name="metadata">Metadata attached to <paramref name="midiEvent"/>.</param>
        /// <returns><c>true</c> if <paramref name="midiEvent"/> was played; otherwise, <c>false</c>.</returns>
        protected virtual bool TryPlayEvent(MidiEvent midiEvent, object metadata)
        {
            OutputDevice?.SendEvent(midiEvent);
            return true;
        }

        /// <summary>
        /// Returns collection of <see cref="TimedEvent"/> representing the specified timed object.
        /// </summary>
        /// <remarks>
        /// The method can be useful in case of <see href="xref:a_playback_custom">custom playback</see> and
        /// custom input object.
        /// </remarks>
        /// <param name="timedObject">Timed object to get collection of <see cref="TimedEvent"/> from.</param>
        /// <returns>Collection of <see cref="TimedEvent"/> representing the <paramref name="timedObject"/>.</returns>
        protected virtual IEnumerable<TimedEvent> GetTimedEvents(ITimedObject timedObject)
        {
            return Enumerable.Empty<TimedEvent>();
        }

        private bool TryToMoveToSnapPoint(SnapPoint snapPoint)
        {
            if (snapPoint != null)
                MoveToTime((MetricTimeSpan)snapPoint.Time);

            return snapPoint != null;
        }

        private void SendTrackedData(PlaybackDataTracker.TrackedParameterType trackedParameterType = PlaybackDataTracker.TrackedParameterType.All)
        {
            foreach (var eventWithMetadata in _playbackDataTracker.GetEventsAtTime(_clock.CurrentTime, trackedParameterType))
            {
                PlayEvent(eventWithMetadata.Event, eventWithMetadata.Metadata);
            }
        }

        private void StopStartNotes()
        {
            if (!TrackNotes)
                return;

            var notesToPlay = GetNotesMetadataAtCurrentTime();
            var onNotesMetadata = notesToPlay.Any()
                ? notesToPlay.Where(n => !_activeNotesMetadata.Keys.Contains(n)).ToArray()
                : new NotePlaybackEventMetadata[0];
            var offNotesMetadata = notesToPlay.Any()
                ? _activeNotesMetadata.Keys.Where(n => !notesToPlay.Contains(n)).ToArray()
                : _activeNotesMetadata.Keys;

            OutputDevice?.PrepareForEventsSending();

            var notes = new List<Note>();
            var currentTime = _clock.CurrentTime;

            Note note;

            foreach (var noteMetadata in offNotesMetadata)
            {
                TryPlayNoteEvent(noteMetadata, false, currentTime, out note);
                notes.Add(note);
            }

            OnNotesPlaybackFinished(notes.ToArray());
            notes.Clear();

            foreach (var noteMetadata in onNotesMetadata)
            {
                TryPlayNoteEvent(noteMetadata, true, currentTime, out note);
                notes.Add(note);
            }

            OnNotesPlaybackStarted(notes.ToArray());
        }

        private ICollection<NotePlaybackEventMetadata> GetNotesMetadataAtCurrentTime()
        {
            var currentTime = _clock.CurrentTime;

            int firstIndex;
            MathUtilities.GetLastElementBelowThreshold(
                _notesMetadata,
                currentTime.Ticks,
                m => m.EndTime.Ticks,
                out firstIndex);

            while (++firstIndex < _notesMetadata.Length && _notesMetadata[firstIndex].EndTime <= currentTime)
            {
            }

            var notesToPlay = new List<NotePlaybackEventMetadata>();

            if (firstIndex < _notesMetadata.Length)
            {
                int lastIndex;
                MathUtilities.GetLastElementBelowThreshold(
                    _notesMetadata,
                    firstIndex,
                    _notesMetadata.Length - 1,
                    currentTime.Ticks,
                    m => m.StartTime.Ticks,
                    out lastIndex);

                for (var i = firstIndex; i <= lastIndex; i++)
                {
                    var metadata = _notesMetadata[i];
                    if (metadata.StartTime < currentTime && metadata.EndTime > currentTime)
                        notesToPlay.Add(metadata);
                }
            }

            return notesToPlay;
        }

        private void OnStarted()
        {
            Started?.Invoke(this, EventArgs.Empty);
        }

        private void OnStopped()
        {
            Stopped?.Invoke(this, EventArgs.Empty);
        }

        private void OnFinished()
        {
            Finished?.Invoke(this, EventArgs.Empty);
        }

        private void OnRepeatStarted()
        {
            RepeatStarted?.Invoke(this, EventArgs.Empty);
        }

        private void OnNotesPlaybackStarted(params Note[] notes)
        {
            if (notes.Any())
                NotesPlaybackStarted?.Invoke(this, new NotesEventArgs(notes));
        }

        private void OnNotesPlaybackFinished(params Note[] notes)
        {
            if (notes.Any())
                NotesPlaybackFinished?.Invoke(this, new NotesEventArgs(notes));
        }

        private void OnEventPlayed(MidiEvent midiEvent, object metadata)
        {
            EventPlayed?.Invoke(this, new MidiEventPlayedEventArgs(midiEvent, metadata));
        }

        private void OnDeviceErrorOccurred(Exception exception)
        {
            DeviceErrorOccurred?.Invoke(this, new ErrorOccurredEventArgs(exception));
        }

        private void OnClockTicked(object sender, EventArgs e)
        {
            do
            {
                var time = _clock.CurrentTime;
                if (time >= _playbackEndMetric)
                    break;

                var playbackEvent = GetCurrentPlaybackEvent();
                if (playbackEvent == null)
                    continue;

                if (playbackEvent.Time > time)
                    return;

                var midiEvent = playbackEvent.Event;
                if (midiEvent == null)
                    continue;

                if (!IsRunning)
                    return;

                Note note;
                if (TryPlayNoteEvent(playbackEvent, out note))
                {
                    if (note != null)
                    {
                        if (playbackEvent.Event is NoteOnEvent)
                            OnNotesPlaybackStarted(note);
                        else
                            OnNotesPlaybackFinished(note);
                    }

                    continue;
                }

                var eventCallback = EventCallback;
                if (eventCallback != null)
                    midiEvent = eventCallback(midiEvent.Clone(), playbackEvent.RawTime, time);

                if (midiEvent == null)
                    continue;

                PlayEvent(midiEvent, playbackEvent.Metadata.TimedEvent.Metadata);
            }
            while (MoveToNextPlaybackEvent());

            if (!Loop)
            {
                _clock.StopInternally();
                OnFinished();
                return;
            }

            _clock.StopShortly();
            _clock.ResetCurrentTime();
            ResetPlaybackEventsPosition();
            MoveToNextPlaybackEvent();

            MoveToStart();
            _clock.Start();
            OnRepeatStarted();
        }

        private void EnsureIsNotDisposed()
        {
            if (_disposed)
                throw new ObjectDisposedException("Playback is disposed.");
        }

        private void MoveToTime(ITimeSpan time, int firstIndex, int lastIndex)
        {
            ThrowIfArgument.IsNull(nameof(time), time);
            EnsureIsNotDisposed();

            if (TimeConverter.ConvertFrom(time, TempoMap) > _durationInTicks)
                time = (MetricTimeSpan)_duration;

            var isRunning = IsRunning;

            SetStartTime(time, firstIndex, lastIndex);

            if (isRunning)
            {
                SendTrackedData();
                StopStartNotes();
                _clock.Start();
            }
        }

        private void SetStartTime(ITimeSpan time, int firstIndex, int lastIndex)
        {
            var convertedTime = (TimeSpan)TimeConverter.ConvertTo<MetricTimeSpan>(time, TempoMap);
            _clock.SetCurrentTime(convertedTime);

            if (convertedTime.Ticks == 0)
            {
                _playbackEventsPosition = 0;
                return;
            }

            if (convertedTime > _duration)
            {
                _playbackEventsPosition = _playbackEvents.Length;
                return;
            }

            if (firstIndex >= lastIndex)
                return;

            MathUtilities.GetLastElementBelowThreshold(
                _playbackEvents,
                Math.Max(firstIndex, 0),
                Math.Min(lastIndex, _playbackEvents.Length - 1),
                convertedTime.Ticks,
                e => e.Time.Ticks,
                out _playbackEventsPosition);

            _playbackEventsPosition++;
        }

        private void PlayEvent(MidiEvent midiEvent, object metadata)
        {
            _playbackDataTracker.UpdateCurrentData(midiEvent, metadata);

            try
            {
                if (TryPlayEvent(midiEvent, metadata))
                    OnEventPlayed(midiEvent, metadata);
            }
            catch (Exception e)
            {
                OnDeviceErrorOccurred(e);
            }
        }

        private bool TryPlayNoteEvent(NotePlaybackEventMetadata noteMetadata, bool isNoteOnEvent, TimeSpan time, out Note note)
        {
            return TryPlayNoteEvent(noteMetadata, null, isNoteOnEvent, time, out note);
        }

        private bool TryPlayNoteEvent(PlaybackEvent playbackEvent, out Note note)
        {
            return TryPlayNoteEvent(playbackEvent.Metadata.Note, playbackEvent.Event, playbackEvent.Event is NoteOnEvent, playbackEvent.Time, out note);
        }

        private bool TryPlayNoteEvent(NotePlaybackEventMetadata noteMetadata, MidiEvent midiEvent, bool isNoteOnEvent, TimeSpan time, out Note note)
        {
            note = null;

            if (noteMetadata == null)
                return false;

            var notePlaybackData = noteMetadata.NotePlaybackData;

            var noteCallback = NoteCallback;
            if (noteCallback != null && midiEvent is NoteOnEvent)
            {
                notePlaybackData = noteCallback(noteMetadata.RawNotePlaybackData, noteMetadata.RawNote.Time, noteMetadata.RawNote.Length, time);
                noteMetadata.SetCustomNotePlaybackData(notePlaybackData);
            }

            note = noteMetadata.RawNote;

            if (noteMetadata.IsCustomNotePlaybackDataSet)
            {
                if (notePlaybackData == null || !notePlaybackData.PlayNote)
                    midiEvent = null;
                else
                {
                    note = noteMetadata.GetEffectiveNote();
                    midiEvent = midiEvent is NoteOnEvent
                        ? (MidiEvent)notePlaybackData.GetNoteOnEvent()
                        : notePlaybackData.GetNoteOffEvent();
                }
            }
            else if (midiEvent == null)
                midiEvent = isNoteOnEvent
                    ? (MidiEvent)notePlaybackData.GetNoteOnEvent()
                    : notePlaybackData.GetNoteOffEvent();

            if (midiEvent != null)
            {
                var timedObjectWithMetadata = isNoteOnEvent ? noteMetadata.RawNote.TimedNoteOnEvent : noteMetadata.RawNote.TimedNoteOffEvent;
                PlayEvent(midiEvent, (timedObjectWithMetadata as IMetadata)?.Metadata);

                if (midiEvent is NoteOnEvent)
                    _activeNotesMetadata.TryAdd(noteMetadata, 0);
                else
                {
                    byte value;
                    _activeNotesMetadata.TryRemove(noteMetadata, out value);
                }
            }
            else
                note = null;

            return true;
        }

        private PlaybackEvent[] GetPlaybackEvents(IEnumerable<ITimedObject> timedObjects, TempoMap tempoMap)
        {
            var playbackEvents = new List<PlaybackEvent>();

            foreach (var timedObject in timedObjects)
            {
                var customObjectProcessed = false;
                foreach (var e in GetTimedEvents(timedObject))
                {
                    playbackEvents.Add(GetPlaybackEvent(e, tempoMap));
                    customObjectProcessed = true;
                }

                if (customObjectProcessed)
                    continue;

                var chord = timedObject as Chord;
                if (chord != null)
                {
                    playbackEvents.AddRange(GetPlaybackEvents(chord, tempoMap));
                    continue;
                }

                var note = timedObject as Note;
                if (note != null)
                {
                    playbackEvents.AddRange(GetPlaybackEvents(note, tempoMap));
                    continue;
                }

                var timedEvent = timedObject as TimedEvent;
                if (timedEvent != null)
                {
                    playbackEvents.Add(GetPlaybackEvent(timedEvent, tempoMap));
                    continue;
                }

                var registeredParameter = timedObject as RegisteredParameter;
                if (registeredParameter != null)
                {
                    playbackEvents.AddRange(registeredParameter.GetTimedEvents().Select(e => GetPlaybackEvent(e, tempoMap)));
                    continue;
                }
            }

            return playbackEvents.OrderBy(e => e, new PlaybackEventsComparer()).ToArray();
        }

        private PlaybackEvent GetPlaybackEvent(TimedEvent timedEvent, TempoMap tempoMap)
        {
            var playbackEvent = CreatePlaybackEvent(timedEvent, tempoMap);
            playbackEvent.Metadata.TimedEvent = new TimedEventPlaybackEventMetadata((timedEvent as IMetadata)?.Metadata);
            InitializeTrackedData(playbackEvent);
            return playbackEvent;
        }

        private IEnumerable<PlaybackEvent> GetPlaybackEvents(Chord chord, TempoMap tempoMap)
        {
            foreach (var note in chord.Notes)
            {
                foreach (var playbackEvent in GetPlaybackEvents(note, tempoMap))
                {
                    yield return playbackEvent;
                }
            }
        }

        private IEnumerable<PlaybackEvent> GetPlaybackEvents(Note note, TempoMap tempoMap)
        {
            TimeSpan noteStartTime = note.TimeAs<MetricTimeSpan>(tempoMap);
            TimeSpan noteEndTime = TimeConverter.ConvertTo<MetricTimeSpan>(note.EndTime, tempoMap);
            var noteMetadata = new NotePlaybackEventMetadata(note, noteStartTime, noteEndTime);

            yield return GetPlaybackEventWithNoteMetadata(note.TimedNoteOnEvent, tempoMap, noteMetadata);
            yield return GetPlaybackEventWithNoteMetadata(note.TimedNoteOffEvent, tempoMap, noteMetadata);
        }

        private PlaybackEvent GetPlaybackEventWithNoteMetadata(TimedEvent timedEvent, TempoMap tempoMap, NotePlaybackEventMetadata noteMetadata)
        {
            var playbackEvent = CreatePlaybackEvent(timedEvent, tempoMap);
            playbackEvent.Metadata.Note = noteMetadata;
            playbackEvent.Metadata.TimedEvent = new TimedEventPlaybackEventMetadata((timedEvent as IMetadata)?.Metadata);
            InitializeTrackedData(playbackEvent);
            return playbackEvent;
        }

        private PlaybackEvent CreatePlaybackEvent(TimedEvent timedEvent, TempoMap tempoMap)
        {
            return new PlaybackEvent(timedEvent.Event, timedEvent.TimeAs<MetricTimeSpan>(tempoMap), timedEvent.Time);
        }

        private void InitializeTrackedData(PlaybackEvent playbackEvent)
        {
            _playbackDataTracker.InitializeData(
                playbackEvent.Event,
                playbackEvent.RawTime,
                playbackEvent.Metadata.TimedEvent.Metadata);

            var noteMetadata = playbackEvent.Metadata.Note;
            if (noteMetadata != null)
                _notesMetadataHashSet.Add(noteMetadata);
        }

        private void ResetPlaybackEventsPosition()
        {
            _playbackEventsPosition = -1;
        }

        private bool MoveToNextPlaybackEvent()
        {
            _playbackEventsPosition++;
            return IsPlaybackEventsPositionValid();
        }

        private PlaybackEvent GetCurrentPlaybackEvent()
        {
            return IsPlaybackEventsPositionValid()
                ? _playbackEvents[_playbackEventsPosition]
                : null;
        }

        private bool IsPlaybackEventsPositionValid()
        {
            return _playbackEventsPosition >= 0 && _playbackEventsPosition < _playbackEvents.Length;
        }

        #endregion

        #region IClockDrivenObject

        /// <summary>
        /// Ticks internal clock.
        /// </summary>
        /// <exception cref="ObjectDisposedException">The current <see cref="Playback"/> is disposed.</exception>
        public void TickClock()
        {
            EnsureIsNotDisposed();

            _clock?.Tick();
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

                _clock.Ticked -= OnClockTicked;
                _clock.Dispose();
            }

            _disposed = true;
        }

        #endregion
    }
}
