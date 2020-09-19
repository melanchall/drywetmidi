using System;
using System.ComponentModel;
using System.Text;

namespace Melanchall.DryWetMidi.Core
{
    /// <summary>
    /// Settings according to which MIDI data should be written.
    /// </summary>
    public class WritingSettings
    {
        #region Properties

        public bool UseRunningStatus { get; set; }

        public bool NoteOffAsSilentNoteOn { get; set; }

        public bool DeleteDefaultTimeSignature { get; set; }

        public bool DeleteDefaultKeySignature { get; set; }

        public bool DeleteDefaultSetTempo { get; set; }

        public bool DeleteUnknownMetaEvents { get; set; }

        public bool DeleteUnknownChunks { get; set; }

        public bool WriteHeaderChunk { get; set; } = true;

        /// <summary>
        /// Gets or sets compression rules for the writing engine. The default is
        /// <see cref="CompressionPolicy.NoCompression"/>.
        /// </summary>
        /// <remarks>
        /// <para>You can specify <see cref="CompressionPolicy.Default"/> to use basic compression rules.
        /// <see cref="CompressionPolicy"/> is marked with <see cref="FlagsAttribute"/> so you can
        /// combine separate rules as you want.</para>
        /// </remarks>
        /// <exception cref="InvalidEnumArgumentException"><paramref name="value"/> specified an invalid value.</exception>
        [Obsolete("OBS1")]
        public CompressionPolicy CompressionPolicy
        {
            get
            {
                var result = CompressionPolicy.NoCompression;

                if (UseRunningStatus)
                    result |= CompressionPolicy.UseRunningStatus;

                if (NoteOffAsSilentNoteOn)
                    result |= CompressionPolicy.NoteOffAsSilentNoteOn;

                if (DeleteDefaultTimeSignature)
                    result |= CompressionPolicy.DeleteDefaultTimeSignature;

                if (DeleteDefaultKeySignature)
                    result |= CompressionPolicy.DeleteDefaultKeySignature;

                if (DeleteDefaultSetTempo)
                    result |= CompressionPolicy.DeleteDefaultSetTempo;

                if (DeleteUnknownMetaEvents)
                    result |= CompressionPolicy.DeleteUnknownMetaEvents;

                if (DeleteUnknownChunks)
                    result |= CompressionPolicy.DeleteUnknownChunks;

                return result;
            }
            set
            {
                UseRunningStatus = value.HasFlag(CompressionPolicy.UseRunningStatus);
                NoteOffAsSilentNoteOn = value.HasFlag(CompressionPolicy.NoteOffAsSilentNoteOn);
                DeleteDefaultTimeSignature = value.HasFlag(CompressionPolicy.DeleteDefaultTimeSignature);
                DeleteDefaultKeySignature = value.HasFlag(CompressionPolicy.DeleteDefaultKeySignature);
                DeleteDefaultSetTempo = value.HasFlag(CompressionPolicy.DeleteDefaultSetTempo);
                DeleteUnknownMetaEvents = value.HasFlag(CompressionPolicy.DeleteUnknownMetaEvents);
                DeleteUnknownChunks = value.HasFlag(CompressionPolicy.DeleteUnknownChunks);
            }
        }

        /// <summary>
        /// Gets or sets collection of custom meta events types.
        /// </summary>
        /// <remarks>
        /// <para>Types within this collection must be derived from the <see cref="MetaEvent"/>
        /// class and have parameterless constructor. No exception will be thrown
        /// while writing a MIDI file if some types don't meet these requirements.</para>
        /// </remarks>
        public EventTypesCollection CustomMetaEventTypes { get; set; }

        /// <summary>
        /// Gets or sets an <see cref="Encoding"/> that will be used to write the text of a
        /// text-based meta event. The default is <see cref="Encoding.ASCII"/>.
        /// </summary>
        public Encoding TextEncoding { get; set; } = SmfConstants.DefaultTextEncoding;

        public WriterSettings WriterSettings { get; set; } = new WriterSettings();

        #endregion
    }
}
