using Melanchall.DryWetMidi.Common;
using Melanchall.DryWetMidi.Composing;
using Melanchall.DryWetMidi.Core;
using Melanchall.DryWetMidi.Interaction;
using Melanchall.DryWetMidi.MusicTheory;
using Melanchall.DryWetMidi.Standards;
using NUnit.Framework;

namespace Melanchall.DryWetMidi.Tests.Composing
{
    [TestFixture]
    public sealed partial class PatternBuilderTests
    {
        #region Test methods

        [Test]
        public void ProgramChange_Number()
        {
            var programNumber = (SevenBitNumber)10;
            var eventTime = MusicalTimeSpan.Quarter;

            var pattern = new PatternBuilder()

                .Note(NoteName.A, eventTime)
                .ProgramChange(programNumber)

                .Build();

            PatternTestUtilities.TestTimedEvents(pattern, new[]
            {
                new TimedEventInfo(new ProgramChangeEvent(programNumber) { Channel = PatternTestUtilities.Channel }, eventTime)
            });
        }

        [Test]
        public void ProgramChange_GeneralMidiProgram()
        {
            var program1 = GeneralMidiProgram.Applause;
            var program2 = GeneralMidiProgram.AltoSax;
            var eventTime = MusicalTimeSpan.Quarter;

            var noteNumber = (SevenBitNumber)100;
            var note = DryWetMidi.MusicTheory.Note.Get(noteNumber);

            var pattern = new PatternBuilder()

                .ProgramChange(program1)
                .Note(note, eventTime)
                .ProgramChange(program2)

                .Build();

            PatternTestUtilities.TestTimedEventsWithExactOrder(pattern, new[]
            {
                new TimedEventInfo(new ProgramChangeEvent(program1.AsSevenBitNumber()) { Channel = PatternTestUtilities.Channel }, new MidiTimeSpan()),
                new TimedEventInfo(new NoteOnEvent(noteNumber, DryWetMidi.Interaction.Note.DefaultVelocity) { Channel = PatternTestUtilities.Channel }, new MidiTimeSpan()),
                new TimedEventInfo(new ProgramChangeEvent(program2.AsSevenBitNumber()) { Channel = PatternTestUtilities.Channel }, eventTime),
                new TimedEventInfo(new NoteOffEvent(noteNumber, SevenBitNumber.MinValue) { Channel = PatternTestUtilities.Channel }, eventTime)
            });
        }

        [Test]
        public void ProgramChange_GeneralMidi2Program()
        {
            var eventsTime = MusicalTimeSpan.Quarter;

            var bankMsbControlNumber = ControlName.BankSelect.AsSevenBitNumber();
            var bankMsb = (SevenBitNumber)0x79;

            var bankLsbControlNumber = ControlName.LsbForBankSelect.AsSevenBitNumber();
            var bankLsb = (SevenBitNumber)0x03;

            var generalMidiProgram = GeneralMidiProgram.BirdTweet;
            var generalMidi2Program = GeneralMidi2Program.BirdTweet2;

            var pattern = new PatternBuilder()

                .Note(NoteName.A, eventsTime)
                .ProgramChange(generalMidi2Program)

                .Build();

            PatternTestUtilities.TestTimedEvents(pattern, new[]
            {
                new TimedEventInfo(new ControlChangeEvent(bankMsbControlNumber, bankMsb) { Channel = PatternTestUtilities.Channel }, eventsTime),
                new TimedEventInfo(new ControlChangeEvent(bankLsbControlNumber, bankLsb) { Channel = PatternTestUtilities.Channel }, eventsTime),
                new TimedEventInfo(new ProgramChangeEvent(generalMidiProgram.AsSevenBitNumber()) { Channel = PatternTestUtilities.Channel }, eventsTime),
            });
        }

        #endregion
    }
}
