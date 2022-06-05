`ResizeNotesUtilities` class and its methods have been generalized by the [Resizer](xref:Melanchall.DryWetMidi.Tools.Resizer) class which can resize groups of objects of different types simultaneously.

Quick example of how to resize objects group using the new tool:

```csharp
objects.ResizeObjectsGroup(
    new MetricTimeSpan(0, 1, 0),
    TempoMap.Default,
    new ObjectsGroupResizingSettings
    {
        DistanceCalculationType = TimeSpanType.Metric
    });
```