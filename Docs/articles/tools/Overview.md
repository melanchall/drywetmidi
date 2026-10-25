---
uid: a_tools_overview
---

# Overview

DryWetMIDI provides some useful tools to solve complex tasks:

* [Splitter](xref:a_splitter) to split objects and MIDI files in many different ways.
* [Quantizer](xref:a_quantizer) to quantize objects.
* [Merger](xref:a_merger) to merge nearby objects and MIDI files.
* [Resizer](xref:a_resizer) to resize groups of objects or MIDI files.
* [Repeater](xref:a_repeater) to repeat groups of objects or MIDI files.
* [Sanitizer](xref:a_sanitizer) to clean a MIDI file, removing redundant events and so on.
* [CSV serializer](xref:a_csv_serializer) to convert MIDI objects to CSV representation and read them back.

## Asynchronous tools

All tool families provide asynchronous counterparts for bulk operations. For example,
[`Sanitizer.SanitizeAsync`](xref:Melanchall.DryWetMidi.Tools.Sanitizer.SanitizeAsync*)
cleans a file in place, and
[`Quantizer.QuantizeAsync`](xref:Melanchall.DryWetMidi.Tools.Quantizer.QuantizeAsync*)
quantizes objects. `Splitter` and `Merger` operations that produce lazy sequences
use asynchronous enumeration; operations that return one result or a materialized
collection return a task. In-place operations return a task representing completion,
not a new copy of their input.

Quantizing, splitting, merging, resizing, repeating, and sanitizing are CPU-bound.
Their asynchronous APIs move that work from the calling thread to a thread-pool
worker. For example, an editor can keep drawing its window and responding to clicks
while another thread quantizes a large file. The processor still has to perform the
same calculations: this does not make quantization faster or automatically split one
operation across multiple cores. Scheduling the work can even add a little overhead.

Prefer synchronous methods for small inputs or when the caller already runs the
work on a background thread. In a service, limit how many operations run at once
so they do not compete for all available processor time. Lazy processing may still require
preprocessing or buffers before producing a result. CPU-bound asynchronous sequences
advance their underlying iterator in bounded batches, so they can compute ahead of
the consumer without retaining a worker thread while the consumer is idle.

Cancellation is cooperative. In-place transformations may leave completed changes,
and a lazy sequence may already have yielded results when cancellation or an error
occurs. Optional progress notifications count algorithm work checkpoints, are throttled,
and do not require pre-enumeration to determine a total.

For example, request cancellation after a time limit. Catch cancellation separately
from other failures, and remember that the file may already have been modified:

```csharp
using (var cancellation = new CancellationTokenSource())
{
    cancellation.CancelAfter(TimeSpan.FromSeconds(5));

    try
    {
        await midiFile.SanitizeAsync(cancellationToken: cancellation.Token);
    }
    catch (OperationCanceledException)
    {
        Console.WriteLine("Stopped; some changes may already have been applied.");
    }
}
```

To report progress, pass an `IProgress<T>` receiver for
[`AsyncOperationProgress`](xref:Melanchall.DryWetMidi.Common.AsyncOperationProgress).
Display its phase and processed work count rather than treating the count as a
percentage or as the number of notes in the output:

```csharp
var progress = new Progress<AsyncOperationProgress>(update =>
{
    Console.WriteLine($"{update.Phase}: {update.ProcessedItems} work checkpoints");

    if (update.IsCompleted)
    {
        Console.WriteLine("Completed.");
    }
});

await midiFile.SanitizeAsync(progress: progress);
```

CSV import/export uses asynchronous transport I/O; formatting, object detection,
and parsing still require CPU time. Caller-supplied streams remain caller-owned.
Canceled or failed writes may leave partial output.

Consume lazy tool results with `await foreach`. For example,
[`Splitter.SplitByChannelAsync`](xref:Melanchall.DryWetMidi.Tools.Splitter.SplitByChannelAsync*)
returns an `IAsyncEnumerable<MidiFile>` whose enumeration observes the supplied
cancellation token:

```csharp
using (var cancellation = new CancellationTokenSource())
{
    cancellation.CancelAfter(TimeSpan.FromSeconds(5));

    IAsyncEnumerable<MidiFile> parts = midiFile.SplitByChannelAsync(
        cancellationToken: cancellation.Token);

    await foreach (var part in parts)
    {
        Console.WriteLine($"Received a file with {part.Chunks.Count} chunks.");
    }
}
```

Handle `OperationCanceledException` as in the earlier example; cancellation does
not undo processing of results already consumed. `await foreach` disposes the enumerator on completion,
cancellation, or an early `break`, without materializing the entire sequence.

Do not mutate a file, its settings, or its collections while an asynchronous tool is
using them. Existing synchronous subclass hooks remain supported; asynchronous
execution does not add thread safety or transactional rollback.

For example, an override of
[`Repeater.ProcessPart`](xref:Melanchall.DryWetMidi.Tools.Repeater.ProcessPart*)
is also called when using
[`Repeater.RepeatAsync`](xref:Melanchall.DryWetMidi.Tools.Repeater.RepeatAsync*).
The override runs synchronously on the worker performing the repetition; it must
not access UI controls directly or start unawaited asynchronous work:

```csharp
class LoggingRepeater : Repeater
{
    protected override void ProcessPart(PartProcessingContext context)
    {
        Console.WriteLine("Processing a repeated part.");
        base.ProcessPart(context);
    }
}
```

Await the operation before changing the input or using its result:

```csharp
var repeater = new LoggingRepeater();
var repeatedFile = await repeater.RepeatAsync(midiFile, repeatsNumber: 2);

await repeatedFile.WriteAsync("repeated.mid", overwriteFile: true);
```