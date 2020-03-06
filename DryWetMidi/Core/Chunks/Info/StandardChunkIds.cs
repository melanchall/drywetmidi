namespace Melanchall.DryWetMidi.Core
{
    internal static class StandardChunkIds
    {
        #region Methods

        public static string[] GetIds()
        {
            return new[]
            {
                HeaderChunk.Id,
                TrackChunk.Id
            };
        }

        #endregion
    }
}
