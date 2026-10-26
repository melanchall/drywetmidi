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

## Asynchronous CSV I/O

[`CsvSerializer.SerializeToCsvAsync`](xref:Melanchall.DryWetMidi.Tools.CsvSerializer.SerializeToCsvAsync*)
and
[`CsvSerializer.DeserializeFileFromCsvAsync`](xref:Melanchall.DryWetMidi.Tools.CsvSerializer.DeserializeFileFromCsvAsync*)
await transport writes and reads. Formatting, parsing, and object reconstruction
run on the calling thread until an incomplete I/O operation is awaited.
Subsequent processing runs on the continuation thread; these methods do not
schedule CPU-bound processing on a background worker.

CSV import is a bulk operation: the returned task completes with a fully
materialized result, not a lazy asynchronous sequence. Export writes buffered
CSV data as the input is processed. Caller-supplied streams remain caller-owned;
path overloads manage their own file streams. Cancellation or failure can leave
partial output, and no rollback or atomic replacement is implied.

For example, export a MIDI file to CSV and import it again:

```csharp
using (var cancellation = new CancellationTokenSource())
{
    cancellation.CancelAfter(TimeSpan.FromSeconds(30));

    var progress = new Progress<AsyncOperationProgress>(update =>
    {
        Console.WriteLine($"{update.ProcessedItems} work checkpoints");

        if (update.IsCompleted)
        {
            Console.WriteLine("CSV export completed.");
        }
    });

    try
    {
        var midiFile = await MidiFile.ReadAsync(
            "input.mid",
            cancellationToken: cancellation.Token);

        await midiFile.SerializeToCsvAsync(
            "output.csv",
            overwriteFile: true,
            progress: progress,
            cancellationToken: cancellation.Token);

        var restoredFile = await CsvSerializer.DeserializeFileFromCsvAsync(
            "output.csv",
            cancellationToken: cancellation.Token);

        await restoredFile.WriteAsync(
            "restored.mid",
            overwriteFile: true,
            cancellationToken: cancellation.Token);
    }
    catch (OperationCanceledException)
    {
        Console.WriteLine("Canceled; output files may be incomplete.");
    }
}
```

Optional [`AsyncOperationProgress`](xref:Melanchall.DryWetMidi.Common.AsyncOperationProgress)
notifications count work checkpoints, not bytes, lines, notes, or output records.
Reports are throttled; totals can be unknown without pre-enumerating the input.
Successful completion emits a final notification after output has been flushed.
Do not mutate the input, its settings, or its collections while an operation
uses them. Asynchronous I/O does not add thread safety.
