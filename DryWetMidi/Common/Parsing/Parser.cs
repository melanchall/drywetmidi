using System;
using System.Diagnostics.CodeAnalysis;

namespace Melanchall.DryWetMidi.Common
{
    internal abstract class Parser
    {
        [DoesNotReturn]
        protected void ThrowError(string error)
        {
            throw new FormatException(error);
        }

        [DoesNotReturn]
        protected void ThrowInvalidFormatError()
        {
            throw new FormatException("Input string has invalid format.");
        }
    }
}
