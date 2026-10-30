---
uid: a_custom_meta_event
---

# Custom meta events

Meta events specify non-MIDI information useful to specific applications. As with [custom chunks](xref:a_custom_chunk), future or custom meta events may be designed. Format of meta events allows programs which don't know about these new events to skip them without reading process failure. DryWetMIDI allows you to implement custom meta events which can be written to a MIDI file [track chunk](xref:Melanchall.DryWetMidi.Core.TrackChunk) and be read from it.

For example, let's create an event which will hold an image. Custom meta event must be derived from [`MetaEvent`](xref:Melanchall.DryWetMidi.Core.MetaEvent) and must implement six abstract methods:

* [ReadContent](xref:Melanchall.DryWetMidi.Core.MetaEvent.ReadContent(Melanchall.DryWetMidi.Core.MidiReader,Melanchall.DryWetMidi.Core.ReadingSettings,System.Int32));
* [WriteContent](xref:Melanchall.DryWetMidi.Core.MetaEvent.WriteContent(Melanchall.DryWetMidi.Core.MidiWriter,Melanchall.DryWetMidi.Core.WritingSettings));
* [`ReadContentAsync`](xref:Melanchall.DryWetMidi.Core.MetaEvent.ReadContentAsync(Melanchall.DryWetMidi.Core.MidiReader,Melanchall.DryWetMidi.Core.ReadingSettings,System.Int32,System.Threading.CancellationToken));
* [`WriteContentAsync`](xref:Melanchall.DryWetMidi.Core.MetaEvent.WriteContentAsync(Melanchall.DryWetMidi.Core.MidiWriter,Melanchall.DryWetMidi.Core.WritingSettings,System.Threading.CancellationToken));
* [GetContentSize](xref:Melanchall.DryWetMidi.Core.MetaEvent.GetContentSize(Melanchall.DryWetMidi.Core.WritingSettings));
* [CloneEvent](xref:Melanchall.DryWetMidi.Core.MidiEvent.CloneEvent).

Also a class must have a parameterless constructor:

```csharp
public sealed class ImageEvent : MetaEvent
{
    public ImageEvent()
        : base()
    {
    }

    public ImageEvent(Image image)
        : this()
    {
        Image = image;
    }

    public Image Image { get; set; }

    protected override void ReadContent(MidiReader reader, ReadingSettings settings, int size)
    {
        throw new NotImplementedException();
    }

    protected override void WriteContent(MidiWriter writer, WritingSettings settings)
    {
        throw new NotImplementedException();
    }

    protected override Task ReadContentAsync(
        MidiReader reader,
        ReadingSettings settings,
        int size,
        CancellationToken cancellationToken)
    {
        throw new NotImplementedException();
    }

    protected override Task WriteContentAsync(
        MidiWriter writer,
        WritingSettings settings,
        CancellationToken cancellationToken)
    {
        throw new NotImplementedException();
    }

    protected override int GetContentSize(WritingSettings settings)
    {
        throw new NotImplementedException();
    }

    protected override MidiEvent CloneEvent()
    {
        throw new NotImplementedException();
    }
}
```

Now we implement methods mentioned above. Start from the `ReadContent`:

```csharp
protected override void ReadContent(MidiReader reader, ReadingSettings settings, int size)
{
    if (size == 0)
        return;

    var imageBytes = reader.ReadBytes(size);

    var converter = new ImageConverter();
    Image = (Image)converter.ConvertFrom(imageBytes);
}
```

Every meta event contains the size of the event's content. Size is passed to `ReadContent` through the `size` parameter so we know how many bytes we need to read in order to restore an image.

Now let's implement `WriteContent`:

```csharp
protected override void WriteContent(MidiWriter writer, WritingSettings settings)
{
    if (Image == null)
        return;

    var converter = new ImageConverter();
    var imageBytes = (byte[])converter.ConvertTo(Image, typeof(byte[]));

    writer.WriteBytes(imageBytes);
}
```

Now we have to implement `GetContentSize`:

```csharp
protected override int GetContentSize(WritingSettings settings)
{
    if (Image == null)
        return 0;

    var converter = new ImageConverter();
    var imageBytes = (byte[])converter.ConvertTo(Image, typeof(byte[]));

    return imageBytes.Length;
}
```

Value returned by this method will be written to the event as its content size.

To support cloning of an event we need to implement `CloneEvent` method:

```csharp
protected override MidiEvent CloneEvent()
{
    return new ImageEvent(Image?.Clone() as Image);
}
```

## Asynchronous content methods

The asynchronous content overrides are mandatory, just like the synchronous ones.
Add `using System.Threading;` and `using System.Threading.Tasks;` for the
following examples. Implement awaited stream I/O for `ImageEvent` as follows:

```csharp
protected override async Task ReadContentAsync(
    MidiReader reader,
    ReadingSettings settings,
    int size,
    CancellationToken cancellationToken)
{
    cancellationToken.ThrowIfCancellationRequested();

    if (size == 0)
        return;

    var imageBytes = await reader.ReadBytesAsync(size, cancellationToken)
        .ConfigureAwait(false);
    if (imageBytes.Length != size &&
        settings.NotEnoughBytesPolicy == NotEnoughBytesPolicy.Abort)
    {
        throw new System.IO.EndOfStreamException("Not enough bytes to read the image.");
    }

    var converter = new ImageConverter();
    Image = (Image)converter.ConvertFrom(imageBytes);
}

protected override async Task WriteContentAsync(
    MidiWriter writer,
    WritingSettings settings,
    CancellationToken cancellationToken)
{
    cancellationToken.ThrowIfCancellationRequested();

    if (Image == null)
        return;

    var converter = new ImageConverter();
    var imageBytes = (byte[])converter.ConvertTo(Image, typeof(byte[]));

    await writer.WriteBytesAsync(imageBytes, cancellationToken).ConfigureAwait(false);
}
```

