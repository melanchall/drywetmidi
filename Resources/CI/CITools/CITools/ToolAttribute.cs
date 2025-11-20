namespace CITools
{
    [AttributeUsage(AttributeTargets.Class)]
    internal sealed class ToolAttribute : Attribute
    {
        public ToolAttribute(string name)
        {
            Name = name;
        }

        public string Name { get; }
    }
}
