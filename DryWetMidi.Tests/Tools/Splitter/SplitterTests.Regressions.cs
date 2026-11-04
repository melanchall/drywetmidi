using Melanchall.DryWetMidi.Common;
using Melanchall.DryWetMidi.Core;
using Melanchall.DryWetMidi.Interaction;
using Melanchall.DryWetMidi.Tests.Utilities;
using Melanchall.DryWetMidi.Tools;
using NUnit.Framework;
using NUnit.Framework.Legacy;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace Melanchall.DryWetMidi.Tests.Tools
{
    public sealed partial class SplitterTests
    {
        #region Nested classes

        public enum RegressionSliceOperation
        {
            Grid,
            Take,
            Skip,
            Cut,
            TakeFromStart
        }

        public enum RegressionObjectContainer
        {
            Objects,
            TrackChunk,
            TrackChunks,
            MidiFile
        }

        public enum RegressionGridType
        {
            Arbitrary,
            Custom,
            Stepped
        }

        public enum RegressionCompoundState
        {
            BankProgram,
            RegisteredParameter,
            NonRegisteredParameter,
            MpeConfiguration,
            RelativeParameter,
            FourteenBitController
        }

        private sealed class RegressionGrid : IGrid
        {
            private readonly IEnumerable<long> _times;

            public RegressionGrid(IEnumerable<long> times)
            {
                _times = times;
            }

            public IEnumerable<long> GetTimes(TempoMap tempoMap) => _times;
        }

        private sealed class SinglePassSequence<T> : IEnumerable<T>
        {
            private readonly IEnumerable<T> _items;
            private bool _enumerated;

            public SinglePassSequence(IEnumerable<T> items)
            {
                _items = items;
            }

            public IEnumerator<T> GetEnumerator()
            {
                if (_enumerated)
                    throw new InvalidOperationException("The sequence was enumerated more than once.");

                _enumerated = true;
                return _items.GetEnumerator();
            }

            IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
        }

        #endregion

        #region Test methods

        [Test]
        public void Regression_SplitByChannel_IndependentSharedEvents()
        {
            var midiFile = new[]
            {
                new TimedEvent(new NoteOnEvent((SevenBitNumber)60, (SevenBitNumber)100), 10),
                new TimedEvent(new NoteOnEvent((SevenBitNumber)64, (SevenBitNumber)100)
                {
                    Channel = (FourBitNumber)1
                }, 50),
                new TimedEvent(new TextEvent("shared"), 100),
                new TimedEvent(new SetTempoEvent(600000), 150)
            }.ToFile();
            var originalFile = midiFile.Clone();

            var resultFiles = midiFile.SplitByChannel().ToArray();

            CompareTimedEvents(
                resultFiles[0].GetTimedEvents(),
                new[]
                {
                    new TimedEvent(new NoteOnEvent((SevenBitNumber)60, (SevenBitNumber)100), 10),
                    new TimedEvent(new TextEvent("shared"), 100),
                    new TimedEvent(new SetTempoEvent(600000), 150)
                },
                "Constructing later outputs changed the first output's event times.");
            MidiAsserts.AreEqual(originalFile, midiFile, false, "Splitting changed the source file.");

            var firstTextEvent = resultFiles[0].GetTrackChunks().Single().Events.OfType<TextEvent>().Single();
            var secondTextEvent = resultFiles[1].GetTrackChunks().Single().Events.OfType<TextEvent>().Single();
            firstTextEvent.Text = "changed";

            ClassicAssert.AreNotSame(firstTextEvent, secondTextEvent, "Outputs share a mutable event.");
            ClassicAssert.AreEqual("shared", secondTextEvent.Text, "Editing one output changed another output.");
        }

        [Test]
        public void Regression_CutPart_SyntheticEventsOnly(
            [Values] NoteStartDetectionPolicy detectionPolicy,
            [Values] bool preserveTimes)
        {
            var midiFile = new[]
            {
                new TimedEvent(new NoteOnEvent((SevenBitNumber)60, (SevenBitNumber)90), 50),
                new TimedEvent(new NoteOnEvent((SevenBitNumber)60, (SevenBitNumber)80), 90),
                new TimedEvent(new NoteOffEvent((SevenBitNumber)60, (SevenBitNumber)0), 100),
                new TimedEvent(new NoteOnEvent((SevenBitNumber)60, (SevenBitNumber)100), 200),
                new TimedEvent(new NoteOffEvent((SevenBitNumber)60, (SevenBitNumber)0), 250),
                new TimedEvent(new NoteOffEvent((SevenBitNumber)60, (SevenBitNumber)0), 450)
            }.ToFile();
            var originalFile = midiFile.Clone();
            var settings = new SliceMidiFileSettings
            {
                PreserveTimes = preserveTimes,
                NoteDetectionSettings = new NoteDetectionSettings
                {
                    NoteStartDetectionPolicy = detectionPolicy
                }
            };

            var result = midiFile.CutPart((MidiTimeSpan)100, (MidiTimeSpan)100, settings);

            var endShift = preserveTimes ? 0 : 100;
            CompareTimedEvents(
                result.GetTimedEvents(),
                new[]
                {
                    new TimedEvent(new NoteOnEvent((SevenBitNumber)60, (SevenBitNumber)90), 50),
                    new TimedEvent(new NoteOnEvent((SevenBitNumber)60, (SevenBitNumber)80), 90),
                    new TimedEvent(new NoteOffEvent((SevenBitNumber)60, (SevenBitNumber)0), 100),
                    new TimedEvent(new NoteOnEvent((SevenBitNumber)60, (SevenBitNumber)100), 200 - endShift),
                    new TimedEvent(new NoteOffEvent((SevenBitNumber)60, (SevenBitNumber)0), 250 - endShift),
                    new TimedEvent(new NoteOffEvent((SevenBitNumber)60, (SevenBitNumber)0), 450 - endShift)
                },
                "Cutting removed an original note event sharing the synthetic event's pitch and velocity.");
            MidiAsserts.AreEqual(originalFile, midiFile, false, "Cutting changed the source file.");
        }

        [Test]
        public void Regression_Slice_ZeroLengthNoteOnBoundary(
            [Values] RegressionSliceOperation operation,
            [Values] bool splitNotes,
            [Values] bool preserveTimes)
        {
            var midiFile = new ITimedObject[]
            {
                new Note((SevenBitNumber)60, 0, 100),
                new TimedEvent(new TextEvent("tail"), 300)
            }.ToFile();
            var settings = new SliceMidiFileSettings
            {
                SplitNotes = splitNotes,
                PreserveTimes = preserveTimes
            };

            var result = SliceRegressionFile(midiFile, operation, settings);

            if (operation == RegressionSliceOperation.Cut || operation == RegressionSliceOperation.TakeFromStart)
            {
                ClassicAssert.IsFalse(result.GetTimedEvents().Any(e => e.Event is NoteEvent),
                    "A zero-length note was split across the removed or excluded boundary.");
            }
            else
            {
                MidiAsserts.AreEqual(
                    new[] { new Note((SevenBitNumber)60, 0, preserveTimes ? 100 : 0) },
                    result.GetNotes(),
                    true,
                    0,
                    "A zero-length note was separated from its ending event.");
            }
        }

        [Test]
        public void Regression_Slice_ZeroVelocityNoteOffOnBoundary(
            [Values] RegressionSliceOperation operation,
            [Values] bool splitNotes,
            [Values] bool preserveTimes)
        {
            var midiFile = new[]
            {
                new TimedEvent(new NoteOnEvent((SevenBitNumber)60, (SevenBitNumber)100), 50),
                new TimedEvent(new NoteOnEvent((SevenBitNumber)60, (SevenBitNumber)0), 100),
                new TimedEvent(new TextEvent("tail"), 300)
            }.ToFile();
            var settings = new SliceMidiFileSettings
            {
                SplitNotes = splitNotes,
                PreserveTimes = preserveTimes
            };

            var result = SliceRegressionFile(midiFile, operation, settings);

            var expectedEvents = operation == RegressionSliceOperation.Cut || operation == RegressionSliceOperation.TakeFromStart
                ? new[]
                {
                    new TimedEvent(new NoteOnEvent((SevenBitNumber)60, (SevenBitNumber)100), 50),
                    new TimedEvent(new NoteOnEvent((SevenBitNumber)60, (SevenBitNumber)0), 100)
                }
                : Array.Empty<TimedEvent>();
            CompareTimedEvents(
                result.GetTimedEvents().Where(e => e.Event is NoteEvent),
                expectedEvents,
                "A zero-velocity Note On was not treated as a boundary Note Off.");
        }

        [Test]
        public void Regression_Slice_ContextAcrossTracks(
            [Values(RegressionSliceOperation.Grid, RegressionSliceOperation.Take, RegressionSliceOperation.Skip, RegressionSliceOperation.Cut)]
            RegressionSliceOperation operation,
            [Values] bool preserveTimes,
            [Values] bool updateAtBoundary)
        {
            var updateTime = updateAtBoundary ? 100 : 50;
            var midiFile = new MidiFile(
                new[]
                {
                    new TimedEvent(new SetTempoEvent(600000), updateTime),
                    RegressionControlChange(ControlName.ChannelVolume, 100, updateTime)
                }.ToTrackChunk(),
                new ITimedObject[]
                {
                    new TimedEvent(new SetTempoEvent(300000), 0),
                    RegressionControlChange(ControlName.ChannelVolume, 10, 0),
                    new Note((SevenBitNumber)60, 10, 250)
                }.ToTrackChunk());
            var settings = new SliceMidiFileSettings
            {
                PreserveTimes = preserveTimes,
                PreserveTrackChunks = true
            };

            var result = SliceRegressionFile(midiFile, operation, settings);

            var checkTime = operation == RegressionSliceOperation.Cut || preserveTimes ? 210 : 10;
            ClassicAssert.AreEqual(
                new Tempo(600000),
                result.GetTempoMap().GetTempoAtTime((MidiTimeSpan)checkTime),
                "Track order replaced the latest tempo with an older tempo.");
            var volume = result.GetTimedEvents()
                .Where(e => e.Time <= checkTime)
                .Select(e => e.Event)
                .OfType<ControlChangeEvent>()
                .Last(e => e.ControlNumber == (SevenBitNumber)(byte)ControlName.ChannelVolume);
            ClassicAssert.AreEqual((SevenBitNumber)100, volume.ControlValue,
                "Track order replaced the latest channel state with an older value.");
        }

        [Test]
        public void Regression_Slice_PortScopedContext(
            [Values(RegressionSliceOperation.Grid, RegressionSliceOperation.Take, RegressionSliceOperation.Skip, RegressionSliceOperation.Cut)]
            RegressionSliceOperation operation)
        {
            var midiFile = new MidiFile(
                new[]
                {
                    new TimedEvent(new PortPrefixEvent((SevenBitNumber)1), 0),
                    RegressionControlChange(ControlName.ChannelVolume, 100, 50)
                }.ToTrackChunk(),
                new[]
                {
                    new TimedEvent(new PortPrefixEvent((SevenBitNumber)2), 0),
                    RegressionControlChange(ControlName.ChannelVolume, 10, 0),
                    new TimedEvent(new TextEvent("tail"), 250)
                }.ToTrackChunk());

            var result = SliceRegressionFile(
                midiFile,
                operation,
                new SliceMidiFileSettings { PreserveTrackChunks = true });

            var trackChunks = result.GetTrackChunks().ToArray();
            ClassicAssert.AreEqual(2, trackChunks.Length, "The source track layout was lost.");
            ClassicAssert.AreEqual((SevenBitNumber)100,
                trackChunks[0].Events.OfType<ControlChangeEvent>().Last().ControlValue,
                "Context from another MIDI port replaced the first port's state.");
            ClassicAssert.AreEqual((SevenBitNumber)10,
                trackChunks[1].Events.OfType<ControlChangeEvent>().Last().ControlValue,
                "Context from another MIDI port replaced the second port's state.");
        }

        [Test]
        public void Regression_Slice_CompoundStateHistory(
            [Values(RegressionSliceOperation.Grid, RegressionSliceOperation.Take, RegressionSliceOperation.Skip, RegressionSliceOperation.Cut)]
            RegressionSliceOperation operation,
            [Values] RegressionCompoundState state,
            [Values] bool preserveTimes)
        {
            var history = GetRegressionCompoundHistory(state);
            var midiFile = history.Concat(new[]
            {
                new TimedEvent(new TextEvent("tail"), 300)
            }).ToFile();
            var settings = new SliceMidiFileSettings { PreserveTimes = preserveTimes };

            var result = SliceRegressionFile(midiFile, operation, settings);

            var expectedHistory = history.Select(e => new TimedEvent(
                e.Event.Clone(),
                operation == RegressionSliceOperation.Cut ? e.Time : preserveTimes ? 100 : 0));
            CompareTimedEvents(
                result.GetTimedEvents().Where(e => e.Event is ControlChangeEvent || e.Event is ProgramChangeEvent),
                expectedHistory,
                "Compound state lost its transaction order, parameter assignments, or relative messages.");
        }

        [Test]
        public void Regression_CutPart_CompoundHistoryDoesNotRepeatPrefix()
        {
            var history = GetRegressionCompoundHistory(RegressionCompoundState.RelativeParameter);
            var midiFile = history.Concat(new[]
            {
                RegressionControlChange(ControlName.DataIncrement, 0, 150),
                new TimedEvent(new TextEvent("tail"), 300)
            }).ToFile();

            var result = midiFile.CutPart((MidiTimeSpan)100, (MidiTimeSpan)100);

            ClassicAssert.AreEqual(3,
                result.GetTimedEvents().Count(e => e.Event is ControlChangeEvent controlChange &&
                    controlChange.ControlNumber == (SevenBitNumber)(byte)ControlName.DataIncrement),
                "Cutting replayed increments already present in the retained prefix.");
        }

        [Test]
        public void Regression_Slice_CompoundHistoryAcrossTracks(
            [Values(RegressionSliceOperation.Grid, RegressionSliceOperation.Take, RegressionSliceOperation.Skip, RegressionSliceOperation.Cut)]
            RegressionSliceOperation operation)
        {
            var history = GetRegressionCompoundHistory(RegressionCompoundState.RegisteredParameter);
            var midiFile = new MidiFile(
                history.Where((e, index) => index % 2 == 1).ToTrackChunk(),
                history.Where((e, index) => index % 2 == 0)
                    .Concat(new[] { new TimedEvent(new TextEvent("tail"), 300) }).ToTrackChunk());

            var result = SliceRegressionFile(midiFile, operation, new SliceMidiFileSettings());

            CompareTimedEvents(
                result.GetTimedEvents().Where(e => e.Event is ControlChangeEvent),
                history.Select(e => new TimedEvent(
                    e.Event.Clone(),
                    operation == RegressionSliceOperation.Cut ? e.Time : 0)),
                "Track order changed the order of a compound-state transaction.");
        }

        [Test]
        public void Regression_SplitGrid_InvalidTimes(
            [Values] RegressionObjectContainer container,
            [Values] RegressionGridType gridType,
            [Values] bool decreasing)
        {
            var times = decreasing ? new long[] { 20, 10, 40 } : new long[] { 20, 20, 40 };
            IGrid grid = gridType == RegressionGridType.Arbitrary
                ? new ArbitraryGrid(times.Select(t => (ITimeSpan)(MidiTimeSpan)t))
                : gridType == RegressionGridType.Custom
                    ? new RegressionGrid(times)
                    : decreasing
                        ? new SteppedGrid((MidiTimeSpan)(long.MaxValue - 10), (MidiTimeSpan)20)
                        : new SteppedGrid(new ITimeSpan[] { (MidiTimeSpan)20, (MidiTimeSpan)0 });
            var note = gridType == RegressionGridType.Stepped && decreasing
                ? new Note((SevenBitNumber)60, 2, long.MaxValue - 5)
                : new Note((SevenBitNumber)60, 100);

            ClassicAssert.Throws<InvalidOperationException>(
                () => SplitRegressionObjects(new ITimedObject[] { note }, container, grid),
                "Splitting accepted duplicate or decreasing grid times.");
        }

        [Test]
        public void Regression_SplitByGrid_InvalidTimes(
            [Values] RegressionGridType gridType,
            [Values] bool splitNotes,
            [Values] bool decreasing)
        {
            var times = decreasing ? new long[] { 20, 10, 40 } : new long[] { 20, 20, 40 };
            IGrid grid = gridType == RegressionGridType.Arbitrary
                ? new ArbitraryGrid(times.Select(t => (ITimeSpan)(MidiTimeSpan)t))
                : gridType == RegressionGridType.Custom
                    ? new RegressionGrid(times)
                    : decreasing
                        ? new SteppedGrid((MidiTimeSpan)(long.MaxValue - 10), (MidiTimeSpan)20)
                        : new SteppedGrid(new ITimeSpan[] { (MidiTimeSpan)20, (MidiTimeSpan)0 });
            var note = gridType == RegressionGridType.Stepped && decreasing
                ? new Note((SevenBitNumber)60, 2, long.MaxValue - 5)
                : new Note((SevenBitNumber)60, 100);
            var midiFile = new ITimedObject[] { note }.ToFile();

            ClassicAssert.Throws<InvalidOperationException>(
                () => midiFile.SplitByGrid(grid, new SliceMidiFileSettings { SplitNotes = splitNotes }).ToArray(),
                "File splitting accepted duplicate or decreasing grid times.");
        }

        [Test]
        public void Regression_Slice_DuplicateCalculatedBoundaries(
            [Values] bool cutPart,
            [Values] bool splitNotes)
        {
            var midiFile = new ITimedObject[] { new Note((SevenBitNumber)60, 200) }.ToFile();
            var settings = new SliceMidiFileSettings { SplitNotes = splitNotes };

            ClassicAssert.Throws<InvalidOperationException>(() =>
            {
                if (cutPart)
                    midiFile.CutPart((MidiTimeSpan)100, (MidiTimeSpan)0, settings);
                else
                    midiFile.TakePart((MidiTimeSpan)100, (MidiTimeSpan)0, settings);
            }, "A zero-length range produced duplicate slicing boundaries without an exception.");
        }

        [Test]
        [Category("NonAdvancingGrid")]
        public void Regression_SplitGrid_ZeroConvertedStep(
            [Values] RegressionObjectContainer container,
            [Values] bool metricStep)
        {
            var step = metricStep ? (ITimeSpan)new MetricTimeSpan(1) : (MidiTimeSpan)0;

            ClassicAssert.Throws<InvalidOperationException>(
                () => SplitRegressionObjects(
                    new ITimedObject[] { new Note((SevenBitNumber)60, 100) },
                    container,
                    new SteppedGrid(step)),
                "A grid whose converted step is zero did not report non-advancement.");
        }

        [Test]
        [Category("NonAdvancingGrid")]
        public void Regression_SplitByGrid_ZeroConvertedStep(
            [Values] bool splitNotes,
            [Values] bool metricStep)
        {
            var step = metricStep ? (ITimeSpan)new MetricTimeSpan(1) : (MidiTimeSpan)0;
            var midiFile = new ITimedObject[] { new Note((SevenBitNumber)60, 100) }.ToFile();

            ClassicAssert.Throws<InvalidOperationException>(
                () => midiFile.SplitByGrid(new SteppedGrid(step),
                    new SliceMidiFileSettings { SplitNotes = splitNotes }).First(),
                "File splitting hung on a zero converted step.");
        }

        [Test]
        public void Regression_SplitObjectsByGrid_SinglePassAndInputOrder()
        {
            var objects = new SinglePassSequence<ITimedObject>(new ITimedObject[]
            {
                new Note((SevenBitNumber)60, 100, 100),
                new Note((SevenBitNumber)64, 100, 0)
            });
            var times = new SinglePassSequence<long>(new long[] { 50, 150, 250 });

            var result = objects.SplitObjectsByGrid(new RegressionGrid(times), TempoMap.Default).ToArray();

            MidiAsserts.AreEqual(
                new ITimedObject[]
                {
                    new Note((SevenBitNumber)60, 50, 100),
                    new Note((SevenBitNumber)60, 50, 150),
                    new Note((SevenBitNumber)64, 50, 0),
                    new Note((SevenBitNumber)64, 50, 50)
                },
                result,
                true,
                0,
                "Single-pass splitting lost objects or changed the input-object order.");
        }

        [Test]
        public void Regression_SplitObjectsByGrid_YieldsBeforeReadingNextObject()
        {
            var objects = EnumerateRegressionObjectsLazily();

            var result = objects.SplitObjectsByGrid(
                new SteppedGrid((MidiTimeSpan)50),
                TempoMap.Default).First();

            MidiAsserts.AreEqual(
                new ITimedObject[] { new Note((SevenBitNumber)60, 50) },
                new[] { result },
                true,
                0,
                "Splitting read later input objects before yielding the first fragment.");
        }

        [Test]
        public void Regression_SplitObjectsByGrid_YieldsWhileReadingGrid()
        {
            var objects = new ITimedObject[] { new Note((SevenBitNumber)60, 100) };

            var result = objects.SplitObjectsByGrid(
                new RegressionGrid(EnumerateRegressionGridLazily()),
                TempoMap.Default).First();

            MidiAsserts.AreEqual(
                new ITimedObject[] { new Note((SevenBitNumber)60, 50) },
                new[] { result },
                true,
                0,
                "Splitting read the entire grid before yielding the first fragment.");
        }

        [Test]
        public void Regression_Slice_FinalEndMarker(
            [Values] bool skipPart,
            [Values] bool preserveTimes)
        {
            var midiFile = new[] { new TimedEvent(new TextEvent("tail"), 200) }.ToFile();
            var settings = new SliceMidiFileSettings
            {
                PreserveTimes = preserveTimes,
                Markers = new SliceMidiFileMarkers
                {
                    PartEndMarkerEventFactory = () => new MarkerEvent("end")
                }
            };

            var result = skipPart
                ? midiFile.SkipPart((MidiTimeSpan)100, settings)
                : midiFile.SplitByGrid(new ArbitraryGrid((MidiTimeSpan)100), settings).Last();

            ClassicAssert.AreEqual(preserveTimes ? 200 : 100,
                result.GetTimedEvents().Single(e => e.Event is MarkerEvent).Time,
                "The final marker used the event-consumption sentinel instead of the file end.");
        }

        [Test]
        public void Regression_Slice_FinalEndMarker_EmptyTail(
            [Values] bool preserveTimes)
        {
            var midiFile = new[] { new TimedEvent(new TextEvent("prefix"), 50) }.ToFile();
            var settings = new SliceMidiFileSettings
            {
                PreserveTimes = preserveTimes,
                Markers = new SliceMidiFileMarkers
                {
                    PartEndMarkerEventFactory = () => new MarkerEvent("end")
                }
            };

            var result = midiFile.SkipPart((MidiTimeSpan)100, settings);

            ClassicAssert.AreEqual(preserveTimes ? 100 : 0,
                result.GetTimedEvents().Single(e => e.Event is MarkerEvent).Time,
                "An empty tail's marker was not placed at its start.");
        }

        [Test]
        public void Regression_Slice_ResetControllersHistory(
            [Values(RegressionSliceOperation.Grid, RegressionSliceOperation.Take, RegressionSliceOperation.Skip, RegressionSliceOperation.Cut)]
            RegressionSliceOperation operation)
        {
            var history = new[]
            {
                RegressionControlChange(ControlName.DamperPedal, 127, 0),
                new TimedEvent(new PitchBendEvent((ushort)10000), 1),
                RegressionControlChange(ControlName.ResetAllControllers, 0, 2),
                RegressionControlChange(ControlName.ChannelVolume, 90, 3)
            };
            var midiFile = history.Concat(new[] { new TimedEvent(new TextEvent("tail"), 300) }).ToFile();

            var result = SliceRegressionFile(midiFile, operation, new SliceMidiFileSettings());

            CompareTimedEvents(
                result.GetTimedEvents().Where(e => e.Event is ChannelEvent),
                history.Select(e => new TimedEvent(e.Event.Clone(),
                    operation == RegressionSliceOperation.Cut ? e.Time : 0)),
                "Controller reset was reordered relative to the state it resets.");
        }

        [Test]
        public void Regression_Slice_PortChangesDuringHistory(
            [Values(RegressionSliceOperation.Grid, RegressionSliceOperation.Take, RegressionSliceOperation.Skip, RegressionSliceOperation.Cut)]
            RegressionSliceOperation operation)
        {
            var history = new[]
            {
                new TimedEvent(new PortPrefixEvent((SevenBitNumber)1), 0),
                RegressionControlChange(ControlName.RegisteredParameterNumberMsb, 0, 1),
                RegressionControlChange(ControlName.RegisteredParameterNumberLsb, 6, 2),
                RegressionControlChange(ControlName.DataEntryMsb, 2, 3),
                new TimedEvent(new PortPrefixEvent((SevenBitNumber)2), 4),
                RegressionControlChange(ControlName.RegisteredParameterNumberMsb, 0, 5),
                RegressionControlChange(ControlName.RegisteredParameterNumberLsb, 6, 6),
                RegressionControlChange(ControlName.DataEntryMsb, 4, 7)
            };
            var midiFile = history.Concat(new[] { new TimedEvent(new TextEvent("tail"), 300) }).ToFile();

            var result = SliceRegressionFile(midiFile, operation, new SliceMidiFileSettings());

            CompareTimedEvents(
                result.GetTimedEvents().Where(e => e.Event is ChannelEvent || e.Event is PortPrefixEvent),
                history.Select(e => new TimedEvent(e.Event.Clone(),
                    operation == RegressionSliceOperation.Cut ? e.Time : 0))
                    .Concat(operation == RegressionSliceOperation.Cut
                        ? new[] { new TimedEvent(new PortPrefixEvent((SevenBitNumber)2), 100) }
                        : Array.Empty<TimedEvent>()),
                "Compound-state replay lost a port change or applied data to the wrong port.");
        }

        [Test]
        public void Regression_Slice_MpeZoneConfiguration(
            [Values(RegressionSliceOperation.Grid, RegressionSliceOperation.Take, RegressionSliceOperation.Skip, RegressionSliceOperation.Cut)]
            RegressionSliceOperation operation,
            [Values(0, 15)] int masterChannel)
        {
            var history = new[]
            {
                RegressionControlChange(ControlName.RegisteredParameterNumberMsb, 0, 0),
                RegressionControlChange(ControlName.RegisteredParameterNumberLsb, 6, 1),
                RegressionControlChange(ControlName.DataEntryMsb, 4, 2),
                RegressionControlChange(ControlName.DataEntryMsb, 0, 3),
                RegressionControlChange(ControlName.DataEntryMsb, 2, 4),
                RegressionControlChange(ControlName.RegisteredParameterNumberMsb, 127, 5),
                RegressionControlChange(ControlName.RegisteredParameterNumberLsb, 127, 6)
            };

            foreach (var timedEvent in history)
            {
                ((ControlChangeEvent)timedEvent.Event).Channel = (FourBitNumber)masterChannel;
            }

            var midiFile = history.Concat(new[] { new TimedEvent(new TextEvent("tail"), 300) }).ToFile();

            var result = SliceRegressionFile(midiFile, operation, new SliceMidiFileSettings());

            CompareTimedEvents(
                result.GetTimedEvents().Where(e => e.Event is ControlChangeEvent),
                history.Select(e => new TimedEvent(e.Event.Clone(),
                    operation == RegressionSliceOperation.Cut ? e.Time : 0)),
                "MPE zone configuration, disabling, or reconfiguration was lost.");
        }

        [Test]
        public void Regression_CutPart_IgnoresUserMarkers()
        {
            var midiFile = new ITimedObject[] { new Note((SevenBitNumber)60, 400) }.ToFile();
            var settings = new SliceMidiFileSettings
            {
                Markers = new SliceMidiFileMarkers
                {
                    PartStartMarkerEventFactory = () => throw new AssertionException("CutPart called the start factory."),
                    PartEndMarkerEventFactory = () => throw new AssertionException("CutPart called the end factory."),
                    EmptyPartMarkerEventFactory = () => throw new AssertionException("CutPart called the empty factory.")
                }
            };

            var result = midiFile.CutPart((MidiTimeSpan)100, (MidiTimeSpan)100, settings);

            MidiAsserts.AreEqual(
                new ITimedObject[] { new Note((SevenBitNumber)60, 300) }.ToFile(),
                result,
                false,
                "Ignoring user markers changed cutting behavior.");
        }

        [Test]
        public void Regression_SplitObjectsAtDistance_NonFiniteRatio(
            [Values] RegressionObjectContainer container,
            [Values(double.NaN, double.PositiveInfinity, double.NegativeInfinity)] double ratio,
            [Values] TimeSpanType lengthType,
            [Values] LengthedObjectTarget from)
        {
            var trackChunk = new ITimedObject[] { new Note((SevenBitNumber)60, 100) }.ToTrackChunk();

            var exception = ClassicAssert.Throws<ArgumentOutOfRangeException>(() =>
            {
                switch (container)
                {
                    case RegressionObjectContainer.Objects:
                        trackChunk.GetNotes().SplitObjectsAtDistance(ratio, lengthType, from, TempoMap.Default).ToArray();
                        break;
                    case RegressionObjectContainer.TrackChunk:
                        trackChunk.SplitObjectsAtDistance(ObjectType.Note, ratio, lengthType, from, TempoMap.Default);
                        break;
                    case RegressionObjectContainer.TrackChunks:
                        new[] { trackChunk }.SplitObjectsAtDistance(ObjectType.Note, ratio, lengthType, from, TempoMap.Default);
                        break;
                    case RegressionObjectContainer.MidiFile:
                        new MidiFile(trackChunk).SplitObjectsAtDistance(ObjectType.Note, ratio, lengthType, from);
                        break;
                }
            }, "A non-finite ratio was not rejected at the public API boundary.");

            ClassicAssert.AreEqual("ratio", exception.ParamName, "The exception names an internal parameter.");
        }

        #endregion

        #region Private methods

        private static MidiFile SliceRegressionFile(
            MidiFile midiFile,
            RegressionSliceOperation operation,
            SliceMidiFileSettings settings)
        {
            switch (operation)
            {
                case RegressionSliceOperation.Grid:
                    return midiFile.SplitByGrid(new SteppedGrid((MidiTimeSpan)100), settings).Skip(1).First();
                case RegressionSliceOperation.Take:
                    return midiFile.TakePart((MidiTimeSpan)100, (MidiTimeSpan)100, settings);
                case RegressionSliceOperation.Skip:
                    return midiFile.SkipPart((MidiTimeSpan)100, settings);
                case RegressionSliceOperation.Cut:
                    return midiFile.CutPart((MidiTimeSpan)100, (MidiTimeSpan)100, settings);
                case RegressionSliceOperation.TakeFromStart:
                    return midiFile.TakePart((MidiTimeSpan)100, settings);
                default:
                    throw new ArgumentOutOfRangeException(nameof(operation));
            }
        }

        private static void SplitRegressionObjects(
            IEnumerable<ITimedObject> objects,
            RegressionObjectContainer container,
            IGrid grid)
        {
            if (container == RegressionObjectContainer.Objects)
            {
                objects.SplitObjectsByGrid(grid, TempoMap.Default).ToArray();
                return;
            }

            var trackChunk = objects.ToTrackChunk();

            switch (container)
            {
                case RegressionObjectContainer.TrackChunk:
                    trackChunk.SplitObjectsByGrid(ObjectType.Note, grid, TempoMap.Default);
                    break;
                case RegressionObjectContainer.TrackChunks:
                    new[] { trackChunk }.SplitObjectsByGrid(ObjectType.Note, grid, TempoMap.Default);
                    break;
                case RegressionObjectContainer.MidiFile:
                    new MidiFile(trackChunk).SplitObjectsByGrid(ObjectType.Note, grid);
                    break;
            }
        }

        private static IEnumerable<ITimedObject> EnumerateRegressionObjectsLazily()
        {
            yield return new Note((SevenBitNumber)60, 100);
            throw new InvalidOperationException("Splitting read the next object too early.");
        }

        private static IEnumerable<long> EnumerateRegressionGridLazily()
        {
            yield return 50;
            throw new InvalidOperationException("Splitting read the next grid point too early.");
        }

        private static TimedEvent RegressionControlChange(ControlName controlName, int value, long time)
        {
            return new TimedEvent(
                new ControlChangeEvent((SevenBitNumber)(byte)controlName, (SevenBitNumber)value),
                time);
        }

        private static TimedEvent[] GetRegressionCompoundHistory(RegressionCompoundState state)
        {
            if (state == RegressionCompoundState.BankProgram)
            {
                return new[]
                {
                    RegressionControlChange(ControlName.BankSelect, 1, 0),
                    RegressionControlChange(ControlName.LsbForBankSelect, 2, 1),
                    new TimedEvent(new ProgramChangeEvent((SevenBitNumber)10), 2),
                    RegressionControlChange(ControlName.BankSelect, 3, 3)
                };
            }

            if (state == RegressionCompoundState.FourteenBitController)
            {
                return new[]
                {
                    RegressionControlChange(ControlName.Modulation, 1, 0),
                    RegressionControlChange(ControlName.LsbForModulation, 2, 1),
                    RegressionControlChange(ControlName.Modulation, 3, 2),
                    RegressionControlChange(ControlName.LsbForModulation, 4, 3)
                };
            }

            var parameterMsb = state == RegressionCompoundState.NonRegisteredParameter
                ? ControlName.NonRegisteredParameterNumberMsb
                : ControlName.RegisteredParameterNumberMsb;
            var parameterLsb = state == RegressionCompoundState.NonRegisteredParameter
                ? ControlName.NonRegisteredParameterNumberLsb
                : ControlName.RegisteredParameterNumberLsb;
            var parameterNumber = state == RegressionCompoundState.MpeConfiguration ? 6 : 0;
            var history = new List<TimedEvent>
            {
                RegressionControlChange(parameterMsb, 0, 0),
                RegressionControlChange(parameterLsb, parameterNumber, 1)
            };

            if (state != RegressionCompoundState.RelativeParameter)
            {
                history.Add(RegressionControlChange(ControlName.DataEntryMsb, 2, 2));
                history.Add(RegressionControlChange(ControlName.LsbForDataEntry, 3, 3));
            }

            history.Add(RegressionControlChange(ControlName.DataIncrement, 0, 4));
            history.Add(RegressionControlChange(ControlName.DataIncrement, 0, 5));
            history.Add(RegressionControlChange(ControlName.DataDecrement, 0, 6));

            if (state != RegressionCompoundState.RelativeParameter)
            {
                history.Add(RegressionControlChange(parameterLsb, parameterNumber + 1, 7));
                history.Add(RegressionControlChange(ControlName.DataEntryMsb, 64, 8));
                history.Add(RegressionControlChange(parameterMsb, 127, 9));
                history.Add(RegressionControlChange(parameterLsb, 127, 10));
            }

            return history.ToArray();
        }

        #endregion
    }
}
