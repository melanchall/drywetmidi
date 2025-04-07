using System.Linq;
using Melanchall.DryWetMidi.Core;
using Melanchall.DryWetMidi.Interaction;
using NUnit.Framework;
using NUnit.Framework.Legacy;

namespace Melanchall.DryWetMidi.Tests.Interaction
{
    [TestFixture]
    public class TempoMapTests
    {
        #region Test methods

        [Test]
        public void Default()
        {
            TestSimpleTempoMap(TempoMap.Default,
                               new TicksPerQuarterNoteTimeDivision(),
                               Tempo.Default,
                               TimeSignature.Default);
        }

        [Test]
        public void Create_Tempo_TimeSignature()
        {
            var expectedTempo = Tempo.FromBeatsPerMinute(123);
            var expectedTimeSignature = new TimeSignature(3, 8);

            TestSimpleTempoMap(TempoMap.Create(expectedTempo, expectedTimeSignature),
                               new TicksPerQuarterNoteTimeDivision(),
                               expectedTempo,
                               expectedTimeSignature);
        }

        [Test]
        public void Create_Tempo()
        {
            var expectedTempo = new Tempo(123456);

            TestSimpleTempoMap(TempoMap.Create(expectedTempo),
                               new TicksPerQuarterNoteTimeDivision(),
                               expectedTempo,
                               TimeSignature.Default);
        }

        [Test]
        public void Create_TimeSignature()
        {
            var expectedTimeSignature = new TimeSignature(3, 8);

            TestSimpleTempoMap(TempoMap.Create(expectedTimeSignature),
                               new TicksPerQuarterNoteTimeDivision(),
                               Tempo.Default,
                               expectedTimeSignature);
        }

        [Test]
        public void Create_TimeDivision_Tempo_TimeSignature()
        {
            var expectedTimeDivision = new TicksPerQuarterNoteTimeDivision(10000);
            var expectedTempo = Tempo.FromBeatsPerMinute(123);
            var expectedTimeSignature = new TimeSignature(3, 8);

            TestSimpleTempoMap(TempoMap.Create(expectedTimeDivision, expectedTempo, expectedTimeSignature),
                               expectedTimeDivision,
                               expectedTempo,
                               expectedTimeSignature);
        }

        [Test]
        public void Create_TimeDivision_Tempo()
        {
            var expectedTimeDivision = new TicksPerQuarterNoteTimeDivision(10000);
            var expectedTempo = new Tempo(123456);

            TestSimpleTempoMap(TempoMap.Create(expectedTimeDivision, expectedTempo),
                               expectedTimeDivision,
                               expectedTempo,
                               TimeSignature.Default);
        }

        [Test]
        public void Create_TimeDivision_TimeSignature()
        {
            var expectedTimeDivision = new TicksPerQuarterNoteTimeDivision(10000);
            var expectedTimeSignature = new TimeSignature(3, 8);

            TestSimpleTempoMap(TempoMap.Create(expectedTimeDivision, expectedTimeSignature),
                               expectedTimeDivision,
                               Tempo.Default,
                               expectedTimeSignature);
        }

        [Test]
        public void GetTempoChanges_NoChanges()
        {
            var tempoMap = TempoMap.Default;
            CollectionAssert.IsEmpty(tempoMap.GetTempoChanges(), "There are tempo changes.");
        }

        [Test]
        public void GetTempoChanges_SingleChange_AtStart()
        {
            var microsecondsPerQuarterNote = 100000;

            var tempoMap = TempoMap.Create(new Tempo(microsecondsPerQuarterNote));
            var changes = tempoMap.GetTempoChanges();
            ClassicAssert.AreEqual(1, changes.Count(), "Count of tempo changes is invalid.");

            var change = changes.First();
            ClassicAssert.AreEqual(0, change.Time, "Time of change is invalid.");
            ClassicAssert.AreEqual(new Tempo(microsecondsPerQuarterNote), change.Value, "Tempo of change is invalid.");
        }

