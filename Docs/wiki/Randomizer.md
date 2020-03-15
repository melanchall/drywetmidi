To randomize time of timed events, notes and chords DryWetMIDI provides following classes:

* `TimedEventsRandomizer`
* `NotesRandomizer`
* `ChordsRandomizer`

These classes have `Randomize` method which do randomization of time:

```csharp
Randomize(IEnumerable<TObject> objects,
          IBounds bounds,
          TempoMap tempoMap,
          TSettings settings = null)
```

where `TObject` and `TSettings` are:

* `TimedEvent` and `TimedEventsRandomizingSettings` for `TimedEventsRandomizer`;
* `Note` and `NotesRandomizingSettings` for `NotesRandomizer`;
* `Chord` and `ChordsRandomizingSettings` for `ChordsRandomizer`.

In general, randomization is very similar to quantizing so please read [Quantizer](Quantizer.md) article to learn more.

The only thing that should be discussed here is _bounds_ parameter of the `Randomize` method. Bounds define an area where random time should be calculated. At now there is one built-in implementation of the `IBounds` interface � `ConstantBounds`. Using this class random values will be generated in area starting at the specified distance from an object's time to the left and ended at the specified distance to the right. These distance can be different.