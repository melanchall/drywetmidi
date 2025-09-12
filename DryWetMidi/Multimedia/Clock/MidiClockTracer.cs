using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

namespace Melanchall.DryWetMidi.Multimedia
{
    internal sealed class MidiClockTracer
    {
        #region Fields

        private readonly Stopwatch _stopwatch = new Stopwatch();
        private readonly object _lockObject = new object();

        private readonly List<List<long>> _tickTimes = Enumerable
            .Range(0, 500)
            .Select(_ => new List<long>(10000))
            .ToList();

        private readonly List<long> _startTimes = new List<long>(500);
        private readonly List<long> _stopTimes = new List<long>(500);

        private int _currentTickTimesListIndex = 0;
        private bool _isActive;

        #endregion

        #region Methods

        public void Start()
        {
            if (_isActive)
                return;

            lock (_lockObject)
            {
                if (_isActive)
                    return;

                _stopwatch.Start();
                _isActive = true;

                _startTimes.Add(_stopwatch.ElapsedMilliseconds);
            }
        }

        public void Stop()
        {
            if (!_isActive)
                return;

            lock (_lockObject)
            {
                if (!_isActive)
                    return;

                _isActive = false;
                _currentTickTimesListIndex++;

                _stopTimes.Add(_stopwatch.ElapsedMilliseconds);
            }
        }

        public void TraceTick()
        {
            if (!_isActive)
                return;

            lock (_lockObject)
            {
                if (!_isActive)
                    return;

                _tickTimes[_currentTickTimesListIndex].Add(_stopwatch.ElapsedMilliseconds);
            }
        }

        public long[][] GetTickTimes()
        {
            lock (_lockObject)
            {
                return _tickTimes.Where(t => t.Any()).Select(t => t.ToArray()).ToArray();
            }
        }

        public long[] GetStartTimes()
        {
            lock (_lockObject)
            {
                return _startTimes.ToArray();
            }
        }

        public long[] GetStopTimes()
        {
            lock (_lockObject)
            {
                return _stopTimes.ToArray();
            }
        }

        #endregion
    }
}
