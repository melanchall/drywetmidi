using System.Collections.Generic;
using System.IO;
using Melanchall.DryWetMidi.Common;
using Melanchall.DryWetMidi.Smf.Interaction;

namespace Melanchall.DryWetMidi.Tools
{
    internal static class CsvToNotesConverter
    {
        #region Methods

        public static IEnumerable<Note> ConvertToNotes(Stream stream, TempoMap tempoMap, NoteCsvConversionSettings settings)
        {
            using (var csvReader = new CsvReader(stream, settings.CsvDelimiter))
            {
                CsvRecord record = null;

                while ((record = csvReader.ReadRecord()) != null)
                {
                    var values = record.Values;
                    if (values.Length < 6)
                        CsvError.ThrowBadFormat(record.LineNumber, "Missing required parameters.");

                    ITimeSpan time;
                    if (!TimeSpanUtilities.TryParse(values[0], settings.TimeType, out time))
                        CsvError.ThrowBadFormat(record.LineNumber, "Invalid time.");

                    FourBitNumber channel;
                    if (!TryParseFourBitNumber(values[1], out channel))
                        CsvError.ThrowBadFormat(record.LineNumber, "Invalid channel.");

                    SevenBitNumber noteNumber;
                    if (!TryParseNoteNumber(values[2], settings.NoteNumberFormat, out noteNumber))
                        CsvError.ThrowBadFormat(record.LineNumber, "Invalid note number or letter.");

                    ITimeSpan length;
                    if (!TimeSpanUtilities.TryParse(values[3], settings.NoteLengthType, out length))
                        CsvError.ThrowBadFormat(record.LineNumber, "Invalid length.");

                    SevenBitNumber velocity;
                    if (!TryParseSevenBitNumber(values[4], out velocity))
                        CsvError.ThrowBadFormat(record.LineNumber, "Invalid velocity.");

                    SevenBitNumber offVelocity;
                    if (!TryParseSevenBitNumber(values[5], out offVelocity))
                        CsvError.ThrowBadFormat(record.LineNumber, "Invalid off velocity.");

                    var convertedTime = TimeConverter.ConvertFrom(time, tempoMap);
                    var convertedLength = LengthConverter.ConvertFrom(length, convertedTime, tempoMap);

                    yield return new Note(noteNumber, convertedLength, convertedTime)
                    {
                        Channel = channel,
                        Velocity = velocity,
                        OffVelocity = offVelocity
                    };
                }
            }
        }

        public static bool TryParseNoteNumber(string input, NoteNumberFormat noteNumberFormat, out SevenBitNumber result)
        {
            result = default(SevenBitNumber);

            switch (noteNumberFormat)
            {
                case NoteNumberFormat.NoteNumber:
                    return TryParseSevenBitNumber(input, out result);
                case NoteNumberFormat.Letter:
                    {
                        MusicTheory.Note note;
                        if (!MusicTheory.Note.TryParse(input, out note))
                            return false;

                        result = note.NoteNumber;
                        return true;
                    }
            }

            return false;
        }

        private static bool TryParseFourBitNumber(string input, out FourBitNumber result)
        {
            result = default(FourBitNumber);

            byte value;
            if (!byte.TryParse(input, out value) || value < FourBitNumber.MinValue || value > FourBitNumber.MaxValue)
                return false;

            result = (FourBitNumber)value;
            return true;
        }

        private static bool TryParseSevenBitNumber(string input, out SevenBitNumber result)
        {
            result = default(SevenBitNumber);

            byte value;
            if (!byte.TryParse(input, out value) || value < SevenBitNumber.MinValue || value > SevenBitNumber.MaxValue)
                return false;

            result = (SevenBitNumber)value;
            return true;
        }

        #endregion
    }
}
