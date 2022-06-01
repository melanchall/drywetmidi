using Melanchall.DryWetMidi.Common;
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

        public static IEnumerable<MidiFile> SplitByObjects<TKey>(
            this MidiFile midiFile,
            ObjectType objectType,
            Func<ITimedObject, TKey> keySelector,
            Predicate<ITimedObject> writeToAllFilesPredicate,
            SplitByObjectsSettings settings = null,
            ObjectDetectionSettings objectDetectionSettings = null)
        {
            ThrowIfArgument.IsNull(nameof(midiFile), midiFile);
            ThrowIfArgument.IsNull(nameof(keySelector), keySelector);
            ThrowIfArgument.IsNull(nameof(writeToAllFilesPredicate), writeToAllFilesPredicate);

            settings = settings ?? new SplitByObjectsSettings();

            var tempoMap = midiFile.GetTempoMap();
            var objects = midiFile.GetObjects(objectType, objectDetectionSettings);

            var objectsByKeys = new Dictionary<TKey, List<ITimedObject>>();
            var allFilesObjects = new List<ITimedObject>();

            List<ITimedObject> nullKeyObjects = null;

            foreach (var obj in objects)
            {
                if (settings.Filter?.Invoke(obj) == false)
                    continue;

                var key = keySelector(obj);
                if (writeToAllFilesPredicate(obj))
                {
                    if (settings.AllFilesObjectsFilter?.Invoke(obj) == false)
                        continue;

                    allFilesObjects.Add(obj);

                    foreach (var objectsByKey in objectsByKeys.Values)
                    {
                        objectsByKey.Add(obj);
                    }

                    nullKeyObjects?.Add(obj);
                }
                else
                {
                    List<ITimedObject> objectsByKey;

                    if (key == null)
                    {
                        if (nullKeyObjects == null)
                            nullKeyObjects = new List<ITimedObject>(allFilesObjects);

                        objectsByKey = nullKeyObjects;
                    }
                    else if (!objectsByKeys.TryGetValue(key, out objectsByKey))
                    {
                        objectsByKeys.Add(key, objectsByKey = new List<ITimedObject>(allFilesObjects));
                    }

                    objectsByKey.Add(obj);
                }
            }

            return objectsByKeys
                .Values
                .Concat(new[] { nullKeyObjects ?? Enumerable.Empty<ITimedObject>() })
                .Where(objectsByKey => objectsByKey.Any())
                .Select(objectsByKey =>
                {
                    var file = objectsByKey.ToFile();
                    file.TimeDivision = midiFile.TimeDivision.Clone();
                    file.ReplaceTempoMap(tempoMap);
                    return file;
                });
        }

        #endregion
    }
}
