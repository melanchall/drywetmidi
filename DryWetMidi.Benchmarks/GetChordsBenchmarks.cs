using BenchmarkDotNet.Attributes;
using Melanchall.DryWetMidi.Common;
using Melanchall.DryWetMidi.Core;
using Melanchall.DryWetMidi.Interaction;

namespace Melanchall.DryWetMidi.Benchmarks;

[MemoryDiagnoser]
public class GetChordsBenchmarks
{
    private MidiEvent[] _noOverlapEvents = null!;
    private MidiEvent[] _withOverlapEvents = null!;
    private MidiEvent[] _mixedEvents = null!;

    [Params(1000, 10000, 100000)]
    public int EventsCount { get; set; }

    [GlobalSetup]
    public void Setup()
    {
        _noOverlapEvents = BuildNoOverlapEvents(EventsCount);
        _withOverlapEvents = BuildOverlapEvents(EventsCount);
        _mixedEvents = BuildMixedEvents(EventsCount);
    }

    [Benchmark(Description = "GetChords – no overlaps")]
    public int GetChords_NoOverlap()
    {
        var chords = _noOverlapEvents.GetChords();
        return chords.Count;
    }

    [Benchmark(Description = "GetChords – with overlaps")]
    public int GetChords_WithOverlap()
    {
        var chords = _withOverlapEvents.GetChords();
        return chords.Count;
    }

    [Benchmark(Description = "GetChords – mixed")]
    public int GetChords_Mixed()
    {
        var chords = _mixedEvents.GetChords();
        return chords.Count;
    }

    private static MidiEvent[] BuildNoOverlapEvents(int pairCount)
    {
        var events = new MidiEvent[pairCount * 2];
        for (int i = 0; i < pairCount; i++)
        {
            var noteNumber = (SevenBitNumber)(i % 128);
            events[i * 2] = new NoteOnEvent(noteNumber, (SevenBitNumber)64);
            events[i * 2 + 1] = new NoteOffEvent(noteNumber, (SevenBitNumber)0);
        }
        return events;
    }

    private static MidiEvent[] BuildOverlapEvents(int pairCount)
    {
        var events = new MidiEvent[pairCount * 2];
        for (int i = 0; i < pairCount; i++)
        {
            var noteNumber = (SevenBitNumber)(i % 128);
            events[i] = new NoteOnEvent(noteNumber, (SevenBitNumber)64);
            events[pairCount + i] = new NoteOffEvent(noteNumber, (SevenBitNumber)0);
        }
        return events;
    }

    private static MidiEvent[] BuildMixedEvents(int pairCount)
    {
        int half = pairCount / 2;
        var events = new MidiEvent[pairCount * 2];
        int idx = 0;

        for (int i = 0; i < half; i++)
        {
            var noteNumber = (SevenBitNumber)(i % 128);
            events[idx++] = new NoteOnEvent(noteNumber, (SevenBitNumber)64);
            events[idx++] = new NoteOffEvent(noteNumber, (SevenBitNumber)0);
        }

        for (int i = half; i < pairCount; i++)
        {
            var noteNumber = (SevenBitNumber)(i % 128);
            events[idx++] = new NoteOnEvent(noteNumber, (SevenBitNumber)64);
        }
        for (int i = half; i < pairCount; i++)
        {
            var noteNumber = (SevenBitNumber)(i % 128);
            events[idx++] = new NoteOffEvent(noteNumber, (SevenBitNumber)0);
        }

        return events;
    }
}