        [Test]
        public void GetTempoChanges_SingleChange_AtMiddle()
        {
            var microsecondsPerQuarterNote = 100000;
            var time = 1000;

            using (var tempoMapManager = new TempoMapManager())
            {
                tempoMapManager.SetTempo(time, new Tempo(microsecondsPerQuarterNote));

                var tempoMap = tempoMapManager.TempoMap;
                var changes = tempoMap.GetTempoChanges();
                ClassicAssert.AreEqual(1, changes.Count(), "Count of tempo changes is invalid.");

                var change = changes.First();
                ClassicAssert.AreEqual(time, change.Time, "Time of change is invalid.");
                ClassicAssert.AreEqual(new Tempo(microsecondsPerQuarterNote), change.Value, "Tempo of change is invalid.");
            }
        }

        [Test]
        public void GetTempoChanges_MultipleChanges()
        {
            var microsecondsPerQuarterNote1 = 100000;
            var time1 = 1000;

            var microsecondsPerQuarterNote2 = 700000;
            var time2 = 1500;

            using (var tempoMapManager = new TempoMapManager())
            {
                tempoMapManager.SetTempo(time2, new Tempo(microsecondsPerQuarterNote2));
                tempoMapManager.SetTempo(time1, new Tempo(microsecondsPerQuarterNote1));

                var tempoMap = tempoMapManager.TempoMap;
                var changes = tempoMap.GetTempoChanges();
                ClassicAssert.AreEqual(2, changes.Count(), "Count of tempo changes is invalid.");

                var change1 = changes.First();
                ClassicAssert.AreEqual(time1, change1.Time, "Time of first change is invalid.");
                ClassicAssert.AreEqual(new Tempo(microsecondsPerQuarterNote1), change1.Value, "Tempo of first change is invalid.");

                var change2 = changes.Last();
                ClassicAssert.AreEqual(time2, change2.Time, "Time of second change is invalid.");
                ClassicAssert.AreEqual(new Tempo(microsecondsPerQuarterNote2), change2.Value, "Tempo of second change is invalid.");
            }
        }

        [Test]
        public void GetTimeSignatureChanges_NoChanges()
        {
            var tempoMap = TempoMap.Default;
            CollectionAssert.IsEmpty(tempoMap.GetTimeSignatureChanges(), "There are time signature changes.");
        }

        [Test]
        public void GetTimeSignatureChanges_SingleChange_AtStart()
        {
            var numerator = 2;
            var denominator = 16;

            var tempoMap = TempoMap.Create(new TimeSignature(numerator, denominator));
            var changes = tempoMap.GetTimeSignatureChanges();
            ClassicAssert.AreEqual(1, changes.Count(), "Count of time signature changes is invalid.");

            var change = changes.First();
            ClassicAssert.AreEqual(0, change.Time, "Time of change is invalid.");
            ClassicAssert.AreEqual(new TimeSignature(numerator, denominator), change.Value, "Time signature of change is invalid.");
        }

        [Test]
        public void GetTimeSignatureChanges_SingleChange_AtMiddle()
        {
            var numerator = 2;
            var denominator = 16;
            var time = 1000;

            using (var tempoMapManager = new TempoMapManager())
            {
                tempoMapManager.SetTimeSignature(time, new TimeSignature(numerator, denominator));

                var tempoMap = tempoMapManager.TempoMap;
                var changes = tempoMap.GetTimeSignatureChanges();
                ClassicAssert.AreEqual(1, changes.Count(), "Count of time signature changes is invalid.");

                var change = changes.First();
                ClassicAssert.AreEqual(time, change.Time, "Time of change is invalid.");
                ClassicAssert.AreEqual(new TimeSignature(numerator, denominator), change.Value, "Time signature of change is invalid.");
            }
        }

