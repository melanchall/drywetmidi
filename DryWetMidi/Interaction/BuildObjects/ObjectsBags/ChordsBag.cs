using System.Collections.Generic;
using System.Linq;
using Melanchall.DryWetMidi.Common;

namespace Melanchall.DryWetMidi.Interaction
{
    internal sealed class ChordsBag : ObjectsBag
    {
        #region Fields

        private readonly List<NotesBag> _notesBags = new List<NotesBag>();

        private long _chordStart = -1;
        private FourBitNumber _chordChannel;
        private bool _canObjectsBeAdded = true;

        #endregion

        #region Properties

        public override bool IsCompleted
        {
            get
            {
                if (!_notesBags.Any())
                    return _timedObjects.Any();

                return _notesBags.All(b => b.IsCompleted);
            }
        }

        public override bool CanObjectsBeAdded => _canObjectsBeAdded;

        #endregion

        #region Methods

        public override IEnumerable<ITimedObject> GetObjects()
        {
            var result = base.GetObjects();
            return _notesBags.Any()
                ? result.Concat(new[] { new Chord(_notesBags.SelectMany(b => b.GetObjects()).OfType<Note>()) })
                : result;
        }

        public override IEnumerable<ITimedObject> GetRawObjects()
        {
            return _notesBags.SelectMany(b => b.GetRawObjects());
        }

        public override bool TryAddObject(ITimedObject timedObject, IBuildingContext context, ObjectsBuildingSettings settings)
        {
            if (!CanObjectsBeAdded)
                return false;

            return TryAddTimedEvent(timedObject as TimedEvent, settings) ||
                   TryAddNote(timedObject as Note, settings) ||
                   TryAddChord(timedObject as Chord);
        }

        private bool TryAddTimedEvent(TimedEvent timedEvent, ObjectsBuildingSettings settings)
        {
            if (timedEvent == null)
                return false;

            var handlingBag = _notesBags.FirstOrDefault(b => b.TryAddObject(timedEvent, null, settings));
            if (handlingBag != null)
                return true;

            return TryAddObjectToNewNoteBag(timedEvent, settings);
        }

        private bool TryAddNote(Note note, ObjectsBuildingSettings settings)
        {
            if (note == null)
                return false;

            return TryAddObjectToNewNoteBag(note, settings);
        }

        private bool TryAddChord(Chord chord)
        {
            if (chord == null)
                return false;

            _timedObjects.Add(chord);
            return true;
        }

        private bool TryAddObjectToNewNoteBag(ITimedObject timedObject, ObjectsBuildingSettings settings)
        {
            var bag = new NotesBag();
            if (!bag.TryAddObject(timedObject, null, settings))
                return false;

            var newNoteTime = bag.Time;
            var newNoteChannel = bag.NoteId.Channel;

            if (_chordStart < 0)
            {
                _notesBags.Add(bag);
                _chordStart = newNoteTime;
                _chordChannel = newNoteChannel;
                return true;
            }
            else
            {
                if (newNoteTime - _chordStart > settings.ChordBuilderSettings.NotesTolerance || newNoteChannel != _chordChannel)
                {
                    _canObjectsBeAdded = !IsCompleted;
                    return false;
                }

                _notesBags.Add(bag);
                return true;
            }
        }

        #endregion
    }
}
