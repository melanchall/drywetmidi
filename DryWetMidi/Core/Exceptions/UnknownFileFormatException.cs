using Melanchall.DryWetMidi.Common;
using System;
using System.Runtime.Serialization;

namespace Melanchall.DryWetMidi.Core
{
    /// <summary>
    /// The exception that is thrown when a MIDI file format is unknown.
    /// </summary>
    /// <remarks>
    /// <para>Note that this exception will be thrown only if <see cref="ReadingSettings.UnknownFileFormatPolicy"/>
    /// is set to <see cref="UnknownFileFormatPolicy.Abort"/> for the <see cref="ReadingSettings"/>
    /// used for reading a MIDI file.</para>
    /// </remarks>
    [Serializable]
    public sealed class UnknownFileFormatException : MidiException
    {
        #region Constructors

        internal UnknownFileFormatException(ushort fileFormat)
            : base($"File format {fileFormat} is unknown.")
        {
            FileFormat = fileFormat;
        }

        private UnknownFileFormatException(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
            FileFormat = info.GetUInt16(nameof(FileFormat));
        }

        #endregion

        #region Properties

        /// <summary>
        /// Gets the number that represents format of a MIDI file.
        /// </summary>
        public ushort FileFormat { get; }

        #endregion

        #region Overrides

        public override void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            base.GetObjectData(info, context);

            info.AddValue(nameof(FileFormat), FileFormat);
        }

        #endregion
    }
}
