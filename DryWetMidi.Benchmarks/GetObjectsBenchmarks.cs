using BenchmarkDotNet.Attributes;
using Melanchall.DryWetMidi.Common;
using Melanchall.DryWetMidi.Core;
using Melanchall.DryWetMidi.Interaction;

namespace Melanchall.DryWetMidi.Benchmarks;

[MemoryDiagnoser]
public class GetObjectsBenchmarks
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

    // ── ObjectType.Note (GetNotesAndTimedEventsLazy path, notes only filtered out) ─────

    [Benchmark(Description = "GetObjects(Note) – no overlaps")]
    public int GetObjects_Note_NoOverlap()
    {
        var objects = _noOverlapEvents.GetObjects(ObjectType.Note);
        return objects.Count;
    }

    [Benchmark(Description = "GetObjects(Note) – with overlaps")]
    public int GetObjects_Note_WithOverlap()
    {
        var objects = _withOverlapEvents.GetObjects(ObjectType.Note);
        return objects.Count;
    }

    [Benchmark(Description = "GetObjects(Note) – mixed")]
    public int GetObjects_Note_Mixed()
    {
        var objects = _mixedEvents.GetObjects(ObjectType.Note);
        return objects.Count;
    }

    // ── ObjectType.TimedEvent ──────────────────────────────────────────────────────────

    [Benchmark(Description = "GetObjects(TimedEvent) – no overlaps")]
    public int GetObjects_TimedEvent_NoOverlap()
    {
        var objects = _noOverlapEvents.GetObjects(ObjectType.TimedEvent);
        return objects.Count;
    }

    [Benchmark(Description = "GetObjects(TimedEvent) – with overlaps")]
    public int GetObjects_TimedEvent_WithOverlap()
    {
        var objects = _withOverlapEvents.GetObjects(ObjectType.TimedEvent);
        return objects.Count;
    }

    [Benchmark(Description = "GetObjects(TimedEvent) – mixed")]
    public int GetObjects_TimedEvent_Mixed()
    {
        var objects = _mixedEvents.GetObjects(ObjectType.TimedEvent);
        return objects.Count;
    }

    // ── ObjectType.Note | ObjectType.TimedEvent (GetNotesAndTimedEventsLazy path) ─────

    [Benchmark(Description = "GetObjects(Note|TimedEvent) – no overlaps")]
    public int GetObjects_NoteAndTimedEvent_NoOverlap()
    {
        var objects = _noOverlapEvents.GetObjects(ObjectType.Note | ObjectType.TimedEvent);
        return objects.Count;
    }

    [Benchmark(Description = "GetObjects(Note|TimedEvent) – with overlaps")]
    public int GetObjects_NoteAndTimedEvent_WithOverlap()
    {
        var objects = _withOverlapEvents.GetObjects(ObjectType.Note | ObjectType.TimedEvent);
        return objects.Count;
    }

    [Benchmark(Description = "GetObjects(Note|TimedEvent) – mixed")]
    public int GetObjects_NoteAndTimedEvent_Mixed()
    {
        var objects = _mixedEvents.GetObjects(ObjectType.Note | ObjectType.TimedEvent);
        return objects.Count;
    }

    // ── ObjectType.Chord ──────────────────────────────────────────────────────────────

    [Benchmark(Description = "GetObjects(Chord) – no overlaps")]
    public int GetObjects_Chord_NoOverlap()
    {
        var objects = _noOverlapEvents.GetObjects(ObjectType.Chord);
        return objects.Count;
    }

    [Benchmark(Description = "GetObjects(Chord) – with overlaps")]
    public int GetObjects_Chord_WithOverlap()
    {
        var objects = _withOverlapEvents.GetObjects(ObjectType.Chord);
        return objects.Count;
    }

    [Benchmark(Description = "GetObjects(Chord) – mixed")]
    public int GetObjects_Chord_Mixed()
    {
        var objects = _mixedEvents.GetObjects(ObjectType.Chord);
        return objects.Count;
    }

    // ── ObjectType.Chord | ObjectType.TimedEvent ──────────────────────────────────────

    [Benchmark(Description = "GetObjects(Chord|TimedEvent) – no overlaps")]
    public int GetObjects_ChordAndTimedEvent_NoOverlap()
    {
        var objects = _noOverlapEvents.GetObjects(ObjectType.Chord | ObjectType.TimedEvent);
        return objects.Count;
    }

    [Benchmark(Description = "GetObjects(Chord|TimedEvent) – with overlaps")]
    public int GetObjects_ChordAndTimedEvent_WithOverlap()
    {
        var objects = _withOverlapEvents.GetObjects(ObjectType.Chord | ObjectType.TimedEvent);
        return objects.Count;
    }

    [Benchmark(Description = "GetObjects(Chord|TimedEvent) – mixed")]
    public int GetObjects_ChordAndTimedEvent_Mixed()
    {
        var objects = _mixedEvents.GetObjects(ObjectType.Chord | ObjectType.TimedEvent);
        return objects.Count;
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
