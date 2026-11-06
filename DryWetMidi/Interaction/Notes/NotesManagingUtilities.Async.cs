using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using Melanchall.DryWetMidi.Core;

namespace Melanchall.DryWetMidi.Interaction
{
    public static partial class NotesManagingUtilities
    {
        internal static async IAsyncEnumerable<ITimedObject> GetNotesAndTimedEventsAsync(
            this IAsyncEnumerable<ITimedObject> timedObjects,
            NoteDetectionSettings settings,
            [EnumeratorCancellation] CancellationToken cancellationToken)
        {
            var constructor = settings.Constructor;
            var objectsDescriptors = new List<PendingObjectDescriptor>();
            var notesDescriptorsNodes = new Dictionary<int, NoteOnEntry>();
            var firstDescriptorIndex = 0;
            var headDescriptorIndex = 0;
            var nextDescriptorIndex = 0;

            void CompactObjectsDescriptors()
            {
                var removeCount = headDescriptorIndex - firstDescriptorIndex;
                if (removeCount == 0)
                    return;

                if (removeCount == objectsDescriptors.Count)
                {
                    objectsDescriptors.Clear();
                    firstDescriptorIndex = headDescriptorIndex;
                    return;
                }

                if (removeCount < 128 && removeCount * 2 < objectsDescriptors.Count)
                    return;

                objectsDescriptors.RemoveRange(0, removeCount);
                firstDescriptorIndex = headDescriptorIndex;
            }

            await foreach (var timedObject in timedObjects.WithCancellation(cancellationToken).ConfigureAwait(false))
            {
                cancellationToken.ThrowIfCancellationRequested();
                var timedEvent = (TimedEvent)timedObject;

                switch (timedEvent.Event.EventType)
                {
                    case MidiEventType.NoteOn:
                        {
                            var noteId = ((NoteOnEvent)timedEvent.Event).GetNoteId();
                            var noteDescriptorIndex = nextDescriptorIndex;
                            objectsDescriptors.Add(PendingObjectDescriptor.CreateNote(timedEvent));
                            nextDescriptorIndex++;

                            if (!notesDescriptorsNodes.TryGetValue(noteId, out var entry))
                            {
                                notesDescriptorsNodes[noteId] = new NoteOnEntry { SingleIndex = noteDescriptorIndex };
                            }
                            else if (entry.Holder == null)
                            {
                                notesDescriptorsNodes[noteId] = new NoteOnEntry
                                {
                                    Holder = NoteOnIndexesHolder.Create(
                                        settings.NoteStartDetectionPolicy,
                                        entry.SingleIndex,
                                        noteDescriptorIndex)
                                };
                            }
                            else
                            {
                                entry.Holder.Add(noteDescriptorIndex);
                            }
                        }
                        break;
                    case MidiEventType.NoteOff:
                        {
                            var noteId = ((NoteOffEvent)timedEvent.Event).GetNoteId();
                            var noteDescriptorIndex = -1;

                            if (notesDescriptorsNodes.TryGetValue(noteId, out var entry))
                            {
                                if (entry.Holder == null)
                                {
                                    if (entry.SingleIndex >= headDescriptorIndex)
                                        noteDescriptorIndex = entry.SingleIndex;

                                    notesDescriptorsNodes.Remove(noteId);
                                }
                                else
                                {
                                    var holder = entry.Holder;

                                    while (holder.Count > 0)
                                    {
                                        var candidateDescriptorIndex = holder.GetNext();
                                        if (candidateDescriptorIndex >= headDescriptorIndex)
                                        {
                                            noteDescriptorIndex = candidateDescriptorIndex;
                                            break;
                                        }
                                    }

                                    if (holder.Count == 0)
                                        notesDescriptorsNodes.Remove(noteId);
                                }
                            }

                            if (noteDescriptorIndex < 0)
                            {
                                if (headDescriptorIndex == nextDescriptorIndex)
                                    yield return timedEvent;
                                else
                                {
                                    objectsDescriptors.Add(PendingObjectDescriptor.CreateTimedEvent(timedEvent));
                                    nextDescriptorIndex++;
                                }

                                break;
                            }

                            var descriptorIndex = noteDescriptorIndex - firstDescriptorIndex;
                            var noteDescriptor = objectsDescriptors[descriptorIndex];
                            noteDescriptor.NoteOffTimedEvent = timedEvent;
                            objectsDescriptors[descriptorIndex] = noteDescriptor;

                            if (noteDescriptorIndex != headDescriptorIndex)
                                break;

                            for (; headDescriptorIndex < nextDescriptorIndex; headDescriptorIndex++)
                            {
                                cancellationToken.ThrowIfCancellationRequested();
                                var completedDescriptor = objectsDescriptors[headDescriptorIndex - firstDescriptorIndex];
                                if (!completedDescriptor.IsCompleted)
                                    break;

                                yield return completedDescriptor.GetObject(constructor);
                            }

                            CompactObjectsDescriptors();
                        }
                        break;
                    default:
                        {
                            if (headDescriptorIndex == nextDescriptorIndex)
                                yield return timedEvent;
                            else
                            {
                                objectsDescriptors.Add(PendingObjectDescriptor.CreateTimedEvent(timedEvent));
                                nextDescriptorIndex++;
                            }
                        }
                        break;
                }
            }

            for (; headDescriptorIndex < nextDescriptorIndex; headDescriptorIndex++)
            {
                cancellationToken.ThrowIfCancellationRequested();
                yield return objectsDescriptors[headDescriptorIndex - firstDescriptorIndex].GetObject(constructor);
            }
        }
    }
}
