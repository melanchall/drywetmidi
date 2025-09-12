using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;

namespace Melanchall.DryWetMidi.Multimedia
{
    internal sealed class PlaybackActionsTracer
    {
        #region Nested classes

        private sealed class Action
        {
            public Action(
                TimeSpan timestamp,
                TimeSpan playbackTime,
                string description)
            {
                Timestamp = timestamp;
                PlaybackTime = playbackTime;
                Description = description;
            }

            public TimeSpan Timestamp { get; }

            public TimeSpan PlaybackTime { get; }

            public string Description { get; }
        }

        #endregion

        #region Fields

        private readonly List<Action> _actions = new List<Action>();
        private readonly object _lockObject = new object();

        private Stopwatch _stopwatch = new Stopwatch();
        private int _tickCounter = -1;

        #endregion

        #region Methods

        public void SetStopwatch(Stopwatch stopwatch)
        {
            _stopwatch = stopwatch;
        }

        public void TraceTick(TimeSpan playbackTime, string action)
        {
            var counter = Interlocked.Increment(ref _tickCounter);
            if (counter % 10 == 0)
                TraceAction(playbackTime, $"{action}: {counter}");
        }

        public void TraceAction(TimeSpan playbackTime, string action)
        {
            lock (_lockObject)
            {
                _actions.Add(new Action(_stopwatch.Elapsed, playbackTime, action));
            }
        }

        public ICollection<string> GetTraces() => _actions
            .Select(a => $"{a.Timestamp} -> {a.PlaybackTime}: {a.Description}")
            .ToArray();

        #endregion
    }
}
