namespace Melanchall.DryWetMidi.Core
{
    internal static class MidiFileEquality
    {
        #region Methods

        public static bool Equals(MidiFile midiFile1, MidiFile midiFile2, MidiFileEqualityCheckSettings settings, out string message)
        {
            message = null;

            if (ReferenceEquals(midiFile1, midiFile2))
                return true;

            if (ReferenceEquals(null, midiFile1) || ReferenceEquals(null, midiFile2))
            {
                message = "One of files is null.";
                return false;
            }

            if (settings.CompareOriginalFormat)
            {
                var originalFormat1 = midiFile1._originalFormat;
                var originalFormat2 = midiFile2._originalFormat;

                if (originalFormat1 != originalFormat2)
                {
                    message = $"Original formats are different ({originalFormat1} vs {originalFormat2}).";
                    return false;
                }
            }

            var timeDivision1 = midiFile1.TimeDivision;
            var timeDivision2 = midiFile2.TimeDivision;
            if (!ReferenceEquals(timeDivision1, timeDivision2))
            {
                if (ReferenceEquals(null, timeDivision1) || ReferenceEquals(null, timeDivision2))
                {
                    message = "Time division of one of the files is null.";
                    return false;
                }

                if (!timeDivision1.Equals(timeDivision2))
                {
                    message = $"Time divisions are different ({timeDivision1} vs {timeDivision2}).";
                    return false;
                }
            }

            var chunks1 = midiFile1.Chunks;
            var chunks2 = midiFile2.Chunks;

            if (chunks1.Count != chunks2.Count)
            {
                message = $"Counts of chunks are different ({chunks1.Count} vs {chunks2.Count}).";
                return false;
            }

            for (var i = 0; i < chunks1.Count; i++)
            {
                var chunk1 = chunks1[i];
                var chunk2 = chunks2[i];

                string chunksComparingMessage;
                if (!MidiChunk.Equals(chunk1, chunk2, settings.ChunkEqualityCheckSettings, out chunksComparingMessage))
                {
                    message = $"Chunks at position {i} are different. {chunksComparingMessage}";
                    return false;
                }
            }

            return true;
        }

        #endregion
    }
}
