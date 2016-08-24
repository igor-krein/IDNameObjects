using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Web.Mvc;
using IDNameObjects.Engine;

namespace IDNameObjects.Mvc
{
    internal class MultiSelectListManager<TEntity>
        where TEntity : class
    {
        internal static IQueryable<TEntity> Query(IQueryable<TEntity> set, IList selectedIDlist = null, Expression<Func<TEntity, object>> customOrderBySelector = null)
        {
//            var q = (selectedIDlist == null) ? set.DefaultWhere() : set.DefaultWhere(selectedIDlist);
            var q = (selectedIDlist == null) ? set : set.DefaultWhere(selectedIDlist);
            return (customOrderBySelector == null) ? q.DefaultOrder() : q.OrderBy(customOrderBySelector);
        }

        internal static IQueryable<SimpleIDNameObject> SimpleINOQuery(IQueryable<TEntity> set, IList selectedIDlist = null, Expression<Func<TEntity, object>> customOrderBySelector = null)
        {
            return Query(set, selectedIDlist, customOrderBySelector).AsSimpleINOs();
        }

        internal static MultiSelectList FromList(IList<TEntity> list, IEnumerable ids)
        {
            if (typeof(TEntity) == typeof(SimpleIDNameObject))
                return new MultiSelectList(list, IDNameObjectManager<TEntity>.PropertyName_ID, IDNameObjectManager<TEntity>.PropertyName_Name, ids);
            else
                return new MultiSelectList(list, IDNameObjectType<TEntity>.IDProperty.Name, IDNameObjectType<TEntity>.NameProperty.Name, ids);
        }
    }
}
