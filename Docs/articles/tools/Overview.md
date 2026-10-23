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

All tool families provide `Async` counterparts for bulk operations. Splitter and
merger operations that produce lazy sequences use asynchronous enumeration; operations
that return one result or a materialized collection return a task. In-place operations
return a task representing completion, not a new copy of their input.

Quantizing, splitting, merging, resizing, repeating, and sanitizing are CPU-bound.
Their asynchronous APIs offload computation to the thread pool for responsiveness,
not faster execution. Prefer synchronous methods for small inputs and bound the
number of concurrent operations in services. Lazy processing may still require
preprocessing or buffers before producing a result. CPU-bound asynchronous sequences
advance their underlying iterator in bounded batches, so they can compute ahead of
the consumer without retaining a worker thread while the consumer is idle.

Cancellation is cooperative. In-place transformations may leave completed changes,
and a lazy sequence may already have yielded results when cancellation or an error
occurs. Optional progress notifications count algorithm work items, are throttled,
and do not require pre-enumeration to determine a total.

CSV import/export uses asynchronous transport I/O; formatting, object detection,
and parsing still require CPU time. Caller-supplied streams remain caller-owned.
Canceled or failed writes may leave partial output.

Do not mutate a file, its settings, or its collections while an asynchronous tool is
using them. Existing synchronous subclass hooks remain supported; asynchronous
execution does not add thread safety or transactional rollback.