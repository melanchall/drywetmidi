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

        /// <summary>
        /// Splits <see cref="MidiFile"/> by objects. More info in the
        /// <see href="xref:a_file_splitting#splitbyobjects">MIDI file splitting: SplitByObjects</see> article.
        /// </summary>
        /// <param name="midiFile"><see cref="MidiFile"/> to split.</param>
        /// <param name="objectType">Combination of desired types of objects to split by.</param>
        /// <param name="settings">Settings according to which notes should be detected and built.</param>
        /// <param name="objectDetectionSettings">Settings according to which objects should be detected and built.</param>
        /// <returns>Collection of <see cref="MidiFile"/> where each file contains objects as defined by
        /// <paramref name="settings"/>.</returns>
        /// <exception cref="ArgumentNullException"><paramref name="midiFile"/> is <c>null</c>.</exception>
        public static IEnumerable<MidiFile> SplitByObjects(
            this MidiFile midiFile,
            ObjectType objectType,
            SplitByObjectsSettings settings = null,
            ObjectDetectionSettings objectDetectionSettings = null)
        {
            ThrowIfArgument.IsNull(nameof(midiFile), midiFile);

            settings = settings ?? new SplitByObjectsSettings();

            var keySelector = settings.KeySelector ?? (obj => obj.GetObjectId());
            var writeToAllFilesPredicate = settings.WriteToAllFilesPredicate ?? (obj => false);

            var tempoMap = midiFile.GetTempoMap();
            var objects = midiFile.GetObjects(objectType, objectDetectionSettings);

            var objectsByKeys = new Dictionary<object, List<ITimedObject>>();
            var allFilesObjects = new List<ITimedObject>();

            foreach (var obj in objects)
            {
                if (settings.Filter?.Invoke(obj) == false)
                    continue;

                if (writeToAllFilesPredicate(obj))
                {
                    if (settings.AllFilesObjectsFilter?.Invoke(obj) == false)
                        continue;

                    allFilesObjects.Add(obj);

                    foreach (var objectsByKey in objectsByKeys.Values)
                    {
                        objectsByKey.Add(obj);
                    }
                }
                else
                {
                    var key = keySelector(obj) ?? obj.GetObjectId();

                    List<ITimedObject> objectsByKey;
                    if (!objectsByKeys.TryGetValue(key, out objectsByKey))
                    {
                        objectsByKeys.Add(key, objectsByKey = new List<ITimedObject>(allFilesObjects));
                    }

                    objectsByKey.Add(obj);
                }
            }

            return objectsByKeys
                .Values
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
