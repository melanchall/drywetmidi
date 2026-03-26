Different methods to search timed objects at the specified time have been replaced with single [`TimedObjectUtilities.AtTime`](xref:Melanchall.DryWetMidi.Interaction.TimedObjectUtilities.AtTime*). It takes [`ITimeSpan`](xref:Melanchall.DryWetMidi.Interaction.ITimeSpan) and has no overload where time is represented by `long`. If you want to search timed objects by time in MIDI ticks, you just need to use [`MidiTimeSpan`](xref:Melanchall.DryWetMidi.Interaction.MidiTimeSpan):

```csharp
var objectsAtTime = timedObjects.AtTime(new MidiTimeSpan(1000), tempoMap);
```
And yes, you need to pass [`TempoMap`](xref:Melanchall.DryWetMidi.Interaction.TempoMap) always.

More than that, the single method allows to search objects of different types at the same time, no separate methods for collections of just [`ITimedObject`](xref:Melanchall.DryWetMidi.Interaction.ITimedObject) and [`ILengthedObject`](xref:Melanchall.DryWetMidi.Interaction.ILengthedObject) instances. Since `AtTime`, `StartAtTime` and `EndAtTime` methods for [`ILengthedObject`](xref:Melanchall.DryWetMidi.Interaction.ILengthedObject) are removed now, there is a question: "_How can we match only start time of an object or end one?_". The answer is: the new method takes **entire object** into account. So if you want to exclude objects which endpoint is exactly at the specified time, you need to do it manually. For example:

```csharp
var timeInTicks = 1000;
var notesWithMiddleAtTime = timedObjects
    .AtTime(new MidiTimeSpan(timeInTicks), tempoMap)
    .Where(o => o.Time != timeInTicks && o.EndTime != timeInTicks);
```