using System.Collections.Generic;

namespace IDNameObjects
{
    public class Select2Object
    {
        public string id { get; internal set; }
        public string text { get; internal set; }
    }

    public class Select2PageResult
    {
        public IEnumerable<Select2Object> items { get; internal set; }
        public bool more { get; internal set; }
    }
}
