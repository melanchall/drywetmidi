using Melanchall.DryWetMidi.Common;
using System;
using System.Collections.Generic;

namespace Melanchall.DryWetMidi.Smf.Interaction
{
    public sealed class IntervalDefinition
    {
        #region Fields

        private static readonly Dictionary<SevenBitNumber, Dictionary<IntervalDirection, IntervalDefinition>> _cache =
            new Dictionary<SevenBitNumber, Dictionary<IntervalDirection, IntervalDefinition>>();

        #endregion

        #region Constants

        public static readonly IntervalDefinition Zero = FromSteps(0);
        public static readonly IntervalDefinition One = FromSteps(1);
        public static readonly IntervalDefinition Two = FromSteps(2);
        public static readonly IntervalDefinition Three = FromSteps(3);
        public static readonly IntervalDefinition Four = FromSteps(4);
        public static readonly IntervalDefinition Five = FromSteps(5);
        public static readonly IntervalDefinition Six = FromSteps(6);
        public static readonly IntervalDefinition Seven = FromSteps(7);
        public static readonly IntervalDefinition Eight = FromSteps(8);
        public static readonly IntervalDefinition Nine = FromSteps(9);
        public static readonly IntervalDefinition Ten = FromSteps(10);
        public static readonly IntervalDefinition Eleven = FromSteps(11);
        public static readonly IntervalDefinition Twelve = FromSteps(12);

        #endregion

        #region Constructor

        private IntervalDefinition(SevenBitNumber interval, IntervalDirection direction)
        {
            Interval = interval;
            Direction = direction;
        }

        #endregion

        #region Properties

        public SevenBitNumber Interval { get; }

        public IntervalDirection Direction { get; }

        public int Steps => Direction == IntervalDirection.Up
            ? Interval
            : -Interval;

        #endregion

        #region Methods

        public IntervalDefinition Up()
        {
            return Get(Interval, IntervalDirection.Up);
        }

        public IntervalDefinition Down()
        {
            return Get(Interval, IntervalDirection.Down);
        }

        public static IntervalDefinition Get(SevenBitNumber interval, IntervalDirection direction)
        {
            Dictionary<IntervalDirection, IntervalDefinition> intervalDefinitions;
            if (!_cache.TryGetValue(interval, out intervalDefinitions))
                _cache.Add(interval, intervalDefinitions = new Dictionary<IntervalDirection, IntervalDefinition>());

            IntervalDefinition intervalDefinition;
            if (!intervalDefinitions.TryGetValue(direction, out intervalDefinition))
                intervalDefinitions.Add(direction, intervalDefinition = new IntervalDefinition(interval, direction));

            return intervalDefinition;
        }

        public static IntervalDefinition GetUp(SevenBitNumber interval)
        {
            return Get(interval, IntervalDirection.Up);
        }

        public static IntervalDefinition GetDown(SevenBitNumber interval)
        {
            return Get(interval, IntervalDirection.Down);
        }

        public static IntervalDefinition FromSteps(int steps)
        {
            ThrowIfArgument.IsOutOfRange(nameof(steps),
                                         steps,
                                         -SevenBitNumber.MaxValue,
                                         SevenBitNumber.MaxValue,
                                         "Steps number is out of range.");

            return Get((SevenBitNumber)Math.Abs(steps),
                       Math.Sign(steps) < 0 ? IntervalDirection.Down : IntervalDirection.Up);
        }

        #endregion

        #region Operators

        public static implicit operator int(IntervalDefinition intervalDefinition)
        {
            return intervalDefinition.Steps;
        }

        public static implicit operator IntervalDefinition(SevenBitNumber interval)
        {
            return GetUp(interval);
        }

        public static bool operator ==(IntervalDefinition intervalDefinition1, IntervalDefinition intervalDefinition2)
        {
            if (ReferenceEquals(intervalDefinition1, intervalDefinition2))
                return true;

            if (ReferenceEquals(null, intervalDefinition1) || ReferenceEquals(null, intervalDefinition2))
                return false;

            return intervalDefinition1.Steps == intervalDefinition2.Steps;
        }

        public static bool operator !=(IntervalDefinition intervalDefinition1, IntervalDefinition intervalDefinition2)
        {
            return !(intervalDefinition1 == intervalDefinition2);
        }

        public static IntervalDefinition operator +(IntervalDefinition intervalDefinition, int steps)
        {
            ThrowIfArgument.IsNull(nameof(intervalDefinition), intervalDefinition);

            return FromSteps(intervalDefinition.Steps + steps);
        }

        public static IntervalDefinition operator -(IntervalDefinition intervalDefinition, int steps)
        {
            ThrowIfArgument.IsNull(nameof(intervalDefinition), intervalDefinition);

            return FromSteps(intervalDefinition.Steps - steps);
        }

        public static IntervalDefinition operator *(IntervalDefinition intervalDefinition, int multiplier)
        {
            ThrowIfArgument.IsNull(nameof(intervalDefinition), intervalDefinition);

            return FromSteps(intervalDefinition.Steps * multiplier);
        }

        public static IntervalDefinition operator /(IntervalDefinition intervalDefinition, int divisor)
        {
            ThrowIfArgument.IsNull(nameof(intervalDefinition), intervalDefinition);

            return FromSteps(intervalDefinition.Steps / divisor);
        }

        public static IntervalDefinition operator +(IntervalDefinition intervalDefinition)
        {
            ThrowIfArgument.IsNull(nameof(intervalDefinition), intervalDefinition);

            return intervalDefinition.Up();
        }

        public static IntervalDefinition operator -(IntervalDefinition intervalDefinition)
        {
            ThrowIfArgument.IsNull(nameof(intervalDefinition), intervalDefinition);

            return intervalDefinition.Down();
        }

        #endregion

        #region Overrides

        public override string ToString()
        {
            return $"{(Direction == IntervalDirection.Up ? "+" : "-")}{Interval}";
        }

        public override bool Equals(object obj)
        {
            return this == (obj as IntervalDefinition);
        }

        public override int GetHashCode()
        {
            return Steps.GetHashCode();
        }

        #endregion
    }
}
