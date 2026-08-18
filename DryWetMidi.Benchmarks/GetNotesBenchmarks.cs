using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Jobs;
using Melanchall.DryWetMidi.Common;
using Melanchall.DryWetMidi.Core;
using Melanchall.DryWetMidi.Interaction;

namespace Melanchall.DryWetMidi.Benchmarks;

[MediumRunJob]
[MemoryDiagnoser]
public class GetNotesBenchmarks
{
    private const int EventsCount = 10_000;

    private MidiEvent[] _noOverlapEvents = null!;
    private MidiEvent[] _withOverlapEvents = null!;
    private MidiEvent[] _mixedEvents = null!;

    [GlobalSetup]
    public void Setup()
    {
        _noOverlapEvents = BuildNoOverlapEvents(EventsCount);
        _withOverlapEvents = BuildOverlapEvents(EventsCount);
        _mixedEvents = BuildMixedEvents(EventsCount);
    }

    [Benchmark(Description = "GetNotes – no overlaps")]
    public int GetNotes_NoOverlap()
    {
        var notes = _noOverlapEvents.GetNotes();
        return notes.Count;
    }

    [Benchmark(Description = "GetNotes – with overlaps")]
    public int GetNotes_WithOverlap()
    {
        var notes = _withOverlapEvents.GetNotes();
        return notes.Count;
    }

    [Benchmark(Description = "GetNotes – mixed")]
    public int GetNotes_Mixed()
    {
        var notes = _mixedEvents.GetNotes();
        return notes.Count;
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
        // All NoteOn events followed by all NoteOff events (maximum overlap per note)
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
        // First quarter: non-overlapping (NoteOn/NoteOff interleaved)
        // Second quarter: overlapping (all NoteOns then all NoteOffs)
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
