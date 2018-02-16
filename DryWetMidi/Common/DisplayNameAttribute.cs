using System;

namespace Melanchall.DryWetMidi.Common
{
    internal sealed class DisplayNameAttribute : Attribute
    {
        #region Constructor

        public DisplayNameAttribute(string name)
        {
            Name = name;
        }

        #endregion

        #region Properties

        public string Name { get; }

        #endregion
    }
}
