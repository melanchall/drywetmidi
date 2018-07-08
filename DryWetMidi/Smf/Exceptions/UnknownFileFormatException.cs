using Melanchall.DryWetMidi.Common;
using System;
using System.Runtime.Serialization;

namespace Melanchall.DryWetMidi.Smf
{
    /// <summary>
    /// The exception that is thrown when a MIDI file format is unknown.
    /// </summary>
    /// <remarks>
    /// Note that this exception will be thrown only if <see cref="ReadingSettings.UnknownFileFormatPolicy"/>
    /// is set to <see cref="UnknownFileFormatPolicy.Abort"/> for the <see cref="ReadingSettings"/>
    /// used for reading a MIDI file.
    /// </remarks>
    [Serializable]
    public sealed class UnknownFileFormatException : MidiException
    {
        #region Constants

        private const string FileFormatSerializationPropertyName = "FileFormat";

        #endregion

        #region Constructors

        /// <summary>
        /// Initializes a new instance of the <see cref="UnknownFileFormatException"/>.
        /// </summary>
        public UnknownFileFormatException()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="UnknownFileFormatException"/> with the
        /// specified error message.
        /// </summary>
        /// <param name="message">The error message that explains the reason for the exception.</param>
        public UnknownFileFormatException(string message)
            : base(message)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="UnknownFileFormatException"/> with the
        /// specified error message and format of a MIDI file.
        /// </summary>
        /// <param name="message">The error message that explains the reason for the exception.</param>
        /// <param name="fileFormat">Number that represents format of a MIDI file.</param>
        public UnknownFileFormatException(string message, ushort fileFormat)
            : this(message)
        {
            FileFormat = fileFormat;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="UnknownFileFormatException"/>
        /// with serialized data.
        /// </summary>
        /// <param name="info">The <see cref="SerializationInfo"/> that holds the serialized
        /// object data about the exception being thrown.</param>
        /// <param name="context">The <see cref="StreamingContext"/> that contains contextual information
        /// about the source or destination.</param>
        private UnknownFileFormatException(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
            FileFormat = info.GetUInt16(FileFormatSerializationPropertyName);
        }

        #endregion

        #region Properties

        /// <summary>
        /// Gets the number that represents format of a MIDI file.
        /// </summary>
        public ushort FileFormat { get; }

        #endregion

        #region Overrides

        /// <summary>
        /// Sets the <see cref="SerializationInfo"/> with information about the exception.
        /// </summary>
        /// <param name="info">The <see cref="SerializationInfo"/> that holds the serialized
        /// object data about the exception being thrown.</param>
        /// <param name="context">The <see cref="StreamingContext"/> that contains contextual information
        /// about the source or destination.</param>
        public override void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            ThrowIfArgument.IsNull(nameof(info), info);

            info.AddValue(FileFormatSerializationPropertyName, FileFormat);

            base.GetObjectData(info, context);
        }

        #endregion
    }
}
