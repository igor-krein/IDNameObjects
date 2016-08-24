using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;

namespace IDNameObjects
{
    internal class Select2ObjectManager<TEntity>
        where TEntity : class
    {
        internal static Select2Object ToSelect2Object(TEntity entity)
        {
            SimpleIDNameObject obj;
            obj = (entity is SimpleIDNameObject) ? entity as SimpleIDNameObject : new IDNameObject<TEntity>(entity);
            if (obj == null) return null;
            return new Select2Object { id = obj.ID.ToString(), text = obj.Name };
        }

        internal static Select2PageResult ToSelect2PageResult(IList<TEntity> list, int pageNumber, int pageSize, int total)
        {
            if (pageNumber < 0) pageNumber = 0;
            if (pageSize < 0) pageSize = 0;
            bool more = true;
            if ((pageNumber + pageSize == 0) || (pageNumber * pageSize >= total))
                more = false;

            return new Select2PageResult
            {
                items = list?.Select(ino => ToSelect2Object(ino)),
                more = more
            };
        }

        internal static IQueryable<TEntity> Query(IQueryable<TEntity> set, string search, int pageNumber, int pageSize, Expression<Func<TEntity, object>> customOrderBySelector = null)
        {
            return set.QuickQuery(search, pageNumber, pageSize, customOrderBySelector);
        }

        internal static IQueryable<SimpleIDNameObject> SimpleINOQuery(IQueryable<TEntity> set, string search, int pageNumber, int pageSize, Expression<Func<TEntity, object>> customOrderBySelector = null)
        {
            return set.QuickSimpleINOQuery(search, pageNumber, pageSize, customOrderBySelector);
        }
    }
}
