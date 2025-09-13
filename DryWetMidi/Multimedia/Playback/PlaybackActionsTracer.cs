using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

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

        private readonly Stopwatch _stopwatch = new Stopwatch();
        private readonly List<Action> _actions = new List<Action>();

        #endregion

        #region Methods

        public void TraceAction(TimeSpan playbackTime, string action)
        {
            if (!_stopwatch.IsRunning)
                _stopwatch.Start();

            _actions.Add(new Action(_stopwatch.Elapsed, playbackTime, action));
        }

        public ICollection<string> GetTraces() => _actions
            .Select(a => $"{a.Timestamp} -> {a.PlaybackTime}: {a.Description}")
            .ToArray();

        #endregion
    }
}