        [Test]
        public void GetTimeSignatureChanges_MultipleChanges()
        {
            var numerator1 = 2;
            var denominator1 = 16;
            var time1 = 1000;

            var numerator2 = 3;
            var denominator2 = 8;
            var time2 = 1500;

            using (var tempoMapManager = new TempoMapManager())
            {
                tempoMapManager.SetTimeSignature(time2, new TimeSignature(numerator2, denominator2));
                tempoMapManager.SetTimeSignature(time1, new TimeSignature(numerator1, denominator1));

                var tempoMap = tempoMapManager.TempoMap;
                var changes = tempoMap.GetTimeSignatureChanges();
                ClassicAssert.AreEqual(2, changes.Count(), "Count of time signature changes is invalid.");

                var change1 = changes.First();
                ClassicAssert.AreEqual(time1, change1.Time, "Time of first change is invalid.");
                ClassicAssert.AreEqual(new TimeSignature(numerator1, denominator1), change1.Value, "Time signature of first change is invalid.");

                var change2 = changes.Last();
                ClassicAssert.AreEqual(time2, change2.Time, "Time of second change is invalid.");
                ClassicAssert.AreEqual(new TimeSignature(numerator2, denominator2), change2.Value, "Time signature of second change is invalid.");
            }
        }

        [Test]
        public void GetTempoAtTime_DefaultTempoMap_AtStart()
        {
            var tempoMap = TempoMap.Default;
            var tempo = tempoMap.GetTempoAtTime(new MidiTimeSpan(0));
            ClassicAssert.AreEqual(Tempo.Default, tempo, "Tempo is invalid.");
        }

        [Test]
        public void GetTempoAtTime_DefaultTempoMap_AtMiddle()
        {
            var tempoMap = TempoMap.Default;
            var tempo = tempoMap.GetTempoAtTime(MusicalTimeSpan.Quarter);
            ClassicAssert.AreEqual(Tempo.Default, tempo, "Tempo is invalid.");
        }

        [Test]
        public void GetTempoAtTime_NonDefaultTempoMap_SingleChangeAtStart_GetAtStart()
        {
            var microsecondsPerQuarterNote = 100000;

            var tempoMap = TempoMap.Create(new Tempo(microsecondsPerQuarterNote));
            var tempo = tempoMap.GetTempoAtTime(new MidiTimeSpan(0));
            ClassicAssert.AreEqual(new Tempo(microsecondsPerQuarterNote), tempo, "Tempo is invalid.");
        }

        [Test]
        public void GetTempoAtTime_NonDefaultTempoMap_SingleChangeAtStart_GetAtMiddle()
        {
            var microsecondsPerQuarterNote = 100000;

            var tempoMap = TempoMap.Create(new Tempo(microsecondsPerQuarterNote));
            var tempo = tempoMap.GetTempoAtTime(new MetricTimeSpan(0, 0, 1));
            ClassicAssert.AreEqual(new Tempo(microsecondsPerQuarterNote), tempo, "Tempo is invalid.");
        }

        [Test]
        public void GetTempoAtTime_NonDefaultTempoMap_SingleChangeAtMiddle_GetAtStart()
        {
            var microsecondsPerQuarterNote = 100000;

            using (var tempoMapManager = new TempoMapManager())
            {
                tempoMapManager.SetTempo(1000, new Tempo(microsecondsPerQuarterNote));

                var tempoMap = tempoMapManager.TempoMap;
                var tempo = tempoMap.GetTempoAtTime(new MidiTimeSpan(0));
                ClassicAssert.AreEqual(Tempo.Default, tempo, "Tempo is invalid.");
            }
        }

        [Test]
        public void GetTempoAtTime_NonDefaultTempoMap_SingleChangeAtMiddle_GetBeforeChange()
        {
            var microsecondsPerQuarterNote = 100000;

            using (var tempoMapManager = new TempoMapManager())
            {
                tempoMapManager.SetTempo(1000, new Tempo(microsecondsPerQuarterNote));

                var tempoMap = tempoMapManager.TempoMap;
                var tempo = tempoMap.GetTempoAtTime(new MidiTimeSpan(100));
                ClassicAssert.AreEqual(Tempo.Default, tempo, "Tempo is invalid.");
            }
        }

