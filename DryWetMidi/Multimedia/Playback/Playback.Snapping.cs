using Melanchall.DryWetMidi.Common;
using System.Collections.Generic;
using System;
using Melanchall.DryWetMidi.Interaction;
using Melanchall.DryWetMidi.Core;
using System.Linq;

namespace Melanchall.DryWetMidi.Multimedia
{
    public partial class Playback
    {
        #region Fields

        private readonly RedBlackTree<TimeSpan, SnapPoint> _snapPoints = new RedBlackTree<TimeSpan, SnapPoint>();
        private readonly List<SnapPointsGroup> _snapPointsGroups = new List<SnapPointsGroup>();

        #endregion

        #region Properties

        /// <summary>
        /// Gets or sets a value indicating whether playback snapping is enabled or not. The property
        /// lets turn on or off all snap points at once.
        /// </summary>
        public bool IsSnappingEnabled { get; set; } = true;

        internal IEnumerable<SnapPoint> SnapPoints => _snapPoints;

        #endregion

        #region Methods

        /// <summary>
        /// Adds a snap point with the specified data at given time.
        /// </summary>
        /// <typeparam name="TData">Type of data that will be attached to a snap point.</typeparam>
        /// <param name="time">Time to add snap point at.</param>
        /// <param name="data">Data to attach to snap point.</param>
        /// <returns>An instance of the <see cref="SnapPoint{TData}"/> representing a snap point
        /// with <paramref name="data"/> at <paramref name="time"/>.</returns>
        /// <exception cref="ArgumentNullException"><paramref name="time"/> is <c>null</c>.</exception>
        public SnapPoint<TData> AddSnapPoint<TData>(ITimeSpan time, TData data)
        {
            ThrowIfArgument.IsNull(nameof(time), time);

            return (SnapPoint<TData>)AddSnapPoint(time, metricTime => new SnapPoint<TData>(metricTime, data));
        }

        /// <summary>
        /// Adds a snap point at the specified time.
        /// </summary>
        /// <param name="time">Time to add snap point at.</param>
        /// <returns>An instance of the <see cref="SnapPoint"/> representing a snap point
        /// at <paramref name="time"/>.</returns>
        /// <exception cref="ArgumentNullException"><paramref name="time"/> is <c>null</c>.</exception>
        public SnapPoint AddSnapPoint(ITimeSpan time)
        {
            ThrowIfArgument.IsNull(nameof(time), time);

            return AddSnapPoint(time, metricTime => new SnapPoint(metricTime));
        }

        /// <summary>
        /// Removes a snap point.
        /// </summary>
        /// <param name="snapPoint">Snap point to remove.</param>
        /// <exception cref="ArgumentNullException"><paramref name="snapPoint"/> is <c>null</c>.</exception>
        public void RemoveSnapPoint(SnapPoint snapPoint)
        {
            ThrowIfArgument.IsNull(nameof(snapPoint), snapPoint);

            var node = _snapPoints.GetCoordinate(snapPoint.Time, snapPoint);
            _snapPoints.Remove(node);
        }

        /// <summary>
        /// Removes the specified <see cref="SnapPointsGroup"/> from the list of snap points groups
        /// of the current <see cref="Playback"/>.
        /// </summary>
        /// <param name="snapPointsGroup"><see cref="SnapPointsGroup"/> to remove.</param>
        /// <exception cref="ArgumentNullException"><paramref name="snapPointsGroup"/> is <c>null</c>.</exception>
        public void RemoveSnapPointsGroup(SnapPointsGroup snapPointsGroup)
        {
            ThrowIfArgument.IsNull(nameof(snapPointsGroup), snapPointsGroup);

            _snapPointsGroups.Remove(snapPointsGroup);
        }

        /// <summary>
        /// Removes all snap points that match the conditions defined by the specified predicate.
        /// </summary>
        /// <typeparam name="TData">Type of data attached to snap points to remove.</typeparam>
        /// <param name="predicate">The <see cref="Predicate{TData}"/> delegate that defines the conditions
        /// of snap points to remove.</param>
        /// <exception cref="ArgumentNullException"><paramref name="predicate"/> is <c>null</c>.</exception>
        public void RemoveSnapPointsByData<TData>(Predicate<TData> predicate)
        {
            ThrowIfArgument.IsNull(nameof(predicate), predicate);

            var node = _snapPoints.GetMinimumCoordinate();

            while (node != null)
            {
                var nextNode = _snapPoints.GetNextCoordinate(node);

                var snapPoint = node.Value as SnapPoint<TData>;
                if (snapPoint != null && predicate(snapPoint.Data))
                    _snapPoints.Remove(node);

                node = nextNode;
            }
        }

        /// <summary>
        /// Removes all snap points.
        /// </summary>
        public void RemoveAllSnapPoints()
        {
            _snapPoints.Clear();
            _snapPointsGroups.Clear();
        }

