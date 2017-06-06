using System;

namespace Melanchall.DryWetMidi.Smf
{
    /// <summary>
    /// Represents a Key Signature meta event.
    /// </summary>
    /// <remarks>
    /// The MIDI key signature meta message specifies the key signature and scale of a MIDI file.
    /// </remarks>
    public sealed class KeySignatureEvent : MetaEvent
    {
        #region Constants

        /// <summary>
        /// Default key (C).
        /// </summary>
        public const sbyte DefaultKey = 0;

        /// <summary>
        /// Default scale (major).
        /// </summary>
        public const byte DefaultScale = 0;

        private const sbyte MinKey = -7;
        private const sbyte MaxKey = 7;

        private const byte MinScale = 0;
        private const byte MaxScale = 1;

        #endregion

        #region Fields

        private sbyte _key = DefaultKey;
        private byte _scale = DefaultScale;

        #endregion

        #region Constructor

        /// <summary>
        /// Initializes a new instance of the <see cref="KeySignatureEvent"/>.
        /// </summary>
        public KeySignatureEvent()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="KeySignatureEvent"/> with the
        /// specified key and scale.
        /// </summary>
        /// <param name="key"></param>
        /// <param name="scale"></param>
        public KeySignatureEvent(sbyte key, byte scale)
            : this()
        {
            Key = key;
            Scale = scale;
        }

        #endregion

        #region Properties

        /// <summary>
        /// Gets or sets key signature in terms of number of flats (if negative) or
        /// sharps (if positive).
        /// </summary>
        /// <exception cref="ArgumentOutOfRangeException">Key is out of range.</exception>
        public sbyte Key
        {
            get { return _key; }
            set
            {
                if (value < MinKey || value > MaxKey)
                    throw new ArgumentOutOfRangeException(nameof(value), value, "Key is out of range.");

                _key = value;
            }
        }

        /// <summary>
        /// Gets or sets scale (0 for major or 1 for minor).
        /// </summary>
        /// <exception cref="ArgumentOutOfRangeException">Scale is out of range.</exception>
        public byte Scale
        {
            get { return _scale; }
            set
            {
                if (value < MinScale || value > MaxScale)
                    throw new ArgumentOutOfRangeException(nameof(value), value, "Scale is out of range.");

                _scale = value;
            }
        }

        #endregion

        #region Methods

        /// <summary>
        /// Determines whether the specified event is equal to the current one.
        /// </summary>
        /// <param name="keySignatureEvent">The event to compare with the current one.</param>
        /// <returns>true if the specified event is equal to the current one; otherwise, false.</returns>
        public bool Equals(KeySignatureEvent keySignatureEvent)
        {
            return Equals(keySignatureEvent, true);
        }

        /// <summary>
        /// Determines whether the specified event is equal to the current one.
        /// </summary>
        /// <param name="keySignatureEvent">The event to compare with the current one.</param>
        /// <param name="respectDeltaTime">If true the <see cref="MidiEvent.DeltaTime"/> will be taken into an account
        /// while comparing events; if false - delta-times will be ignored.</param>
        /// <returns>true if the specified event is equal to the current one; otherwise, false.</returns>
        public bool Equals(KeySignatureEvent keySignatureEvent, bool respectDeltaTime)
        {
            if (ReferenceEquals(null, keySignatureEvent))
                return false;

            if (ReferenceEquals(this, keySignatureEvent))
                return true;

            return base.Equals(keySignatureEvent, respectDeltaTime) &&
                   Key == keySignatureEvent.Key &&
                   Scale == keySignatureEvent.Scale;
        }

        private static int ProcessValue(int value, string property, int min, int max, InvalidMetaEventParameterValuePolicy policy)
        {
            if (value >= min && value <= max)
                return value;

            switch (policy)
            {
                case InvalidMetaEventParameterValuePolicy.Abort:
                    throw new InvalidMetaEventParameterValueException($"{value} is invalid value for the {property} of a Key Signature event.", value);
                case InvalidMetaEventParameterValuePolicy.SnapToLimits:
                    return Math.Min(Math.Max(value, min), max);
            }

            return value;
        }

        #endregion

        #region Overrides

        /// <summary>
        /// Reads content of a MIDI meta event.
        /// </summary>
        /// <param name="reader">Reader to read the content with.</param>
        /// <param name="settings">Settings according to which the event's content must be read.</param>
        /// <param name="size">Size of the event's content.</param>
        protected override void ReadContent(MidiReader reader, ReadingSettings settings, int size)
        {
            var invalidMetaEventParameterValuePolicy = settings.InvalidMetaEventParameterValuePolicy;

            Key = (sbyte)ProcessValue(reader.ReadSByte(),
                                       nameof(Key),
                                       MinKey,
                                       MaxKey,
                                       invalidMetaEventParameterValuePolicy);

            Scale = (byte)ProcessValue(reader.ReadByte(),
                                        nameof(Scale),
                                        MinScale,
                                        MaxScale,
                                        invalidMetaEventParameterValuePolicy);
        }

        /// <summary>
        /// Writes content of a MIDI meta event.
        /// </summary>
        /// <param name="writer">Writer to write the content with.</param>
        /// <param name="settings">Settings according to which the event's content must be written.</param>
        protected override void WriteContent(MidiWriter writer, WritingSettings settings)
        {
            writer.WriteSByte(Key);
            writer.WriteByte(Scale);
        }

        /// <summary>
        /// Gets the size of the content of a MIDI meta event.
        /// </summary>
        /// <returns>Size of the event's content.</returns>
        protected override int GetContentSize()
        {
            return 2;
        }

        /// <summary>
        /// Clones event by creating a copy of it.
        /// </summary>
        /// <returns>Copy of the event.</returns>
        protected override MidiEvent CloneEvent()
        {
            return new KeySignatureEvent(Key, Scale);
        }

        /// <summary>
        /// Returns a string that represents the current object.
        /// </summary>
        /// <returns>A string that represents the current object.</returns>
        public override string ToString()
        {
            return $"Key Signature ({Key}, {Scale})";
        }

        /// <summary>
        /// Determines whether the specified object is equal to the current object.
        /// </summary>
        /// <param name="obj">The object to compare with the current object.</param>
        /// <returns>true if the specified object is equal to the current object; otherwise, false.</returns>
        public override bool Equals(object obj)
        {
            return Equals(obj as KeySignatureEvent);
        }

        /// <summary>
        /// Serves as the default hash function.
        /// </summary>
        /// <returns>A hash code for the current object.</returns>
        public override int GetHashCode()
        {
            return base.GetHashCode() ^ Key.GetHashCode() ^ Scale.GetHashCode();
        }

        #endregion
    }
}
