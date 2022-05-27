using Melanchall.DryWetMidi.Common;
using Melanchall.DryWetMidi.Interaction;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Melanchall.DryWetMidi.Tools
{
    public class ObjectsMerger
    {
        #region Fields

        protected readonly List<ILengthedObject> _objects = new List<ILengthedObject>();
        protected readonly ObjectType _objectsType;

        #endregion

        #region Constructor

        public ObjectsMerger(ILengthedObject obj)
        {
            _objects.Add(obj);
            _objectsType = GetObjectsType();
        }

        #endregion

        #region Properties

        public long EndTime
        {
            get
            {
                var lastObject = _objects.LastOrDefault();
                return (lastObject?.Time ?? 0) + (lastObject?.Length ?? 0);
            }
        }

        #endregion

        #region Methods

        public void AddObject(ILengthedObject obj)
        {
            _objects.Add(obj);
        }

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

        public virtual ILengthedObject MergeObjects(ObjectsMergingSettings settings)
        {
            if (_objects.Count == 1)
                return (ILengthedObject)_objects.First().Clone();

            switch (_objectsType)
            {
                case ObjectType.Note:
                    return MergeNotes(settings);
                case ObjectType.Rest:
                    return MergeRests(settings);
                case ObjectType.Chord:
                    return MergeChords(settings);
            }

            throw new NotImplementedException($"Merging objects of {_objectsType} type is not implemented.");
        }

        private Note MergeNotes(ObjectsMergingSettings settings)
        {
            return MergeNotes(_objects.Cast<Note>(), settings);
        }

        private Rest MergeRests(ObjectsMergingSettings settings)
        {
            var result = (Rest)_objects.First().Clone();
            var lastRest = _objects.Last();
            
            result.Length = lastRest.Time + lastRest.Length - result.Time;
            return result;
        }

        private Chord MergeChords(ObjectsMergingSettings settings)
        {
            var result = new Chord();
            var notesCount = ((Chord)_objects.First()).Notes.Count;

            return new Chord(Enumerable
                .Range(0, notesCount)
                .Select(i => MergeNotes(_objects.Select(o => ((Chord)o).Notes[i]), settings)));
        }

        private static Note MergeNotes(IEnumerable<Note> notes, ObjectsMergingSettings settings)
        {
            var result = (Note)notes.First().Clone();
            var lastNote = notes.Last();

            result.Length = lastNote.Time + lastNote.Length - result.Time;
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

        private ObjectType GetObjectsType()
        {
            var firstObject = _objects.First();

            if (firstObject is Note)
                return ObjectType.Note;
            if (firstObject is Chord)
                return ObjectType.Chord;
            if (firstObject is Rest)
                return ObjectType.Rest;

            throw new NotImplementedException($"Getting object type for {firstObject} is not implemented.");
        }

        #endregion
    }
}
