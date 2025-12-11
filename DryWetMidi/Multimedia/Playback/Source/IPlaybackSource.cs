using System;

namespace Melanchall.DryWetMidi.Multimedia
{
    internal interface IPlaybackSource
    {
        void AddPlaybackEvent(PlaybackEvent playbackEvent);

        void CompleteSource();

        bool IsEmpty();

        void InvalidatePosition();

        void ResetPosition(TimeSpan playbackStart, ref bool beforeStart);

        void IncrementPosition(ref bool beforeStart);

        void MoveToFirstPosition();

        void MoveToLastPositionBelowThreshold(TimeSpan threshold, ref bool beforeStart);

        bool IsPositionValid();

        PlaybackEvent GetCurrentPlaybackEvent();

        PlaybackEvent GetLastPlaybackEvent();

        SnapPoint GetNextSnapPoint(TimeSpan fromTime, Func<PlaybackEvent, SnapPoint> getSnapPoint);

        SnapPoint GetPreviousSnapPoint(TimeSpan fromTime, Func<PlaybackEvent, SnapPoint> getSnapPoint);

        void ScalePlaybackEventsTimes(
            TimeSpan tempoChangeTime,
            TimeSpan? nextTempoTime,
            double scaleFactor,
            TimeSpan shift);
    }
}