        /// <summary>
        /// Creates a new <see cref="SnapPointsGroup"/> that holds the specified predicate to select MIDI events
        /// which times will be used as snap points. For example, to get snap points at the markers points, you
        /// can use this predicate: <c>e => e.EventType == MidiEventType.Marker</c>.
        /// </summary>
        /// <remarks>
        /// After the group is created, it will be enabled by default. You can disable it using the
        /// <see cref="SnapPointsGroup.IsEnabled"/> property setting it to <c>false</c>.
        /// </remarks>
        /// <param name="predicate">Predicate to select MIDI events.</param>
        /// <returns><see cref="SnapPointsGroup"/> holding information on how to select events.</returns>
        /// <exception cref="ArgumentNullException"><paramref name="predicate"/> is <c>null</c>.</exception>
        public SnapPointsGroup SnapToEvents(Predicate<MidiEvent> predicate)
        {
            ThrowIfArgument.IsNull(nameof(predicate), predicate);

            var snapPointsGroup = new SnapPointsGroup(predicate);
            _snapPointsGroups.Add(snapPointsGroup);
            return snapPointsGroup;
        }

        private SnapPoint AddSnapPoint(ITimeSpan time, Func<TimeSpan, SnapPoint> createSnapPoint)
        {
            TimeSpan metricTime = TimeConverter.ConvertTo<MetricTimeSpan>(time, TempoMap);
            if (metricTime == TimeSpan.Zero)
                metricTime = new TimeSpan(1);

            var snapPoint = createSnapPoint(metricTime);

            _snapPoints.Add(snapPoint.Time, snapPoint);
            return snapPoint;
        }

        private SnapPoint GetNextSnapPoint(TimeSpan time, SnapPointsGroup snapPointsGroup)
        {
            if (!snapPointsGroup.IsEnabled)
                return null;

            var node = _playbackEvents.GetFirstCoordinateAboveThreshold(time);

            while (node != null)
            {
                if (snapPointsGroup.Predicate(node.Value.Event))
                    return new SnapPoint(node.Key);

                node = _playbackEvents.GetNextCoordinate(node);
            }

            return null;
        }

        private SnapPoint GetNextSnapPoint(TimeSpan time)
        {
            return GetNextSnapPoint(time, point => true);
        }

        private SnapPoint<TData> GetNextSnapPoint<TData>(TimeSpan time, TData data)
        {
            return (SnapPoint<TData>)GetNextSnapPoint(time, p => IsSnapPointWithData(p, data));
        }

        private SnapPoint GetNextSnapPoint(TimeSpan time, Predicate<SnapPoint> predicate)
        {
            var snapPointNode = _snapPoints.GetFirstCoordinateAboveThreshold(time);

            while (snapPointNode != null)
            {
                if (snapPointNode.Value.IsEnabled && predicate(snapPointNode.Value))
                    break;

                snapPointNode = _snapPoints.GetNextCoordinate(snapPointNode);
            }

            if (_snapPointsGroups.Any())
            {
                var node = _playbackEvents.GetFirstCoordinateAboveThreshold(time);

                while (node != null)
                {
                    foreach (var group in _snapPointsGroups)
                    {
                        if (group.IsEnabled && group.Predicate(node.Value.Event) && (snapPointNode == null || node.Key < snapPointNode.Key))
                            return new SnapPoint(node.Key);
                    }

                    node = _playbackEvents.GetNextCoordinate(node);
                }
            }

            return snapPointNode?.Value;
        }

        private SnapPoint GetPreviousSnapPoint(TimeSpan time, SnapPointsGroup snapPointsGroup)
        {
            if (!snapPointsGroup.IsEnabled)
                return null;

            var node = _playbackEvents.GetLastCoordinateBelowThreshold(time);

            while (node != null)
            {
                if (snapPointsGroup.Predicate(node.Value.Event))
                    return new SnapPoint(node.Key);

                node = _playbackEvents.GetPreviousCoordinate(node);
            }

            return null;
        }

        private SnapPoint GetPreviousSnapPoint(TimeSpan time)
        {
            return GetPreviousSnapPoint(time, _ => true);
        }

        private SnapPoint<TData> GetPreviousSnapPoint<TData>(TimeSpan time, TData data)
        {
            return (SnapPoint<TData>)GetPreviousSnapPoint(time, p => IsSnapPointWithData(p, data));
        }

        private SnapPoint GetPreviousSnapPoint(TimeSpan time, Predicate<SnapPoint> predicate)
        {
            var snapPointNode = _snapPoints.GetLastCoordinateBelowThreshold(time);

            while (snapPointNode != null)
            {
                if (snapPointNode.Value.IsEnabled && predicate(snapPointNode.Value))
                    break;

                snapPointNode = _snapPoints.GetPreviousCoordinate(snapPointNode);
            }

            if (_snapPointsGroups.Any())
            {
                var node = _playbackEvents.GetLastCoordinateBelowThreshold(time);

                while (node != null)
                {
                    foreach (var group in _snapPointsGroups)
                    {
                        if (group.IsEnabled && group.Predicate(node.Value.Event) && (snapPointNode == null || node.Key > snapPointNode.Key))
                            return new SnapPoint(node.Key);
                    }

                    node = _playbackEvents.GetPreviousCoordinate(node);
                }
            }

            return snapPointNode?.Value;
        }

        private bool IsSnapPointWithData<TData>(SnapPoint snapPoint, TData data)
        {
            var snapPointWithData = snapPoint as SnapPoint<TData>;
            return snapPointWithData != null && snapPointWithData.Data.Equals(data);
        }

        #endregion
    }
}
