using ObjectExtensions;
using IDNameObjects.Engine;

namespace IDNameObjects
{
    public class SimpleIDNameObject
    {
        // TODO(?): add support for composite keys
        public object ID { get; internal set; }
        public string Name { get; internal set; }
    }

    public class IDNameObject<TEntity> : SimpleIDNameObject
        where TEntity : class
    {

        public string NameDB { get; internal set; }

        public IDNameObject(TEntity ent)
        {
            //            var t = type.IDProperty.PropertyType;
            ID = ent.GetPropertyValue(IDNameObjectType<TEntity>.IDProperty.Name);
            Name = (string)ent.GetPropertyValue(IDNameObjectType<TEntity>.NameProperty.Name);
            NameDB = (string)ent.GetPropertyValue(IDNameObjectType<TEntity>.NameDBProperty.Name);
        }
    }
}
