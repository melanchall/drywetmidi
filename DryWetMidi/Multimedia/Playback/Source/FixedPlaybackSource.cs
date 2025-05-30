using Melanchall.DryWetMidi.Common;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Melanchall.DryWetMidi.Multimedia
{
    internal sealed class FixedPlaybackSource : IPlaybackSource
    {
        #region Fields

        private IList<PlaybackEvent> _playbackEvents = new List<PlaybackEvent>();
        private int _playbackEventsPosition = -1;
        private bool _isCompleted;

        #endregion

        #region Methods

        private TimeSpan ScaleTimeSpan(TimeSpan timeSpan, double scaleFactor)
        {
            return timeSpan.DivideBy(scaleFactor);
        }

        #endregion

        #region IPlaybackSource

        public void AddPlaybackEvent(PlaybackEvent playbackEvent)
        {
            _playbackEvents.Add(playbackEvent);
        }

        public void CompleteSource()
        {
            ((List<PlaybackEvent>)_playbackEvents).Sort((x, y) => Math.Sign(x.RawTime - y.RawTime));
            _isCompleted = true;
        }

        public PlaybackEvent GetCurrentPlaybackEvent()
        {
            return IsPositionValid()
                ? _playbackEvents[_playbackEventsPosition]
                : null;
        }

        public PlaybackEvent GetLastPlaybackEvent()
        {
            return IsEmpty() ? null : _playbackEvents.Last();
        }

        public SnapPoint GetNextSnapPoint(TimeSpan fromTime, Func<PlaybackEvent, SnapPoint> getSnapPoint)
        {
            int i;
            MathUtilities.GetFirstElementAboveThreshold(
                _playbackEvents,
                fromTime.Ticks,
                e => e.Time.Time.Ticks,
                out i);

            for (; i < _playbackEvents.Count; i++)
            {
                var snapPoint = getSnapPoint(_playbackEvents[i]);
                if (snapPoint != null && snapPoint.IsEnabled)
                    return snapPoint;
            }

            return null;
        }

        public SnapPoint GetPreviousSnapPoint(TimeSpan fromTime, Func<PlaybackEvent, SnapPoint> getSnapPoint)
        {
            int i;
            MathUtilities.GetLastElementBelowThreshold(
                _playbackEvents,
                fromTime.Ticks,
                e => e.Time.Time.Ticks,
                out i);

            for (; i >= 0; i--)
            {
                var snapPoint = getSnapPoint(_playbackEvents[i]);
                if (snapPoint != null && snapPoint.IsEnabled)
                    return snapPoint;
            }

            return null;
        }

        public void IncrementPosition(ref bool beforeStart)
        {
            _playbackEventsPosition = _playbackEventsPosition >= 0
                ? _playbackEventsPosition + 1
                : (beforeStart ? (IsEmpty() ? -1 : 0) : -1);

            beforeStart = _playbackEventsPosition >= 0;
        }

        public void InvalidatePosition()
        {
            _playbackEventsPosition = -1;
        }

        public bool IsEmpty()
        {
            return !_playbackEvents.Any();
        }

        public bool IsPositionValid()
        {
            return _playbackEventsPosition >= 0 && _playbackEventsPosition < _playbackEvents.Count;
        }

        public void MoveToFirstPosition()
        {
            _playbackEventsPosition = IsEmpty() ? -1 : 0;
        }

        public void MoveToLastPositionBelowThreshold(TimeSpan threshold, ref bool beforeStart)
        {
            MathUtilities.GetLastElementBelowThreshold(
                _playbackEvents,
                threshold,
                e => e.Time.Time,
                out _playbackEventsPosition);

            if (_playbackEventsPosition == -1)
                beforeStart = true;
        }

        public void ResetPosition(TimeSpan playbackStart, ref bool beforeStart)
        {
            MathUtilities.GetLastElementBelowThreshold(
                _playbackEvents,
                playbackStart,
                e => e.Time.Time,
                out _playbackEventsPosition);

            if (_playbackEventsPosition == -1)
                beforeStart = true;
        }

        public void ScalePlaybackEventsTimes(
            TimeSpan tempoChangeTime,
            TimeSpan? nextTempoTime,
            double scaleFactor,
            TimeSpan shift)
        {
            if (_isCompleted)
                return;

            int firstNodeAfterTempoChange;
            MathUtilities.GetFirstElementAboveThreshold(
                _playbackEvents,
                tempoChangeTime,
                e => e.Time.Time,
                out firstNodeAfterTempoChange);

            if (firstNodeAfterTempoChange < 0)
                return;

            var node = firstNodeAfterTempoChange;

            do
            {
                var key = _playbackEvents[node].Time.Time;
                if (nextTempoTime != null && key > nextTempoTime)
                    key -= shift;
                else
                    key = tempoChangeTime + ScaleTimeSpan(key - tempoChangeTime, scaleFactor);

                _playbackEvents[node].Time.Time = key;
            }
            while (++node < _playbackEvents.Count);
        }

        #endregion
    }
}
