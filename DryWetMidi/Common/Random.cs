using System;

namespace Melanchall.DryWetMidi.Common
{
    internal sealed class Random
    {
        private static readonly Lazy<System.Random> _instance =
            new (() => new System.Random());

        private Random()
        {
        }

        public static System.Random Instance => _instance.Value;
    }
}
