namespace Melanchall.DryWetMidi.Core
{
    internal static class StandardChunkIds
    {
        #region Fields

        private static string[] _ids;

        #endregion

        #region Methods

        public static string[] GetIds()
        {
            return _ids ?? (_ids = new[] { HeaderChunk.Id, TrackChunk.Id });
        }

        #endregion
    }
}
