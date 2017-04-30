using System;

namespace Melanchall.DryWetMidi
{
    public abstract class MetaEvent : MidiEvent
    {
        #region Overrides

        internal sealed override void ReadContent(MidiReader reader, ReadingSettings settings, int size)
        {
            ReadContentData(reader, settings, size);
        }

        internal sealed override void WriteContent(MidiWriter writer, WritingSettings settings)
        {
            WriteContentData(writer, settings);
        }

        internal sealed override int GetContentSize()
        {
            return GetContentDataSize();
        }

        #endregion

        #region Methods

        protected abstract void ReadContentData(MidiReader reader, ReadingSettings settings, int size);

        protected abstract void WriteContentData(MidiWriter writer, WritingSettings settings);

        protected abstract int GetContentDataSize();

        #endregion
    }
}
