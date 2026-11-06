using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using Melanchall.DryWetMidi.Common;

namespace Melanchall.DryWetMidi.Interaction
{
    public static partial class ChordsManagingUtilities
    {
        internal static async IAsyncEnumerable<ITimedObject> GetChordsAndNotesAndTimedEventsAsync(
            this IAsyncEnumerable<ITimedObject> notesAndTimedEvents,
            ChordDetectionSettings settings,
            [EnumeratorCancellation] CancellationToken cancellationToken)
        {
            var constructor = settings.Constructor;
            var timedObjects = new LinkedList<ObjectDescriptor>();
            var chordsDescriptors = new LinkedList<ChordDescriptor>();
            var chordsDescriptorsByChannel = new LinkedListNode<ChordDescriptor>[FourBitNumber.MaxValue + 1];

            await foreach (var timedObject in notesAndTimedEvents.WithCancellation(cancellationToken).ConfigureAwait(false))
            {
                cancellationToken.ThrowIfCancellationRequested();

                if (timedObject is TimedEvent timedEvent)
                {
                    if (timedObjects.Count == 0)
                        yield return timedObject;
                    else
                        timedObjects.AddLast(new TimedEventDescriptor(timedEvent));

                    continue;
                }

                var note = (Note)timedObject;
                var chordDescriptorNode = chordsDescriptorsByChannel[note.Channel];

                if (timedObjects.Count == 0 || chordDescriptorNode == null || chordDescriptorNode.List == null)
                {
                    CreateChordDescriptor(chordsDescriptors, chordsDescriptorsByChannel, timedObjects, note, settings);
                }
                else
                {
                    var chordDescriptor = chordDescriptorNode.Value;
                    if (CanNoteBeAddedToChord(chordDescriptor, note, settings.NotesTolerance))
                    {
                        var noteNode = timedObjects.AddLast(new NoteDescriptor(note, false)
                        {
                            ChordDescriptor = chordDescriptor
                        });
                        chordDescriptor.NotesNodes.Add(noteNode);
                    }
                    else
                    {
                        chordDescriptor.IsSealed = true;

                        if (chordDescriptorNode.Previous == null)
                        {
                            foreach (var detectedObject in GetTimedObjects(
                                chordDescriptorNode,
                                chordsDescriptors,
                                timedObjects,
                                true,
                                constructor))
                            {
                                cancellationToken.ThrowIfCancellationRequested();
                                yield return detectedObject;
                            }
                        }

                        CreateChordDescriptor(chordsDescriptors, chordsDescriptorsByChannel, timedObjects, note, settings);
                    }
                }
            }

            cancellationToken.ThrowIfCancellationRequested();

            if (chordsDescriptors.First != null)
            {
                foreach (var detectedObject in GetTimedObjects(
                    chordsDescriptors.First,
                    chordsDescriptors,
                    timedObjects,
                    false,
                    constructor))
                {
                    cancellationToken.ThrowIfCancellationRequested();
                    yield return detectedObject;
                }
            }
        }
    }
}
