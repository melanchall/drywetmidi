namespace Melanchall.DryWetMidi.Interaction
{
    /// <summary>
    /// The type of a parameter's value.
    /// </summary>
    public enum ParameterValueType
    {
        /// <summary>
        /// Parameter's value represents exact value of the parameter.
        /// </summary>
        Exact = 0,

        /// <summary>
        /// Parameter's value represents a value which should be added to the current
        /// value of the parameter.
        /// </summary>
        Increment,

        /// <summary>
        /// Parameter's value represents a value which should be subtracted from the current
        /// value of the parameter.
        /// </summary>
        Decrement
    }
}
