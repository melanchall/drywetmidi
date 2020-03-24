namespace Melanchall.DryWetMidi.Devices
{
    /// <summary>
    /// Holds volume value for an output MIDI device.
    /// </summary>
    public struct Volume
    {
        #region Constants

        /// <summary>
        /// Zero volume.
        /// </summary>
        public static readonly Volume Zero = new Volume();

        /// <summary>
        /// Maximum volume on left channel and muted right one.
        /// </summary>
        public static readonly Volume FullLeft = Left(ushort.MaxValue);

        /// <summary>
        /// Maximum volume on right channel and muted left one.
        /// </summary>
        public static readonly Volume FullRight = Right(ushort.MaxValue);

        #endregion

        #region Constructor

        /// <summary>
        /// Initializes a new instance of the <see cref="Volume"/> with specified value which
        /// will be applied to both left and right channels.
        /// </summary>
        /// <param name="volume">Value of the volume.</param>
        public Volume(ushort volume)
            : this(volume, volume)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="Volume"/> with specified separate
        /// values for left and right channels.
        /// </summary>
        /// <param name="leftVolume">Value of the volume for the left channel.</param>
        /// <param name="rightVolume">Value of the volume for the right channel.</param>
        public Volume(ushort leftVolume, ushort rightVolume)
        {
            LeftVolume = leftVolume;
            RightVolume = rightVolume;
        }

        #endregion

        #region Properties

        /// <summary>
        /// Gets value of the volume for the left channel.
        /// </summary>
        public ushort LeftVolume { get; }

        /// <summary>
        /// Gets value of the volume for the right channel.
        /// </summary>
        public ushort RightVolume { get; }

        #endregion

        #region Methods

        /// <summary>
        /// Creates an instance of the <see cref="Volume"/> which mutes the left channel
        /// and sets specified volume for the right one.
        /// </summary>
        /// <param name="volume">Value of the volume for the right channel.</param>
        /// <returns>An instance of the <see cref="Volume"/> which mutes the left channel
        /// and sets <paramref name="volume"/> for the right one.</returns>
        public static Volume Right(ushort volume)
        {
            return new Volume(0, volume);
        }

        /// <summary>
        /// Creates an instance of the <see cref="Volume"/> which mutes the right channel
        /// and sets specified volume for the left one.
        /// </summary>
        /// <param name="volume">Value of the volume for the left channel.</param>
        /// <returns>An instance of the <see cref="Volume"/> which mutes the right channel
        /// and sets <paramref name="volume"/> for the left one.</returns>
        public static Volume Left(ushort volume)
        {
            return new Volume(volume, 0);
        }

        #endregion

        #region Overrides

        /// <summary>
        /// Returns a string that represents the current object.
        /// </summary>
        /// <returns>A string that represents the current object.</returns>
        public override string ToString()
        {
            return $"L {LeftVolume} R {RightVolume}";
        }

        /// <summary>
        /// Determines whether the specified object is equal to the current object.
        /// </summary>
        /// <param name="obj">The object to compare with the current object.</param>
        /// <returns><c>true</c> if the specified object is equal to the current object; otherwise, <c>false</c>.</returns>
        public override bool Equals(object obj)
        {
            if (!(obj is Volume))
                return false;

            var volume = (Volume)obj;
            return volume.LeftVolume == LeftVolume &&
                   volume.RightVolume == RightVolume;
        }

        /// <summary>
        /// Serves as the default hash function.
        /// </summary>
        /// <returns>A hash code for the current object.</returns>
        public override int GetHashCode()
        {
            unchecked
            {
                var result = 17;
                result = result * 23 + LeftVolume.GetHashCode();
                result = result * 23 + RightVolume.GetHashCode();
                return result;
            }
        }

        #endregion
    }
}
