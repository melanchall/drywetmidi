Methods from `GetNotesAndRestsUtilities` are now obsolete and you should use `GetObjects` methods from [GetObjectsUtilities](xref:Melanchall.DryWetMidi.Interaction.GetObjectsUtilities). Example how you can get notes and rests:

```csharp
var notesAndRests = midiFile.GetObjects(ObjectType.Note | ObjectType.Rest);
```

[RestSeparationPolicy](xref:Melanchall.DryWetMidi.Interaction.RestSeparationPolicy) can be specified via [ObjectDetectionSettings](xref:Melanchall.DryWetMidi.Interaction.ObjectDetectionSettings):

```csharp
var notesAndRests = midiFile.GetObjects(
    ObjectType.Note | ObjectType.Rest,
    new ObjectDetectionSettings
    {
        RestDetectionSettings = new RestDetectionSettings
        {
            RestSeparationPolicy = RestSeparationPolicy.SeparateByChannel
        }
    });
```