using System;
using System.Runtime.Serialization;

namespace Melanchall.DryWetMidi
{
    [Serializable]
    public sealed class UnknownFileFormatException : MidiException
    {
        #region Constants

        private const string FileFormatSerializationPropertyName = "FileFormat";

        #endregion

        #region Constructors

        public UnknownFileFormatException()
            : base()
        {
        }

        public UnknownFileFormatException(string message)
            : base(message)
        {
        }

        public UnknownFileFormatException(string message, ushort fileFormat)
            : this(message)
        {
        }

        private UnknownFileFormatException(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
            FileFormat = info.GetUInt16(FileFormatSerializationPropertyName);
        }

        #endregion

        #region Properties

        public ushort FileFormat { get; }

        #endregion

        #region Overrides

        public override void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            if (info == null)
                throw new ArgumentNullException(nameof(info));

            info.AddValue(FileFormatSerializationPropertyName, FileFormat);

            base.GetObjectData(info, context);
        }

        #endregion
    }
}
