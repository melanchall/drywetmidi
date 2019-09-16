using Melanchall.DryWetMidi.Common;
using Melanchall.DryWetMidi.Smf.Interaction;

namespace Melanchall.DryWetMidi.Composing
{
    public delegate NoteDescriptor NoteTransformation(MusicTheory.Note note, SevenBitNumber velocity, ITimeSpan length);
}
