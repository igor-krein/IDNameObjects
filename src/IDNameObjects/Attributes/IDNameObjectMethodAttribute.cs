using System;

namespace IDNameObjects
{
    [AttributeUsage(AttributeTargets.Method)]
    public class IDNameObjectMethodAttribute : Attribute
    {
        private string name;

        public IDNameObjectMethodAttribute(string name)
        {
            this.name = name;
        }

        public virtual string Name { get { return name; } }
    }
}
