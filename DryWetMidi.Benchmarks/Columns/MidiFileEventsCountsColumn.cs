using System.Linq;
using BenchmarkDotNet.Columns;
using BenchmarkDotNet.Reports;
using BenchmarkDotNet.Running;

namespace Melanchall.DryWetMidi.Benchmarks
{
    public sealed class MidiFileEventsCountsColumn : IColumn
    {
        private readonly int[] _eventsCounts;

        public MidiFileEventsCountsColumn(int[] eventsCounts)
        {
            _eventsCounts = eventsCounts;
        }

        public string Id => "EventsCounts";

        public string ColumnName => "Events counts";

        public string Legend => "Count of each MIDI file events collection";

        public bool AlwaysShow => true;

        public ColumnCategory Category => ColumnCategory.Custom;

        public int PriorityInCategory => 0;

        public bool IsNumeric => false;

        public UnitType UnitType => UnitType.Dimensionless;

        public string GetValue(Summary summary, BenchmarkCase benchmark)
        {
            return string.Join(", ", _eventsCounts.OrderBy(c => c));
        }

        public string GetValue(Summary summary, BenchmarkCase benchmark, SummaryStyle style)
        {
            return GetValue(summary, benchmark);
        }

        public bool IsAvailable(Summary summary) => true;

        public bool IsDefault(Summary summary, BenchmarkCase benchmark) => false;
    }
}
