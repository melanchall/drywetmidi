namespace Melanchall.DryWetMidi.Common
{
    internal sealed class Random
    {
        #region Fields

        private static volatile global::System.Random? _instance;

#if NET9_0_OR_GREATER
        private static readonly System.Threading.Lock _lockObject = new();
#else
        private static readonly object _lockObject = new();
#endif

        #endregion

        #region Constructor

        private Random()
        {
        }

        #endregion

        #region Properties

        public static global::System.Random Instance
        {
            get
            {
                if (_instance == null)
                {
                    lock (_lockObject)
                    {
                        if (_instance == null)
                        {
                            _instance = new global::System.Random();
                        }
                    }
                }

                return _instance;
            }
        }

        #endregion
    }
}
