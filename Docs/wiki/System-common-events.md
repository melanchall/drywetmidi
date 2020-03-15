System common events are those that prompt all devices on the MIDI system to respond (are not specific to a MIDI channel), but do not require an immediate response from the receiving MIDI devices (unlike system real-time events):

* [MIDI Time Code (Quarter Frame)](#midi-time-code)
* [Song Position Pointer](#song-position-pointer)
* [Song Select](#song-select)
* [Tune Request](#tune-request)

The class diagram below shows system common event types in the DryWetMIDI:

![System common event classes diagram](Images/ClassDiagrams/SystemCommonEventsClassDiagram.png)

Let's see on all system common event classes presented in the DryWetMIDI.

### MIDI Time Code

A MIDI event that carries the MIDI quarter frame message is timing information in the hours:minutes:seconds:frames format (similar to SMPTE) that is used to synchronize MIDI devices.

```csharp
public sealed class MidiTimeCodeEvent : SystemCommonEvent
{
    // ...
    public MidiTimeCodeEvent() { /*...*/ }
    public MidiTimeCodeEvent(MidiTimeCodeComponent component, FourBitNumber componentValue) { /*...*/ }
    // ...
    public MidiTimeCodeComponent Component { get; set; }
    public FourBitNumber ComponentValue { get; set; }
    // ...
}
```

### Song Position Pointer

A MIDI event that carries the MIDI song position pointer message tells a MIDI device to cue to a point in the MIDI sequence to be ready to play.

```csharp
public sealed class SongPositionPointerEvent : SystemCommonEvent
{
    // ...
    public SongPositionPointerEvent() { /*...*/ }
    public SongPositionPointerEvent(ushort pointerValue) { /*...*/ }
    // ...
    public ushort PointerValue { get; set; }
    // ...
}
```

### Song Select

A MIDI event that carries the MIDI song request message (also known as a "song select message") tells a MIDI device to select a sequence for playback.

```csharp
public sealed class SongSelectEvent : SystemCommonEvent
{
    // ...
    public SongSelectEvent() { /*...*/ }
    public SongSelectEvent(SevenBitNumber number) { /*...*/ }
    // ...
    public SevenBitNumber Number { get; set; }
    // ...
}
```

### Tune Request

A MIDI event that carries the MIDI tune request message tells a MIDI device to tune itself.

```csharp
public sealed class TuneRequestEvent : SystemCommonEvent
{
    // ...
}
```

---

_Descriptions of MIDI events are taken from [RecordingBlogs.com](https://www.recordingblogs.com)._