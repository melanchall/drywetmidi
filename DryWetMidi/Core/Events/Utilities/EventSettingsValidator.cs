using System;
using System.Collections.Generic;
using System.Linq;

namespace Melanchall.DryWetMidi.Core
{
    internal static class EventSettingsValidator
    {
        #region Methods

        internal static void ValidateCustomMetaEventsStatusBytes(EventTypesCollection customMetaEventTypesCollection)
        {
            if (customMetaEventTypesCollection == null)
                return;

            var invalidTypes = new List<EventType>();
            Type eventType;

            foreach (var statusByte in customMetaEventTypesCollection.Select(t => t.StatusByte))
            {
                if (StandardEventTypes.Meta.TryGetType(statusByte, out eventType))
                    invalidTypes.Add(new EventType(eventType, statusByte));
            }

            if (invalidTypes.Any())
                throw new InvalidOperationException("Following custom meta events status bytes correspond to standard ones: " +
                    string.Join(", ", invalidTypes.Select(t => $"{t.StatusByte} -> {t.Type.Name}")));
        }

        #endregion
    }
}
