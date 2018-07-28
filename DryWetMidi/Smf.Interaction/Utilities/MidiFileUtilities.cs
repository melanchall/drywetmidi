using Melanchall.DryWetMidi.Common;

namespace Melanchall.DryWetMidi.Smf.Interaction
{
    public static class MidiFileUtilities
    {
        #region Methods

        public static void ShiftEvents(this MidiFile midiFile, ITimeSpan distance)
        {
            ThrowIfArgument.IsNull(nameof(midiFile), midiFile);
            ThrowIfArgument.IsNull(nameof(distance), distance);

            midiFile.GetTrackChunks().ShiftEvents(distance, midiFile.TimeDivision);
        }

        #endregion
    }
}
