namespace Melanchall.DryWetMidi.Common
{
    internal sealed class Random
    {
        #region Fields

        private static volatile System.Random _instance;
        private static readonly object _lockObject = new object();

        #endregion

        #region Constructor

        private Random()
        {
        }

        #endregion

        #region Properties

        public static System.Random Instance
        {
            get
            {
                if (_instance == null)
                {
                    lock (_lockObject)
                    {
                        if (_instance == null)
                        {
                            _instance = new System.Random();
                        }
                    }
                }

                return _instance;
            }
        }

        #endregion
    }
}
