using System.Collections.Generic;

namespace Melanchall.DryWetMidi.Smf.Interaction
{
    public sealed class ChordsCollectionChangedEventArgs
    {
        #region Constructor

        public ChordsCollectionChangedEventArgs(IEnumerable<Chord> addedChords, IEnumerable<Chord> removedChords)
        {
            AddedChords = addedChords;
            RemovedChords = removedChords;
        }

        #endregion

        #region Properties

        public IEnumerable<Chord> AddedChords { get; }

        public IEnumerable<Chord> RemovedChords { get; }

        #endregion
    }

    public delegate void ChordsCollectionChangedEventHandler(ChordsCollection collection, ChordsCollectionChangedEventArgs args);
}