        [Test]
        public void GetTempoAtTime_NonDefaultTempoMap_SingleChangeAtMiddle_GetAtChange()
        {
            var microsecondsPerQuarterNote = 100000;

            using (var tempoMapManager = new TempoMapManager())
            {
                tempoMapManager.SetTempo(1000, new Tempo(microsecondsPerQuarterNote));

                var tempoMap = tempoMapManager.TempoMap;
                var tempo = tempoMap.GetTempoAtTime(new MidiTimeSpan(1000));
                ClassicAssert.AreEqual(new Tempo(microsecondsPerQuarterNote), tempo, "Tempo is invalid.");
            }
        }

        [Test]
        public void GetTempoAtTime_NonDefaultTempoMap_SingleChangeAtMiddle_GetAfterChange()
        {
            var microsecondsPerQuarterNote = 100000;

            using (var tempoMapManager = new TempoMapManager())
            {
                tempoMapManager.SetTempo(1000, new Tempo(microsecondsPerQuarterNote));

                var tempoMap = tempoMapManager.TempoMap;
                var tempo = tempoMap.GetTempoAtTime(new MidiTimeSpan(1500));
                ClassicAssert.AreEqual(new Tempo(microsecondsPerQuarterNote), tempo, "Tempo is invalid.");
            }
        }

        [Test]
        public void GetTempoAtTime_NonDefaultTempoMap_MultipleChangesAtStart_GetAtStart()
        {
            var microsecondsPerQuarterNote1 = 100000;
            var microsecondsPerQuarterNote2 = 700000;

            using (var tempoMapManager = new TempoMapManager())
            {
                tempoMapManager.SetTempo(0, new Tempo(microsecondsPerQuarterNote1));
                tempoMapManager.SetTempo(0, new Tempo(microsecondsPerQuarterNote2));

                var tempoMap = tempoMapManager.TempoMap;
                var tempo = tempoMap.GetTempoAtTime(new MidiTimeSpan(0));
                ClassicAssert.AreEqual(new Tempo(microsecondsPerQuarterNote2), tempo, "Tempo is invalid.");
            }
        }

        [Test]
        public void GetTempoAtTime_NonDefaultTempoMap_MultipleChangesAtStart_GetAtMiddle()
        {
            var microsecondsPerQuarterNote1 = 100000;
            var microsecondsPerQuarterNote2 = 700000;

            using (var tempoMapManager = new TempoMapManager())
            {
                tempoMapManager.SetTempo(0, new Tempo(microsecondsPerQuarterNote1));
                tempoMapManager.SetTempo(0, new Tempo(microsecondsPerQuarterNote2));

                var tempoMap = tempoMapManager.TempoMap;
                var tempo = tempoMap.GetTempoAtTime(new MidiTimeSpan(100));
                ClassicAssert.AreEqual(new Tempo(microsecondsPerQuarterNote2), tempo, "Tempo is invalid.");
            }
        }

        [Test]
        public void GetTempoAtTime_NonDefaultTempoMap_MultipleChangesAtMiddle_GetAtStart()
        {
            var microsecondsPerQuarterNote1 = 100000;
            var microsecondsPerQuarterNote2 = 700000;

            using (var tempoMapManager = new TempoMapManager())
            {
                tempoMapManager.SetTempo(100, new Tempo(microsecondsPerQuarterNote1));
                tempoMapManager.SetTempo(1000, new Tempo(microsecondsPerQuarterNote2));

                var tempoMap = tempoMapManager.TempoMap;
                var tempo = tempoMap.GetTempoAtTime(new MetricTimeSpan());
                ClassicAssert.AreEqual(Tempo.Default, tempo, "Tempo is invalid.");
            }
        }

        [Test]
        public void GetTempoAtTime_NonDefaultTempoMap_MultipleChangesAtMiddle_GetBeforeFirstChange()
        {
            var microsecondsPerQuarterNote1 = 100000;
            var microsecondsPerQuarterNote2 = 700000;

            using (var tempoMapManager = new TempoMapManager())
            {
                tempoMapManager.SetTempo(100, new Tempo(microsecondsPerQuarterNote1));
                tempoMapManager.SetTempo(1000, new Tempo(microsecondsPerQuarterNote2));

                var tempoMap = tempoMapManager.TempoMap;
                var tempo = tempoMap.GetTempoAtTime(new MidiTimeSpan(99));
                ClassicAssert.AreEqual(Tempo.Default, tempo, "Tempo is invalid.");
            }
        }

