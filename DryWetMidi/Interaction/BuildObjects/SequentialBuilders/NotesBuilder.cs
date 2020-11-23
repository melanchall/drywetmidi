using System.Collections.Generic;

namespace Melanchall.DryWetMidi.Interaction
{
    internal sealed class NotesBuilder : SequentialObjectsBuilder<NotesBag, NotesContext>
    {
        #region Constructors

        public NotesBuilder(List<ObjectsBag> objectsBags, ObjectsBuildingSettings settings)
            : base(objectsBags, settings)
        {
        }

        #endregion
    }
}
