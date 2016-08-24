using System;

namespace IDNameObjects
{
    [AttributeUsage(AttributeTargets.Property)]
    public class IDNameObjectPropertyAttribute : Attribute
    {
        public string Name { get; private set; }

        public IDNameObjectPropertyAttribute(string name = null)
        {
            Name = name;
        }
    }
}