        [Test]
        public void GetTempoAtTime_NonDefaultTempoMap_MultipleChangesAtMiddle_GetAtFirstChange()
        {
            var microsecondsPerQuarterNote1 = 100000;
            var microsecondsPerQuarterNote2 = 700000;

            using (var tempoMapManager = new TempoMapManager())
            {
                tempoMapManager.SetTempo(100, new Tempo(microsecondsPerQuarterNote1));
                tempoMapManager.SetTempo(1000, new Tempo(microsecondsPerQuarterNote2));

                var tempoMap = tempoMapManager.TempoMap;
                var tempo = tempoMap.GetTempoAtTime(new MidiTimeSpan(100));
                ClassicAssert.AreEqual(new Tempo(microsecondsPerQuarterNote1), tempo, "Tempo is invalid.");
            }
        }

        [Test]
        public void GetTempoAtTime_NonDefaultTempoMap_MultipleChangesAtMiddle_GetBetweenChanges()
        {
            var microsecondsPerQuarterNote1 = 100000;
            var microsecondsPerQuarterNote2 = 700000;

            using (var tempoMapManager = new TempoMapManager())
            {
                tempoMapManager.SetTempo(100, new Tempo(microsecondsPerQuarterNote1));
                tempoMapManager.SetTempo(1000, new Tempo(microsecondsPerQuarterNote2));

                var tempoMap = tempoMapManager.TempoMap;
                var tempo = tempoMap.GetTempoAtTime(new MidiTimeSpan(500));
                ClassicAssert.AreEqual(new Tempo(microsecondsPerQuarterNote1), tempo, "Tempo is invalid.");
            }
        }

        [Test]
        public void GetTempoAtTime_NonDefaultTempoMap_MultipleChangesAtMiddle_GetAtSecondChange()
        {
            var microsecondsPerQuarterNote1 = 100000;
            var microsecondsPerQuarterNote2 = 700000;

            using (var tempoMapManager = new TempoMapManager())
            {
                tempoMapManager.SetTempo(100, new Tempo(microsecondsPerQuarterNote1));
                tempoMapManager.SetTempo(1000, new Tempo(microsecondsPerQuarterNote2));

                var tempoMap = tempoMapManager.TempoMap;
                var tempo = tempoMap.GetTempoAtTime(new MidiTimeSpan(1000));
                ClassicAssert.AreEqual(new Tempo(microsecondsPerQuarterNote2), tempo, "Tempo is invalid.");
            }
        }

        [Test]
        public void GetTempoAtTime_NonDefaultTempoMap_MultipleChangesAtMiddle_GetAfterSecondChange()
        {
            var microsecondsPerQuarterNote1 = 100000;
            var microsecondsPerQuarterNote2 = 700000;

            using (var tempoMapManager = new TempoMapManager())
            {
                tempoMapManager.SetTempo(100, new Tempo(microsecondsPerQuarterNote1));
                tempoMapManager.SetTempo(1000, new Tempo(microsecondsPerQuarterNote2));

                var tempoMap = tempoMapManager.TempoMap;
                var tempo = tempoMap.GetTempoAtTime(new MidiTimeSpan(5000));
                ClassicAssert.AreEqual(new Tempo(microsecondsPerQuarterNote2), tempo, "Tempo is invalid.");
            }
        }

        [Test]
        public void GetTimeSignatureAtTime_DefaultTempoMap_AtStart()
        {
            var tempoMap = TempoMap.Default;
            var timeSignature = tempoMap.GetTimeSignatureAtTime(new MidiTimeSpan(0));
            ClassicAssert.AreEqual(TimeSignature.Default, timeSignature, "Time signature is invalid.");
        }

