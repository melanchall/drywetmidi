using System;
using System.Collections.Generic;
using System.Linq;
using Melanchall.DryWetMidi.Common;
using Melanchall.DryWetMidi.Smf.Interaction;
using Melanchall.DryWetMidi.Tools;

namespace Melanchall.DryWetMidi.Devices
{
    public sealed class PlaybackSnapping
    {
        #region Fields

        private readonly List<SnapPoint> _snapPoints = new List<SnapPoint>();

        private readonly IEnumerable<PlaybackEvent> _playbackEvents;
        private readonly TempoMap _tempoMap;
        private readonly TimeSpan _maxTime;

        #endregion

        #region Constructor

        internal PlaybackSnapping(IEnumerable<PlaybackEvent> playbackEvents, TempoMap tempoMap)
        {
            _playbackEvents = playbackEvents;
            _tempoMap = tempoMap;
            _maxTime = _playbackEvents.LastOrDefault()?.Time ?? TimeSpan.Zero;
        }

        #endregion

        #region Properties

        public IEnumerable<SnapPoint> SnapPoints => _snapPoints.AsReadOnly();

        #endregion

        #region Methods

        public SnapPoint<TData> AddSnapPoint<TData>(ITimeSpan time, TData data)
        {
            ThrowIfArgument.IsNull(nameof(time), time);

            var metricTime = TimeConverter.ConvertTo<MetricTimeSpan>(time, _tempoMap);
            var snapPoint = new SnapPoint<TData>(metricTime, data);

            _snapPoints.Add(snapPoint);
            return snapPoint;
        }

        public void RemoveSnapPoint(SnapPoint snapPoint)
        {
            ThrowIfArgument.IsNull(nameof(snapPoint), snapPoint);

            _snapPoints.Remove(snapPoint);
        }

        public SnapPointsGroup SnapToGrid(IGrid grid)
        {
            ThrowIfArgument.IsNull(nameof(grid), grid);

            var snapPointsGroup = new SnapPointsGroup();

            foreach (var time in grid.GetTimes(_tempoMap))
            {
                TimeSpan metricTime = TimeConverter.ConvertTo<MetricTimeSpan>(time, _tempoMap);
                if (metricTime > _maxTime)
                    break;

                _snapPoints.Add(new SnapPoint(metricTime) { SnapPointsGroup = snapPointsGroup });
            }

            return snapPointsGroup;
        }

        public void EnableSnapPointsGroup(SnapPointsGroup snapPointsGroup)
        {
            ThrowIfArgument.IsNull(nameof(snapPointsGroup), snapPointsGroup);

            snapPointsGroup.IsEnabled = true;
        }

        public void DisableSnapPointsGroup(SnapPointsGroup snapPointsGroup)
        {
            ThrowIfArgument.IsNull(nameof(snapPointsGroup), snapPointsGroup);

            snapPointsGroup.IsEnabled = false;
        }

        internal SnapPoint GetNextSnapPoint(TimeSpan time, SnapPointsGroup snapPointsGroup)
        {
            return GetActiveSnapPoints(snapPointsGroup).SkipWhile(p => p.Time <= time).FirstOrDefault();
        }

        internal SnapPoint GetNextSnapPoint(TimeSpan time)
        {
            return GetActiveSnapPoints().SkipWhile(p => p.Time <= time).FirstOrDefault();
        }

        internal SnapPoint GetPreviousSnapPoint(TimeSpan time, SnapPointsGroup snapPointsGroup)
        {
            return GetActiveSnapPoints(snapPointsGroup).TakeWhile(p => p.Time < time).LastOrDefault();
        }

        internal SnapPoint GetPreviousSnapPoint(TimeSpan time)
        {
            return GetActiveSnapPoints().TakeWhile(p => p.Time < time).LastOrDefault();
        }

        private IEnumerable<SnapPoint> GetActiveSnapPoints()
        {
            return _snapPoints.Where(p => p.IsEnabled && p.SnapPointsGroup?.IsEnabled != false).OrderBy(p => p.Time);
        }

        private IEnumerable<SnapPoint> GetActiveSnapPoints(SnapPointsGroup snapPointsGroup)
        {
            return GetActiveSnapPoints().Where(p => p.SnapPointsGroup == snapPointsGroup);
        }

        #endregion
    }
}
