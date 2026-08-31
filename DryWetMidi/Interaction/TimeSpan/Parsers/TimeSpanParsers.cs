namespace Melanchall.DryWetMidi.Interaction
{
    internal static class TimeSpanParsers
    {
        public static readonly BarBeatFractionTimeSpanParser BarBeatFractionTimeSpanParser = new();

        public static readonly BarBeatTicksTimeSpanParser BarBeatTicksTimeSpanParser = new();

        public static readonly MetricTimeSpanParser MetricTimeSpanParser = new();

        public static readonly MusicalTimeSpanParser MusicalTimeSpanParser = new();

        public static readonly MidiTimeSpanParser MidiTimeSpanParser = new();
    }
}
