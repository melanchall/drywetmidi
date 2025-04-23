using Melanchall.DryWetMidi.Common;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Melanchall.DryWetMidi.Tools
{
    internal static class TypeParser
    {
        #region Nested enums

        public enum DataType
        {
            Byte,
            SByte,
            Long,
            UShort,
            String,
            Int,
            FourBitNumber,
            SevenBitNumber,
            NoteNumber,
            BytesArray,
        }

        #endregion

        #region Nested delegates

        private delegate object ParameterParser(string parameter, CsvDeserializationSettings settings);

        #endregion

        #region Constants

        private static readonly Dictionary<DataType, ParameterParser> ParameterParsers =
            new Dictionary<DataType, ParameterParser>
            {
                [DataType.Byte] = (p, s) => byte.Parse(p),
                [DataType.SByte] = (p, s) => sbyte.Parse(p),
                [DataType.Long] = (p, s) => long.Parse(p),
                [DataType.UShort] = (p, s) => ushort.Parse(p),
                [DataType.String] = (p, s) => p,
                [DataType.Int] = (p, s) => int.Parse(p),
                [DataType.FourBitNumber] = (p, s) => FourBitNumber.Parse(p),
                [DataType.SevenBitNumber] = (p, s) => SevenBitNumber.Parse(p),
                [DataType.NoteNumber] = (p, s) =>
                {
                    switch (s.NoteFormat)
                    {
                        case CsvNoteFormat.NoteNumber:
                            return SevenBitNumber.Parse(p);
                        case CsvNoteFormat.Letter:
                            return MusicTheory.Note.Parse(p).NoteNumber;
                    }

                    return !p.Any(char.IsLetter)
                        ? SevenBitNumber.Parse(p)
                        : MusicTheory.Note.Parse(p).NoteNumber;
                },
                [DataType.BytesArray] = (p, s) =>
                {
                    return p
                        .Split(s.BytesArrayDelimiter)
                        .Select(b => b.Trim())
                        .Where(b => !string.IsNullOrWhiteSpace(b))
                        .Select(b =>
                        {
                            if (s.BytesArrayFormat == CsvBytesArrayFormat.Hexadecimal)
                                return Convert.ToByte(b, 16);

                            return byte.Parse(b);
                        })
                        .ToArray();
                },
            };

        #endregion

        #region Methods

        public static object Parse(
            string value,
            DataType dataType,
            string valueDescription,
            int? lineNumber,
            CsvDeserializationSettings settings)
        {
            var parser = ParameterParsers[dataType];

            try
            {
                return parser(value, settings);
            }
            catch (Exception ex)
            {
                CsvError.ThrowBadFormat(lineNumber, $"Invalid {valueDescription} ({value}).", ex);
                return null;
            }
        }

        #endregion
    }
}
