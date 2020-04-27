---
uid: a_custom_meta_event
---

# Custom meta events

Meta events specify non-MIDI information useful to specific application. As with [custom chunks](xref:a_custom_chunk), future or custom meta events may be designed. Format of meta events allows to programs which don't know about these new events to skip them without reading process failure. DryWetMIDI allows you to implement custom meta events which can be written to a MIDI file [track chunk](xref:Melanchall.DryWetMidi.Core.TrackChunk) and be read from it.

For example, let's create an event which will hold an image. Custom meta event must be derived from the [MetaEvent](xref:Melanchall.DryWetMidi.Core.MetaEvent) and must implement four abstract methods:

* [ReadContent](xref:Melanchall.DryWetMidi.Core.MetaEvent.ReadContent(Melanchall.DryWetMidi.Core.MidiReader,Melanchall.DryWetMidi.Core.ReadingSettings,System.Int32));
* [WriteContent](xref:Melanchall.DryWetMidi.Core.MetaEvent.WriteContent(Melanchall.DryWetMidi.Core.MidiWriter,Melanchall.DryWetMidi.Core.WritingSettings));
* [GetContentSize](xref:Melanchall.DryWetMidi.Core.MetaEvent.GetContentSize(Melanchall.DryWetMidi.Core.WritingSettings));
* [CloneEvent](xref:Melanchall.DryWetMidi.Core.MidiEvent.CloneEvent).

Also a class must have parameterless constructor.

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

Every meta event contains size of the event's content. Size is passed to `ReadContent` through `size` parameter so we know how much bytes we need to read in order to restore an image.

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
public override MidiEvent CloneEvent()
{
    return new ImageEvent(Image?.Clone() as Image);
}
```

Custom meta event is completely implemented. In order to read and write it we must assign status byte to the event. You have to pick value from the **[0x5F; 0x7E]** range which will be the status byte of your event type. You can get status bytes of standard meta events via [MetaEvent.GetStandardMetaEventStatusBytes](xref:Melanchall.DryWetMidi.Core.MetaEvent.GetStandardMetaEventStatusBytes). See code sample below to know how to read and write custom meta event:

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

file.Write("My Great Song.mid",
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