using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Web.Mvc;
using IDNameObjects.Engine;

namespace IDNameObjects.Mvc
{
    internal class SelectListManager<TEntity>
        where TEntity : class
    {
        internal static IQueryable<TEntity> Query(IQueryable<TEntity> set, Expression<Func<TEntity, object>> customOrderBySelector = null)
        {
            return (customOrderBySelector == null) ? set.DefaultOrder() : set.OrderBy(customOrderBySelector);
        }

        internal static IQueryable<TEntity> Query(IQueryable<TEntity> set, object selectedID)
        {
            return set.WhereID(selectedID);
        }

        internal static IQueryable<SimpleIDNameObject> SimpleINOQuery(IQueryable<TEntity> set, Expression<Func<TEntity, object>> customOrderBySelector = null)
        {
            return Query(set, customOrderBySelector).AsSimpleINOs();
        }

        internal static IQueryable<SimpleIDNameObject> SimpleINOQuery(IQueryable<TEntity> set, object selectedID)
        {
            return Query(set, selectedID).AsSimpleINOs();
        }

        internal static SelectList FromList(IList<TEntity> list)
        {
            if (typeof(TEntity) == typeof(SimpleIDNameObject))
                return new SelectList(list, IDNameObjectManager<TEntity>.PropertyName_ID, IDNameObjectManager<TEntity>.PropertyName_Name);
            else
                return new SelectList(list, IDNameObjectType<TEntity>.IDProperty.Name, IDNameObjectType<TEntity>.NameProperty.Name);
        }

        internal static SelectList FromList(IList<TEntity> list, object selectedID)
        {
            if (typeof(TEntity) == typeof(SimpleIDNameObject))
                return new SelectList(list, IDNameObjectManager<TEntity>.PropertyName_ID, IDNameObjectManager<TEntity>.PropertyName_Name, selectedID);
            else
                return new SelectList(list, IDNameObjectType<TEntity>.IDProperty.Name, IDNameObjectType<TEntity>.NameProperty.Name, selectedID);
        }

        internal static SelectList FromItem(TEntity selectedItem)
        {
            var list = new List<TEntity> { selectedItem };
            var ino = new IDNameObject<TEntity>(selectedItem);
            return FromList(list, ino.ID);
        }
    }
}