[`MidiFile.ReadAsync`](xref:Melanchall.DryWetMidi.Core.MidiFile.ReadAsync*) and
[`MidiFile.WriteAsync`](xref:Melanchall.DryWetMidi.Core.MidiFile.WriteAsync*),
and asynchronous event operations on
[`MidiTokensReader`](xref:Melanchall.DryWetMidi.Core.MidiTokensReader) and
[`MidiTokensWriter`](xref:Melanchall.DryWetMidi.Core.MidiTokensWriter), use these
overrides. They receive the original reader or writer and settings, not a
payload-only reader or writer. The library does not stage custom event content
before invoking them. The byte array above is needed by `ImageConverter`; other
custom events can read and write individual fields directly.

Read and write only the content, not the status bytes or length prefix. `size`
is the declared content length; unread trailing bytes are skipped by the event
reader. Do not read beyond it, and ensure `GetContentSize` matches the number of
bytes written. Do not dispose the supplied reader or writer. Pass the token to
each asynchronous I/O operation and await operations sequentially.
`GetContentSize`, cloning and image conversion remain synchronous CPU work.
Cancellation is cooperative and cannot interrupt image conversion; cancellation
or failure can leave input consumed or output partially written.

### Synchronous-only implementations

There is no automatic fallback to `ReadContent` or `WriteContent`. An event
that deliberately supports only synchronous I/O must explicitly reject async use:

```csharp
protected override Task ReadContentAsync(
    MidiReader reader,
    ReadingSettings settings,
    int size,
    CancellationToken cancellationToken)
{
    throw new NotSupportedException("This event supports only synchronous reading.");
}

protected override Task WriteContentAsync(
    MidiWriter writer,
    WritingSettings settings,
    CancellationToken cancellationToken)
{
    throw new NotSupportedException("This event supports only synchronous writing.");
}
```

Synchronous operations still call the synchronous overrides. Async operations
fail with `NotSupportedException` when they reach this event.

### Fast synchronous implementations

For genuinely memory-only work, an override can perform synchronous work and
return `Task.CompletedTask`:

```csharp
protected override Task ReadContentAsync(
    MidiReader reader,
    ReadingSettings settings,
    int size,
    CancellationToken cancellationToken)
{
    cancellationToken.ThrowIfCancellationRequested();
    ReadContent(reader, settings, size);

    return Task.CompletedTask;
}

protected override Task WriteContentAsync(
    MidiWriter writer,
    WritingSettings settings,
    CancellationToken cancellationToken)
{
    cancellationToken.ThrowIfCancellationRequested();
    WriteContent(writer, settings);

    return Task.CompletedTask;
}
```

This is not asynchronous stream I/O. Use it only when the synchronous methods
cannot block on the supplied reader or writer. Ordinary reader/writer buffering
does not guarantee this: it can refill or flush synchronously. Delegating to
synchronous methods can also fail on async-only streams. For an empty event,
check cancellation and return `Task.CompletedTask` without I/O; validate that
the expected content is empty and return zero from `GetContentSize`.

## Registering and using the event

In order to read and write the custom meta event we must assign a status byte to the event. You have to pick a value from the **[0x5F; 0x7E]** range which will be the status byte of your event type. See code sample below to know how to read and write custom meta event:

```csharp
// Define collection of custom meta event types along with
// corresponding status bytes.

var customMetaEventTypes = new EventTypesCollection
{
    { typeof(ImageEvent), 0x5F }
};

// Write an image event to an existing file.

var file = MidiFile.Read("My Great Song.mid");

var trackChunk = file.Chunks.OfType<TrackChunk>().First();

var image = Image.FromFile("My image.jpg");
var imageEvent = new ImageEvent(image);
trackChunk.Events.Add(imageEvent);

file.Write(
    "My Great Song.mid",
    true,
    MidiFileFormat.MultiTrack,
    new WritingSettings
    {
        CustomMetaEventTypes = customMetaEventTypes
    });

// Read a MIDI file with ImageEvent inside.
//
// Note that if you don't specify custom meta event through CustomMetaEventTypes
// property of the ReadingSettings it will be read as UnknownMetaEvent.

var updatedFile = MidiFile.Read(
    "My Great Song.mid",
    new ReadingSettings
    {
        CustomMetaEventTypes = customMetaEventTypes
    });
```

The same registration is used by the asynchronous file operations:

```csharp
await file.WriteAsync(
    "My Great Song.mid",
    overwriteFile: true,
    settings: new WritingSettings
    {
        CustomMetaEventTypes = customMetaEventTypes
    },
    cancellationToken: cancellationToken);

var updatedFileAsync = await MidiFile.ReadAsync(
    "My Great Song.mid",
    settings: new ReadingSettings
    {
        CustomMetaEventTypes = customMetaEventTypes
    },
    cancellationToken: cancellationToken);
```