        [Test]
        public void GetTimeSignatureAtTime_DefaultTempoMap_AtMiddle()
        {
            var tempoMap = TempoMap.Default;
            var timeSignature = tempoMap.GetTimeSignatureAtTime(MusicalTimeSpan.Quarter);
            ClassicAssert.AreEqual(TimeSignature.Default, timeSignature, "Time signature is invalid.");
        }

        [Test]
        public void GetTimeSignatureAtTime_NonDefaultTempoMap_SingleChangeAtStart_GetAtStart()
        {
            var numerator = 1;
            var denominator = 16;

            var tempoMap = TempoMap.Create(new TimeSignature(numerator, denominator));
            var timeSignature = tempoMap.GetTimeSignatureAtTime(new MidiTimeSpan(0));
            ClassicAssert.AreEqual(new TimeSignature(numerator, denominator), timeSignature, "Time signature is invalid.");
        }

        [Test]
        public void GetTimeSignatureAtTime_NonDefaultTempoMap_SingleChangeAtStart_GetAtMiddle()
        {
            var numerator = 1;
            var denominator = 16;

            var tempoMap = TempoMap.Create(new TimeSignature(numerator, denominator));
            var timeSignature = tempoMap.GetTimeSignatureAtTime(new MetricTimeSpan(0, 0, 1));
            ClassicAssert.AreEqual(new TimeSignature(numerator, denominator), timeSignature, "Time signature is invalid.");
        }

        [Test]
        public void GetTimeSignatureAtTime_NonDefaultTempoMap_SingleChangeAtMiddle_GetAtStart()
        {
            var numerator = 1;
            var denominator = 16;

            using (var tempoMapManager = new TempoMapManager())
            {
                tempoMapManager.SetTimeSignature(1000, new TimeSignature(numerator, denominator));

                var tempoMap = tempoMapManager.TempoMap;
                var timeSignature = tempoMap.GetTimeSignatureAtTime(new MidiTimeSpan(0));
                ClassicAssert.AreEqual(TimeSignature.Default, timeSignature, "Time signature is invalid.");
            }
        }

        [Test]
        public void GetTimeSignatureAtTime_NonDefaultTempoMap_SingleChangeAtMiddle_GetBeforeChange()
        {
            var numerator = 1;
            var denominator = 16;

            using (var tempoMapManager = new TempoMapManager())
            {
                tempoMapManager.SetTimeSignature(1000, new TimeSignature(numerator, denominator));

                var tempoMap = tempoMapManager.TempoMap;
                var timeSignature = tempoMap.GetTimeSignatureAtTime(new MidiTimeSpan(100));
                ClassicAssert.AreEqual(TimeSignature.Default, timeSignature, "Time signature is invalid.");
            }
        }

        [Test]
        public void GetTimeSignatureAtTime_NonDefaultTempoMap_SingleChangeAtMiddle_GetAtChange()
        {
            var numerator = 1;
            var denominator = 16;

            using (var tempoMapManager = new TempoMapManager())
            {
                tempoMapManager.SetTimeSignature(1000, new TimeSignature(numerator, denominator));

                var tempoMap = tempoMapManager.TempoMap;
                var timeSignature = tempoMap.GetTimeSignatureAtTime(new MidiTimeSpan(1000));
                ClassicAssert.AreEqual(new TimeSignature(numerator, denominator), timeSignature, "Time signature is invalid.");
            }
        }

        [Test]
        public void GetTimeSignatureAtTime_NonDefaultTempoMap_SingleChangeAtMiddle_GetAfterChange()
        {
            var numerator = 1;
            var denominator = 16;

            using (var tempoMapManager = new TempoMapManager())
            {
                tempoMapManager.SetTimeSignature(1000, new TimeSignature(numerator, denominator));

                var tempoMap = tempoMapManager.TempoMap;
                var timeSignature = tempoMap.GetTimeSignatureAtTime(new MidiTimeSpan(1500));
                ClassicAssert.AreEqual(new TimeSignature(numerator, denominator), timeSignature, "Time signature is invalid.");
            }
        }

