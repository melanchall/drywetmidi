using Melanchall.DryWetMidi.Core;
using Melanchall.DryWetMidi.Interaction;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Melanchall.DryWetMidi.Tools
{
    public static partial class Splitter
    {
        #region Methods

        private static MidiFile PrepareMidiFileForSlicing(MidiFile midiFile, IGrid grid, SliceMidiFileSettings settings)
        {
            if (settings.SplitNotes)
            {
                midiFile = midiFile.Clone();
                midiFile.SplitNotesByGrid(grid, settings.NoteDetectionSettings);
            }

            return midiFile;
        }

        // TODO: objects filter
        private static void SplitTrackChunkObjects(
            TrackChunk trackChunk,
            ObjectType objectType,
            ObjectDetectionSettings objectDetectionSettings,
            Func<IEnumerable<ITimedObject>, IEnumerable<ITimedObject>> splitOperation)
        {
            using (var objectsManager = new TimedObjectsManager(trackChunk.Events, objectType, objectDetectionSettings))
            {
                var objects = objectsManager.Objects;
                var newObjects = splitOperation(objects).ToList();

                objects.Clear();
                objects.Add(newObjects);
            }
        }

        private static ILengthedObject GetZeroLengthObjectAtStart(ILengthedObject baseObject, long time)
        {
            var chord = baseObject as Chord;
            if (chord != null)
                return new Chord(chord.Notes.Where(n => n.Time == time).Select(n =>
                {
                    var note = (Note)n.Clone();
                    note.Length = 0;
                    return note;
                }));

            var result = (ILengthedObject)baseObject.Clone();
            result.Length = 0;
            return result;
        }

        private static ILengthedObject GetZeroLengthObjectAtEnd(ILengthedObject baseObject, long time)
        {
            var chord = baseObject as Chord;
            if (chord != null)
                return new Chord(chord.Notes.Where(n => n.Time + n.Length == time).Select(n =>
                {
                    var note = (Note)n.Clone();
                    note.Time = time;
                    note.Length = 0;
                    return note;
                }));

            var result = (ILengthedObject)baseObject.Clone();
            result.Time = time;
            result.Length = 0;
            return result;
        }

        #endregion
    }
}
