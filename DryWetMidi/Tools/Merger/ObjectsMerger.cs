using Melanchall.DryWetMidi.Common;
using Melanchall.DryWetMidi.Interaction;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Melanchall.DryWetMidi.Tools
{
    /// <summary>
    /// Provides merging logic for group of objects. More info in the <see href="xref:a_merger">Merger</see> article.
    /// </summary>
    public class ObjectsMerger
    {
        #region Fields

        /// <summary>
        /// Collection of objects that should be merged.
        /// </summary>
        protected readonly List<ILengthedObject> _objects = new List<ILengthedObject>();
        
        /// <summary>
        /// The type of objects in the current group.
        /// </summary>
        protected readonly Type _objectsType;

        #endregion

        #region Constructor

        /// <summary>
        /// Initializes a new instance of the <see cref="ObjectsMerger"/> with the specified first object.
        /// </summary>
        /// <param name="obj">First object of the current group of objects that should be merged.</param>
        public ObjectsMerger(ILengthedObject obj)
        {
            _objects.Add(obj);
            _objectsType = GetObjectsType();
        }

        #endregion

        #region Properties

        /// <summary>
        /// Gets the end time of the last object in the current group.
        /// </summary>
        public long EndTime => _objects.LastOrDefault()?.EndTime ?? 0;

        #endregion

        #region Methods

        /// <summary>
        /// Adds the specified object to the current group.
        /// </summary>
        /// <param name="obj">Object to add to the current group</param>
        public void AddObject(ILengthedObject obj)
        {
            _objects.Add(obj);
        }

        /// <summary>
        /// Returns a value indicating whether the specified object can be added to the current group or not.
        /// In other words, the method determines if the object should be merged with the group.
        /// </summary>
        /// <param name="obj">Object to check.</param>
        /// <param name="tempoMap">Tempo map used for merging.</param>
        /// <param name="settings">Settings according to which merging process should be done.</param>
        /// <returns><c>true</c> if the <paramref name="obj"/> should be added to the current group;
        /// otherwise, <c>false</c>.</returns>
        public virtual bool CanAddObject(ILengthedObject obj, TempoMap tempoMap, ObjectsMergingSettings settings)
        {
            var currentEndTime = EndTime;

            var distance = Math.Max(0, obj.Time - currentEndTime);
            var convertedDistance = LengthConverter.ConvertTo(
                (MidiTimeSpan)distance,
                settings.Tolerance.GetType(),
                currentEndTime,
                tempoMap);

            return convertedDistance.CompareTo(settings.Tolerance) <= 0;
        }

        /// <summary>
        /// Merges objects within the current group into single object.
        /// </summary>
        /// <param name="settings">Settings according to which merging process should be done.</param>
        /// <returns>A new object that is result of merging objects within the current group.</returns>
        public virtual ILengthedObject MergeObjects(ObjectsMergingSettings settings)
        {
            if (_objects.Count == 1)
                return (ILengthedObject)_objects.First().Clone();

            if (_objectsType == typeof(Note))
                return MergeNotes(settings);

            if (_objectsType == typeof(Chord))
                return MergeChords(settings);

            if (_objectsType == typeof(Rest))
                return MergeRests();

            throw new NotImplementedException($"Merging objects of {_objectsType} type is not implemented.");
        }

        private Note MergeNotes(ObjectsMergingSettings settings)
        {
            return MergeNotes(_objects.Cast<Note>(), settings);
        }

        private Rest MergeRests()
        {
            var result = (Rest)_objects.First().Clone();
            var lastRest = _objects.Last();
            
            result.Length = lastRest.EndTime - result.Time;
            return result;
        }

        private Chord MergeChords(ObjectsMergingSettings settings)
        {
            var notesCount = ((Chord)_objects.First()).Notes.Count;

            return new Chord(Enumerable
                .Range(0, notesCount)
                .Select(i => MergeNotes(_objects.Select(o => ((Chord)o).Notes.ElementAt(i)), settings)));
        }

        private static Note MergeNotes(IEnumerable<Note> notes, ObjectsMergingSettings settings)
        {
            var result = (Note)notes.First().Clone();
            var lastNote = notes.Last();

            result.Length = lastNote.EndTime - result.Time;
            result.Velocity = MergeVelocities(notes.Select(n => n.Velocity), settings.VelocityMergingPolicy);
            result.OffVelocity = MergeVelocities(notes.Select(n => n.OffVelocity), settings.OffVelocityMergingPolicy);

            return result;
        }

        private static SevenBitNumber MergeVelocities(
            IEnumerable<SevenBitNumber> velocities,
            VelocityMergingPolicy velocityMergingPolicy)
        {
            switch (velocityMergingPolicy)
            {
                case VelocityMergingPolicy.First:
                    return velocities.First();
                case VelocityMergingPolicy.Last:
                    return velocities.Last();
                case VelocityMergingPolicy.Min:
                    return velocities.Min();
                case VelocityMergingPolicy.Max:
                    return velocities.Max();
                case VelocityMergingPolicy.Average:
                    return (SevenBitNumber)MathUtilities.Round(velocities.Average(v => v));
            }

            throw new NotImplementedException($"Merging velocities by {velocityMergingPolicy} policy is not implemented.");
        }

        private Type GetObjectsType()
        {
            return _objects.First().GetType();
        }

        #endregion
    }
}
