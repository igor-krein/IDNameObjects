using System;

namespace IDNameObjects
{
    [AttributeUsage(AttributeTargets.Class)]
    public class IDNameObjectAttribute : Attribute
    {
        private string nameSearchType = "Contains";
        private int pageSize = 10;

        public string NameSearchType
        {
            get { return nameSearchType; }
            set
            {
                nameSearchType = (value.ToLower() == "startswith") ? "StartsWith" : "Contains";
            }
        }

        public int PageSize
        {
            get { return pageSize; }
            set
            {
                pageSize = (value <= 0) ? 10 : value;
            }
        }

    }
}
