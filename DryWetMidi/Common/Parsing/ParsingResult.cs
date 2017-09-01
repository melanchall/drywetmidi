using System;

namespace Melanchall.DryWetMidi.Common
{
    internal sealed class ParsingResult
    {
        #region Constants

        public static readonly ParsingResult Parsed = new ParsingResult(ParsingStatus.Parsed);
        public static readonly ParsingResult EmptyInputString = new ParsingResult(ParsingStatus.EmptyInputString);
        public static readonly ParsingResult NotMatched = new ParsingResult(ParsingStatus.NotMatched);

        #endregion

        #region Constructor

        public ParsingResult(string error)
            : this(ParsingStatus.FormatError, error)
        {
        }

        private ParsingResult(ParsingStatus status)
            : this(status, null)
        {
        }

        private ParsingResult(ParsingStatus status, string error)
        {
            Status = status;
            Error = error;
        }

        #endregion

        #region Properties

        public ParsingStatus Status { get; }

        public string Error { get; }

        public Exception Exception
        {
            get
            {
                switch (Status)
                {
                    case ParsingStatus.EmptyInputString:
                        return new ArgumentException("Input string is null or contains white-spaces only.");
                    case ParsingStatus.NotMatched:
                        return new FormatException("Input string has invalid format.");
                    case ParsingStatus.FormatError:
                        return new FormatException(Error);
                }

                return null;
            }
        }

        #endregion
    }
}
