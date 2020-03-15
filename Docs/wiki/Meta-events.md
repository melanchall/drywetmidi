Meta events represent non-MIDI data useful for the MIDI format. MIDI file can contain following meta events:

* [Channel Prefix](#channel-prefix)
* [End of Track](#end-of-track)
* [Key Signature](#key-signature)
* [Port Prefix](#port-prefix)
* [Sequence Number](#sequence-number)
* [Sequencer Specific](#sequencer-specific)
* [Set Tempo](#set-tempo)
* [SMPTE Offset](#smpte-offset)
* [Time Signature](#time-signature)
* [Copyright Notice](#copyright-notice)
* [Cue Point](#cue-point)
* [Device Name](#device-name)
* [Instrument Name](#instrument-name)
* [Lyric](#lyric)
* [Marker](#marker)
* [Program Name](#program-name)
* [Sequence/Track Name](#sequencetrack-name)
* [Text](#text)
* [Unknown](#unknown)

The class diagram below shows meta event types in the DryWetMIDI:

![Meta event classes diagram](Images/ClassDiagrams/MetaEventsClassDiagram.png)

`MetaEvent` is the base class which represents meta event.

```csharp
public abstract class MetaEvent : MidiEvent
{
    // ...
    protected abstract void ReadContent(MidiReader reader, ReadingSettings settings, int size);
    protected abstract void WriteContent(MidiWriter writer, WritingSettings settings);
    protected abstract int GetContentSize();
    // ...
}
```

This class has three protected abstract methods which must be implemented by any non-abstract class derived from `MetaEvent`. These methods need to be correctly implemented when you create your custom meta events. Visit [Custom meta events](Custom-meta-events.md) page to learn more.

Some meta events are based on `BaseTextEvent` that provides `Text` property.

```csharp
public abstract class BaseTextEvent : MetaEvent
{
    // ...
    public string Text { get; set; }
    // ...
}
```

Let's see on all meta event classes presented in the DryWetMIDI.

### Channel Prefix

```csharp
public sealed class ChannelPrefixEvent : MetaEvent
{
    // ...
    public ChannelPrefixEvent() { /*...*/ }
    public ChannelPrefixEvent(byte channel) { /*...*/ }
    // ...
    public byte Channel { get; set; }
    // ...
}
```

### End of Track

`EndOfTrackEvent` class is internal and cannot be created by user of the DryWetMIDI.

### Key Signature

```csharp
public sealed class KeySignatureEvent : MetaEvent
{
    // ...
    public const sbyte DefaultKey = 0;
    public const byte DefaultScale = 0;
    // ...
    public KeySignatureEvent() { /*...*/ }
    public KeySignatureEvent(sbyte key, byte scale) { /*...*/ }
    // ...
    public sbyte Key { get; set; }
    public byte Scale { get; set; }
    // ...
}
```

### Port Prefix

```csharp
public sealed class PortPrefixEvent : MetaEvent
{
    // ...
    public PortPrefixEvent() { /*...*/ }
    public PortPrefixEvent(byte port) { /*...*/ }
    // ...
    public byte Port { get; set; }
    // ...
}
```

### Sequence Number

```csharp
public sealed class SequenceNumberEvent : MetaEvent
{
    // ...
    public SequenceNumberEvent() { /*...*/ }
    public SequenceNumberEvent(ushort number) { /*...*/ }
    // ...
    public ushort Number { get; set; }
    // ...
}
```

### Sequencer Specific

```csharp
public sealed class SequencerSpecificEvent : MetaEvent
{
    // ...
    public SequencerSpecificEvent() { /*...*/ }
    public SequencerSpecificEvent(byte[] data) { /*...*/ }
    // ...
    public byte[] Data { get; set; }
    // ...
}
```

### Set Tempo

```csharp
public sealed class SetTempoEvent : MetaEvent
{
    // ...
    public const long DefaultTempo = 500000;
    // ...
    public SetTempoEvent() { /*...*/ }
    public SetTempoEvent(long microsecondsPerQarterNote) { /*...*/ }
    // ...
    public long MicrosecondsPerQarterNote { get; set; }
    // ...
}
```

### SMPTE Offset

```csharp
public sealed class SmpteOffsetEvent : MetaEvent
{
    // ...
    public SmpteOffsetEvent() { /*...*/ }
    public SmpteOffsetEvent(SmpteFormat format,
                            byte hours,
                            byte minutes,
                            byte seconds,
                            byte frames,
                            byte subFrames) { /*...*/ }
    // ...
    public SmpteFormat Format { get; set; }
    public byte Hours { get; set; }
    public byte Minutes { get; set; }
    public byte Seconds { get; set; }
    public byte Frames { get; set; }
    public byte SubFrames { get; set; }
    // ...
}
```

### Time Signature

```csharp
public sealed class TimeSignatureEvent : MetaEvent
{
    // ...
    public const byte DefaultNumerator = 4;
    public const byte DefaultDenominator = 4;
    public const byte DefaultClocksPerClick = 24;
    public const byte Default32ndNotesPerBeat = 8;
    // ...
    public TimeSignatureEvent() { /*...*/ }
    public TimeSignatureEvent(byte numerator,
                              byte denominator,
                              byte clocksPerClick,
                              byte numberOf32ndNotesPerBeat) { /*...*/ }
    // ...
    public byte Numerator { get; set; }
    public byte Denominator { get; set; }
    public byte ClocksPerClick { get; set; }
    public byte NumberOf32ndNotesPerBeat { get; set; }
    // ...
}
```

### Copyright Notice

```csharp
public sealed class CopyrightNoticeEvent : BaseTextEvent
{
    // ...
    public CopyrightNoticeEvent() { /*...*/ }
    public CopyrightNoticeEvent(string text) { /*...*/ }
    // ...
}
```

### Cue Point

```csharp
public sealed class CuePointEvent : BaseTextEvent
{
    // ...
    public CuePointEvent() { /*...*/ }
    public CuePointEvent(string text) { /*...*/ }
    // ...
}
```

### Device Name

```csharp
public sealed class DeviceNameEvent : BaseTextEvent
{
    // ...
    public DeviceNameEvent() { /*...*/ }
    public DeviceNameEvent(string deviceName) { /*...*/ }
    // ...
}
```

### Instrument Name

```csharp
public sealed class InstrumentNameEvent : BaseTextEvent
{
    // ...
    public InstrumentNameEvent() { /*...*/ }
    public InstrumentNameEvent(string instrumentName) { /*...*/ }
    // ...
}
```

### Lyric

```csharp
public sealed class LyricEvent : BaseTextEvent
{
    // ...
    public LyricEvent() { /*...*/ }
    public LyricEvent(string text) { /*...*/ }
    // ...
}
```

### Marker

```csharp
public sealed class MarkerEvent : BaseTextEvent
{
    // ...
    public MarkerEvent() { /*...*/ }
    public MarkerEvent(string text) { /*...*/ }
    // ...
}
```

### Program Name

```csharp
public sealed class ProgramNameEvent : BaseTextEvent
{
    // ...
    public ProgramNameEvent() { /*...*/ }
    public ProgramNameEvent(string programName) { /*...*/ }
    // ...
}
```

### Sequence/Track Name

```csharp
public sealed class SequenceTrackNameEvent : BaseTextEvent
{
    // ...
    public SequenceTrackNameEvent() { /*...*/ }
    public SequenceTrackNameEvent(string text) { /*...*/ }
    // ...
}
```

### Text

```csharp
public sealed class TextEvent : BaseTextEvent
{
    // ...
    public TextEvent() { /*...*/ }
    public TextEvent(string text) { /*...*/ }
    // ...
}
```

### Unknown

```csharp
public sealed class UnknownMetaEvent : MetaEvent
{
    // ...
    public byte StatusByte { get; }
    public byte[] Data { get; }
    // ...
}
```