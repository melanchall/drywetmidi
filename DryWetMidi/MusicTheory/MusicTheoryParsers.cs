namespace Melanchall.DryWetMidi.MusicTheory
{
    internal static class MusicTheoryParsers
    {
        public static readonly NoteNameParser NoteNameParser = new();

        public static readonly NoteParser NoteParser = new();

        public static readonly ChordParser ChordParser = new();

        public static readonly IntervalParser IntervalParser = new();

        public static readonly OctaveParser OctaveParser = new();

        public static readonly ScaleParser ScaleParser = new();

        public static readonly ChordProgressionParser ChordProgressionParser = new();
    }
}
