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

        #region Fields

        private readonly string _error;

        #endregion

        #region Constructor

        private ParsingResult(string error)
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
            _error = error;
        }

        #endregion

        #region Properties

        public ParsingStatus Status { get; }

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
                        return new FormatException(_error);
                }

                return null;
            }
        }

        #endregion

        #region Methods

        public static ParsingResult Error(string error)
        {
            return new ParsingResult(error);
        }

        #endregion
    }
}
