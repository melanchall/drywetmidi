using System;
using System.Linq;
using Melanchall.DryWetMidi.Common;
using Melanchall.DryWetMidi.Smf;
using Melanchall.DryWetMidi.Smf.Interaction;
using NUnit.Framework;

namespace Melanchall.DryWetMidi.Tests
{
    [TestFixture]
    public class QuantizeNotes
    {
        #region Test methods

        [Test]
        [Description("Quantize notes by quarter-step grid.")]
        public void Quantize_Musical_Quarter()
        {
            var midiFile = new MidiFile(new TrackChunk())
            {
                TimeDivision = new TicksPerQuarterNoteTimeDivision(10)
            };

            using (var notesManager = midiFile.GetTrackChunks().First().ManageNotes())
            {
                var notes = notesManager.Notes;
                notes.Add(new Note(SevenBitNumber.MaxValue, 10, 0),
                          new Note(SevenBitNumber.MaxValue, 10, 1),
                          new Note(SevenBitNumber.MaxValue, 10, 8),
                          new Note(SevenBitNumber.MaxValue, 10, 5),
                          new Note(SevenBitNumber.MaxValue, 10, 15),
                          new Note(SevenBitNumber.MaxValue, 10, 19));
            }

            Quantize(midiFile, MusicalTimeSpan.Quarter);

            var actualTimes = midiFile.GetNotes()
                                      .Select(n => n.Time)
                                      .Distinct()
                                      .ToList();
            var expectedTimes = Enumerable.Range(0, actualTimes.Count)
                                          .Select(i => i * 10L)
                                          .ToList();

            CollectionAssert.AreEqual(actualTimes, expectedTimes);
        }

        #endregion

        #region Private methods

        private static void Quantize(MidiFile midiFile, MusicalTimeSpan step)
        {
            var tempoMap = midiFile.GetTempoMap();
            var stepTicks = LengthConverter.ConvertFrom(step, 0, tempoMap);

            midiFile.ProcessNotes(n =>
            {
                var time = n.Time;
                n.Time = (long)Math.Round(time / (double)stepTicks, MidpointRounding.AwayFromZero) * stepTicks;
            });
        }

        #endregion
    }
}
