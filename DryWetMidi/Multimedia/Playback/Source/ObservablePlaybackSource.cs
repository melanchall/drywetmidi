using Melanchall.DryWetMidi.Common;
using System;
using System.Linq;

using PlaybackEventsCollection = Melanchall.DryWetMidi.Common.RedBlackTree<System.TimeSpan, Melanchall.DryWetMidi.Multimedia.PlaybackEvent>;
using PlaybackEventsPosition = Melanchall.DryWetMidi.Common.RedBlackTreeCoordinate<System.TimeSpan, Melanchall.DryWetMidi.Multimedia.PlaybackEvent>;

namespace Melanchall.DryWetMidi.Multimedia
{
    internal sealed class ObservablePlaybackSource : IPlaybackSource
    {
        #region Fields

        private readonly PlaybackEventsCollection _playbackEvents = new PlaybackEventsCollection();
        private PlaybackEventsPosition _playbackEventsPosition;

        #endregion

        #region Properties

        public PlaybackEventsCollection PlaybackEvents => _playbackEvents;

        public PlaybackEventsPosition PlaybackEventsPosition
        {
            get { return _playbackEventsPosition; }
            set { _playbackEventsPosition = value; }
        }

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
        }

        public void CompleteSource()
        {
        }

        public bool IsEmpty()
        {
            return !_playbackEvents.Any();
        }

        public void InvalidatePosition()
        {
            _playbackEventsPosition = null;
        }

        public void ResetPosition(TimeSpan playbackStart, ref bool beforeStart)
        {
            _playbackEventsPosition = _playbackEvents.GetLastCoordinateBelowThreshold(playbackStart);
            if (_playbackEventsPosition == null)
                beforeStart = true;
        }

        public void IncrementPosition(ref bool beforeStart)
        {
            _playbackEventsPosition = _playbackEventsPosition != null
                ? _playbackEvents.GetNextCoordinate(_playbackEventsPosition)
                : (beforeStart ? _playbackEvents.GetMinimumCoordinate() : null);

            beforeStart = _playbackEventsPosition != null;
        }

        public void MoveToFirstPosition()
        {
            _playbackEventsPosition = _playbackEvents.GetMinimumCoordinate();
        }

        public void MoveToLastPositionBelowThreshold(TimeSpan threshold, ref bool beforeStart)
        {
            _playbackEventsPosition = _playbackEvents.GetLastCoordinateBelowThreshold(threshold);
            if (_playbackEventsPosition == null)
                beforeStart = true;
        }

        public bool IsPositionValid()
        {
            return _playbackEventsPosition != null;
        }

        public PlaybackEvent GetCurrentPlaybackEvent()
        {
            return IsPositionValid()
                ? _playbackEventsPosition.Value
                : null;
        }

        public PlaybackEvent GetLastPlaybackEvent()
        {
            return _playbackEvents.GetMaximumCoordinate()?.Value;
        }

        public SnapPoint GetNextSnapPoint(TimeSpan fromTime, Func<PlaybackEvent, SnapPoint> getSnapPoint)
        {
            var node = _playbackEvents.GetFirstCoordinateAboveThreshold(fromTime);

            while (node != null)
            {
                var snapPoint = getSnapPoint(node.Value);
                if (snapPoint != null && snapPoint.IsEnabled)
                    return snapPoint;

                node = _playbackEvents.GetNextCoordinate(node);
            }

            return null;
        }

        public SnapPoint GetPreviousSnapPoint(TimeSpan fromTime, Func<PlaybackEvent, SnapPoint> getSnapPoint)
        {
            var node = _playbackEvents.GetLastCoordinateBelowThreshold(fromTime);

            while (node != null)
            {
                var snapPoint = getSnapPoint(node.Value);
                if (snapPoint != null && snapPoint.IsEnabled)
                    return snapPoint;

                node = _playbackEvents.GetPreviousCoordinate(node);
            }

            return null;
        }

        public void ScalePlaybackEventsTimes(
            TimeSpan tempoChangeTime,
            TimeSpan? nextTempoTime,
            double scaleFactor,
            TimeSpan shift)
        {
            var firstNodeAfterTempoChange = _playbackEvents.GetFirstCoordinateAboveThreshold(tempoChangeTime);
            if (firstNodeAfterTempoChange == null)
                return;

            var node = firstNodeAfterTempoChange;

            do
            {
                if (nextTempoTime != null && node.Key > nextTempoTime)
                    node.Key -= shift;
                else
                    node.Key = tempoChangeTime + ScaleTimeSpan(node.Key - tempoChangeTime, scaleFactor);

                node.Value.Time.Time = node.Key;
            }
            while ((node = _playbackEvents.GetNextCoordinate(node)) != null);
        }

        #endregion
    }
}