        [Test]
        public void GetTimeSignatureAtTime_NonDefaultTempoMap_MultipleChangesAtStart_GetAtStart()
        {
            var numerator1 = 1;
            var denominator1 = 16;

            var numerator2 = 3;
            var denominator2 = 8;

            using (var tempoMapManager = new TempoMapManager())
            {
                tempoMapManager.SetTimeSignature(0, new TimeSignature(numerator1, denominator1));
                tempoMapManager.SetTimeSignature(0, new TimeSignature(numerator2, denominator2));

                var tempoMap = tempoMapManager.TempoMap;
                var timeSignature = tempoMap.GetTimeSignatureAtTime(new MidiTimeSpan(0));
                ClassicAssert.AreEqual(new TimeSignature(numerator2, denominator2), timeSignature, "Time signature is invalid.");
            }
        }

        [Test]
        public void GetTimeSignatureAtTime_NonDefaultTempoMap_MultipleChangesAtStart_GetAtMiddle()
        {
            var numerator1 = 1;
            var denominator1 = 16;

            var numerator2 = 3;
            var denominator2 = 8;

            using (var tempoMapManager = new TempoMapManager())
            {
                tempoMapManager.SetTimeSignature(0, new TimeSignature(numerator1, denominator1));
                tempoMapManager.SetTimeSignature(0, new TimeSignature(numerator2, denominator2));

                var tempoMap = tempoMapManager.TempoMap;
                var timeSignature = tempoMap.GetTimeSignatureAtTime(new MidiTimeSpan(100));
                ClassicAssert.AreEqual(new TimeSignature(numerator2, denominator2), timeSignature, "Time signature is invalid.");
            }
        }

        [Test]
        public void GetTimeSignatureAtTime_NonDefaultTempoMap_MultipleChangesAtMiddle_GetAtStart()
        {
            var numerator1 = 1;
            var denominator1 = 16;

            var numerator2 = 3;
            var denominator2 = 8;

            using (var tempoMapManager = new TempoMapManager())
            {
                tempoMapManager.SetTimeSignature(100, new TimeSignature(numerator1, denominator1));
                tempoMapManager.SetTimeSignature(1000, new TimeSignature(numerator2, denominator2));

                var tempoMap = tempoMapManager.TempoMap;
                var timeSignature = tempoMap.GetTimeSignatureAtTime(new MetricTimeSpan());
                ClassicAssert.AreEqual(TimeSignature.Default, timeSignature, "Time signature is invalid.");
            }
        }

        [Test]
        public void GetTimeSignatureAtTime_NonDefaultTempoMap_MultipleChangesAtMiddle_GetBeforeFirstChange()
        {
            var numerator1 = 1;
            var denominator1 = 16;

            var numerator2 = 3;
            var denominator2 = 8;

            using (var tempoMapManager = new TempoMapManager())
            {
                tempoMapManager.SetTimeSignature(100, new TimeSignature(numerator1, denominator1));
                tempoMapManager.SetTimeSignature(1000, new TimeSignature(numerator2, denominator2));

                var tempoMap = tempoMapManager.TempoMap;
                var timeSignature = tempoMap.GetTimeSignatureAtTime(new MidiTimeSpan(99));
                ClassicAssert.AreEqual(TimeSignature.Default, timeSignature, "Time signature is invalid.");
            }
        }

        [Test]
        public void GetTimeSignatureAtTime_NonDefaultTempoMap_MultipleChangesAtMiddle_GetAtFirstChange()
        {
            var numerator1 = 1;
            var denominator1 = 16;

            var numerator2 = 3;
            var denominator2 = 8;

            using (var tempoMapManager = new TempoMapManager())
            {
                tempoMapManager.SetTimeSignature(100, new TimeSignature(numerator1, denominator1));
                tempoMapManager.SetTimeSignature(1000, new TimeSignature(numerator2, denominator2));

                var tempoMap = tempoMapManager.TempoMap;
                var timeSignature = tempoMap.GetTimeSignatureAtTime(new MidiTimeSpan(100));
                ClassicAssert.AreEqual(new TimeSignature(numerator1, denominator1), timeSignature, "Time signature is invalid.");
            }
        }

