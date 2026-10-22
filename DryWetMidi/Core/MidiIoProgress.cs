using Melanchall.DryWetMidi.Common;
using System;
using System.Diagnostics;

namespace Melanchall.DryWetMidi.Core
{
    internal sealed class MidiIoProgress
    {
        private readonly IProgress<AsyncOperationProgress> _progress;
        private readonly string _phase;
        private readonly Stopwatch _stopwatch = Stopwatch.StartNew();
        private long _processed;
        private long _lastReport;

        internal MidiIoProgress(IProgress<AsyncOperationProgress> progress, string phase)
        {
            _progress = progress;
            _phase = phase;
        }

        internal void Add(long bytes)
        {
            _processed += bytes;
            var elapsed = _stopwatch.ElapsedMilliseconds;
            if (elapsed - _lastReport < 100)
                return;
            _lastReport = elapsed;
            _progress.Report(new AsyncOperationProgress(_processed, phase: _phase));
        }

        internal void Complete() =>
            _progress.Report(new AsyncOperationProgress(_processed, true, _phase));
    }
}
