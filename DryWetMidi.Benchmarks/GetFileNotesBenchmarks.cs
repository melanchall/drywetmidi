using BenchmarkDotNet.Attributes;
using Melanchall.DryWetMidi.Core;
using Melanchall.DryWetMidi.Interaction;

namespace Melanchall.DryWetMidi.Benchmarks
{
    [MemoryDiagnoser]
    public class GetFileNotesBenchmarks
    {
        private MidiFile _midiFile = null!;

        [ParamsSource(nameof(FilesPaths))]
        public string FilePath { get; set; }

        [GlobalSetup]
        public void Setup()
        {
            _midiFile = MidiFile.Read(FilePath);
        }

        [Benchmark]
        public int GetNotes()
        {
            var notes = _midiFile.GetNotes();
            return notes.Count;
        }

        public string[] FilesPaths => Directory
            .GetFiles(@"D:\Dropbox\Melanchall\DryWetMIDI\drywetmidi\Resources\MIDI files\Valid", "*.*", SearchOption.AllDirectories)
            .ToArray();
    }
}