        [Test]
        public void GetTimeSignatureAtTime_NonDefaultTempoMap_MultipleChangesAtMiddle_GetBetweenChanges()
        {
            var numerator1 = 1;
            var denominator1 = 16;

            var numerator2 = 3;
            var denominator2 = 8;

            using (var tempoMapManager = new TempoMapManager())
            {
                tempoMapManager.SetTimeSignature(100, new TimeSignature(numerator1, denominator1));
                tempoMapManager.SetTimeSignature(1000, new TimeSignature(numerator2, denominator2));

                var tempoMap = tempoMapManager.TempoMap;
                var timeSignature = tempoMap.GetTimeSignatureAtTime(new MidiTimeSpan(500));
                ClassicAssert.AreEqual(new TimeSignature(numerator1, denominator1), timeSignature, "Time signature is invalid.");
            }
        }

        [Test]
        public void GetTimeSignatureAtTime_NonDefaultTempoMap_MultipleChangesAtMiddle_GetAtSecondChange()
        {
            var numerator1 = 1;
            var denominator1 = 16;

            var numerator2 = 3;
            var denominator2 = 8;

            using (var tempoMapManager = new TempoMapManager())
            {
                tempoMapManager.SetTimeSignature(100, new TimeSignature(numerator1, denominator1));
                tempoMapManager.SetTimeSignature(1000, new TimeSignature(numerator2, denominator2));

                var tempoMap = tempoMapManager.TempoMap;
                var timeSignature = tempoMap.GetTimeSignatureAtTime(new MidiTimeSpan(1000));
                ClassicAssert.AreEqual(new TimeSignature(numerator2, denominator2), timeSignature, "Time signature is invalid.");
            }
        }

        [Test]
        public void GetTimeSignatureAtTime_NonDefaultTempoMap_MultipleChangesAtMiddle_GetAfterSecondChange()
        {
            var numerator1 = 1;
            var denominator1 = 16;

            var numerator2 = 3;
            var denominator2 = 8;

            using (var tempoMapManager = new TempoMapManager())
            {
                tempoMapManager.SetTimeSignature(100, new TimeSignature(numerator1, denominator1));
                tempoMapManager.SetTimeSignature(1000, new TimeSignature(numerator2, denominator2));

                var tempoMap = tempoMapManager.TempoMap;
                var timeSignature = tempoMap.GetTimeSignatureAtTime(new MidiTimeSpan(5000));
                ClassicAssert.AreEqual(new TimeSignature(numerator2, denominator2), timeSignature, "Time signature is invalid.");
            }
        }

        #endregion

        #region Private methods

        private static void TestSimpleTempoMap(TempoMap tempoMap,
                                               TimeDivision expectedTimeDivision,
                                               Tempo expectedTempo,
                                               TimeSignature expectedTimeSignature)
        {
            ClassicAssert.AreEqual(expectedTimeDivision,
                            tempoMap.TimeDivision,
                            "Unexpected time division.");

            ClassicAssert.AreEqual(expectedTempo,
                            tempoMap.GetTempoAtTime(new MidiTimeSpan(0)),
                            "Unexpected tempo at the start of tempo map.");
            ClassicAssert.AreEqual(expectedTempo,
                            tempoMap.GetTempoAtTime(new MidiTimeSpan(1000)),
                            "Unexpected tempo at the arbitrary time of tempo map.");

            ClassicAssert.AreEqual(expectedTimeSignature,
                            tempoMap.GetTimeSignatureAtTime(new MidiTimeSpan(0)),
                            "Unexpected time signature at the start of tempo map.");
            ClassicAssert.AreEqual(expectedTimeSignature,
                            tempoMap.GetTimeSignatureAtTime(new MidiTimeSpan(1000)),
                            "Unexpected time signature at the arbitrary time of tempo map.");
        }

        #endregion
    }
}
