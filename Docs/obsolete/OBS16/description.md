`TimedEventsManagingUtilities.AddEvent` methods are now obsolete since they are nothing more than just calling [TimedEvent](xref:Melanchall.DryWetMidi.Interaction.TimedEvent) constructor and adding a new instance to a collection:

```csharp
eventsCollection.Add(new TimedEvent(midiEvent, time));
```