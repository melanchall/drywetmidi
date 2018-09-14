namespace Melanchall.DryWetMidi.Devices
{
    public struct Volume
    {
        #region Constants

        public static readonly Volume Zero = new Volume();
        public static readonly Volume FullLeft = Left(ushort.MaxValue);
        public static readonly Volume FullRight = Right(ushort.MaxValue);

        #endregion

        #region Constructor

        public Volume(ushort volume)
            : this(volume, volume)
        {
        }

        public Volume(ushort leftVolume, ushort rightVolume)
        {
            LeftVolume = leftVolume;
            RightVolume = rightVolume;
        }

        #endregion

        #region Properties

        public ushort LeftVolume { get; }

        public ushort RightVolume { get; }

        #endregion

        #region Methods

        public static Volume Right(ushort volume)
        {
            return new Volume(0, volume);
        }

        public static Volume Left(ushort volume)
        {
            return new Volume(volume, 0);
        }

        #endregion

        #region Overrides

        public override string ToString()
        {
            return $"L {LeftVolume} R {RightVolume}";
        }

        public override bool Equals(object obj)
        {
            if (!(obj is Volume))
                return false;

            var volume = (Volume)obj;
            return volume.LeftVolume == LeftVolume &&
                   volume.RightVolume == RightVolume;
        }

        public override int GetHashCode()
        {
            return LeftVolume ^ RightVolume;
        }

        #endregion
    }
}
