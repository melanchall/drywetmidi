#if NETSTANDARD2_0

namespace System.Diagnostics.CodeAnalysis
{
    [AttributeUsage(AttributeTargets.Method, Inherited = false, AllowMultiple = false)]
    internal sealed class DoesNotReturnAttribute : Attribute
    {
        public DoesNotReturnAttribute() { }
    }
}

#endif