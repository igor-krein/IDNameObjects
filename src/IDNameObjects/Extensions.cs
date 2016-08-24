using System;
using System.Collections;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;

namespace IDNameObjects
{
    public static class IDNameObjectExtensions
    {
/*
                public static IQueryable<object> AsID<TEntity>(this IQueryable<TEntity> list)
                    where TEntity : class
                {
                    return Queryable.Select(list, IDNameObjectType<TEntity>.IDSelectorExpression);
                }
                public static IList ToIDsList<TEntity>(this IEnumerable<TEntity> enumerable)
                    where TEntity : class
                {
        //            IList<object> ids = null;
                    if (enumerable is IQueryable<TEntity>)
                    {
                        var e = (enumerable as IQueryable<TEntity>).AsID();
                        return Enumerable.ToList(e);
        //                ids = Enumerable.ToList((enumerable as IQueryable<TEntity>).AsID());
                    }
                    else
                    {
                        ICollection<TEntity> collection = (enumerable is ICollection<TEntity>)
                            ? enumerable as ICollection<TEntity>
                            : enumerable.ToList();
                        var e = collection.AsID();
                        return Enumerable.ToList(e);
        //                ids = Enumerable.ToList(collection.AsID());
                    }
                }

                public static IList ToIDsList<TEntity>(this ICollection<TEntity> list)
                    where TEntity : class
                {
                    return Enumerable.ToList(list.AsID());
                }

                public static IList ToIDsList<TEntity>(this IQueryable<TEntity> list)
                    where TEntity : class
                {
                    return Enumerable.ToList(list.AsID());
                }
*/

        // IQueryable methods
        public static IQueryable<TEntity> WhereID<TEntity, TKey>(this IQueryable<TEntity> set, TKey id)
            where TEntity : class
        {
            return IDNameObjectManager<TEntity>._where(set, IDNameObjectManager<TEntity>.GetIDEqualsPredicate(id));
        }

        public static IQueryable<TEntity> WhereIDsIn<TEntity, TKey>(this IQueryable<TEntity> set, IList<TKey> ids)
            where TEntity : class
        {
            return IDNameObjectManager<TEntity>._where(set, IDNameObjectManager<TEntity>.GetIDsInListPredicate(ids));
        }

        public static IQueryable<TEntity> WhereIDsNotIn<TEntity, TKey>(this IQueryable<TEntity> set, IList<TKey> ids)
            where TEntity : class
        {
            return IDNameObjectManager<TEntity>._where(set, IDNameObjectManager<TEntity>.GetIDsNotInListPredicate(ids));
        }

        public static IQueryable<TEntity> WhereNameContains<TEntity>(this IQueryable<TEntity> set, string search = null)
            where TEntity : class
        {
            return IDNameObjectManager<TEntity>._where(set, (search == null) ? null : IDNameObjectManager<TEntity>.GetNameContainsPredicate(search));
        }

        public static IQueryable<TEntity> WhereNameStartsWith<TEntity>(this IQueryable<TEntity> set, string search = null)
            where TEntity : class
        {
            return IDNameObjectManager<TEntity>._where(set, (search == null) ? null : IDNameObjectManager<TEntity>.GetNameStartsWithPredicate(search));
        }


        public static IQueryable<TEntity> DefaultWhere<TEntity>(this IQueryable<TEntity> set, string search = null)
            where TEntity : class
        {
            return IDNameObjectManager<TEntity>._where(set, (search == null) ? null : IDNameObjectManager<TEntity>.GetNameSearchPredicate(search));
        }

        public static IQueryable<TEntity> DefaultWhere<TEntity, TKey>(this IQueryable<TEntity> set, IList<TKey> ids)
            where TEntity : class
        {
            return IDNameObjectManager<TEntity>._where(set, IDNameObjectManager<TEntity>.GetIDsInListPredicate(ids));
        }

        public static IQueryable<TEntity> DefaultOrder<TEntity>(this IQueryable<TEntity> set)
            where TEntity : class
        {
            return IDNameObjectManager<TEntity>._orderby(set, IDNameObjectType<TEntity>.OrderBySelectorExpression);
        }

        public static IQueryable<TEntity> DefaultWhereOrder<TEntity>(this IQueryable<TEntity> set, string search = null)
            where TEntity : class
        {
            return set.DefaultWhere(search).DefaultOrder();
        }

        public static IQueryable<TEntity> DefaultWhereOrder<TEntity, TKey>(this IQueryable<TEntity> set, IList<TKey> ids)
            where TEntity : class
        {
            return set.DefaultWhere(ids).DefaultOrder();
        }


        public static int DefaultCount<TEntity>(this IQueryable<TEntity> set, string search = null)
            where TEntity : class
        {
            return set.DefaultWhere(search).Count();
        }

        public static int DefaultCount<TEntity, TKey>(this IQueryable<TEntity> set, IList<TKey> ids)
            where TEntity : class
        {
            return set.DefaultWhere(ids).Count();
        }

        public static async Task<int> DefaultCountAsync<TEntity>(this IQueryable<TEntity> set, string search = null)
            where TEntity : class
        {
            return await set.DefaultWhere(search).CountAsync();
        }

        public static async Task<int> DefaultCountAsync<TEntity, TKey>(this IQueryable<TEntity> set, IList<TKey> ids)
            where TEntity : class
        {
            return await set.DefaultWhere(ids).CountAsync();
        }

        public static IQueryable<TEntity> Page<TEntity>(this IQueryable<TEntity> set, int pageNumber, int pageSize = 0)
            where TEntity : class
        {
            if (pageNumber <= 0) return set;
            if (pageSize <= 0) pageSize = IDNameObjectManager<TEntity>.GetDefaultPageSize();
            return set.Skip((pageNumber-1) * pageSize).Take(pageSize);
        }

        public static IQueryable<SimpleIDNameObject> AsSimpleINO<TEntity>(this IQueryable<TEntity> set)
            where TEntity : class
        {
            return IDNameObjectManager<TEntity>._selectAsSimpleINO(set);
        }


        public static IQueryable<TEntity> QuickQuery<TEntity>(this IQueryable<TEntity> set, string search, int pageNumber, int pageSize, Expression<Func<TEntity, object>> customOrderBySelector = null)
            where TEntity : class
        {
            return (customOrderBySelector == null)
                ? set.DefaultWhereOrder(search).Page(pageNumber, pageSize)
                : set.DefaultWhere(search).OrderBy(customOrderBySelector).Page(pageNumber, pageSize);
        }

        public static IQueryable<TEntity> QuickQuery<TEntity>(this IQueryable<TEntity> set, string search, int pageNumber, Expression<Func<TEntity, object>> customOrderBySelector = null)
            where TEntity : class
        {
            return set.QuickQuery(search, pageNumber, 0, customOrderBySelector);
        }

        public static IQueryable<TEntity> QuickQuery<TEntity>(this IQueryable<TEntity> set, string search = null, Expression<Func<TEntity, object>> customOrderBySelector = null)
            where TEntity : class
        {
            return set.QuickQuery(search, 0, 0, customOrderBySelector);
        }

        public static IQueryable<TEntity> QuickQuery<TEntity, TKey>(this IQueryable<TEntity> set, IList<TKey> ids, Expression<Func<TEntity, object>> customOrderBySelector = null)
            where TEntity : class
        {
            return (customOrderBySelector == null)
                ? set.DefaultWhereOrder(ids)
                : set.DefaultWhere(ids).OrderBy(customOrderBySelector);
        }

        public static IQueryable<SimpleIDNameObject> QuickSimpleINOQuery<TEntity>(this IQueryable<TEntity> set, string search, int pageNumber, int pageSize, Expression<Func<TEntity, object>> customOrderBySelector = null)
            where TEntity : class
        {
            return set.QuickQuery(search, pageNumber, pageSize, customOrderBySelector).AsSimpleINO();
        }

        public static IQueryable<SimpleIDNameObject> QuickSimpleINOQuery<TEntity>(this IQueryable<TEntity> set, string search, int pageNumber, Expression<Func<TEntity, object>> customOrderBySelector = null)
            where TEntity : class
        {
            return set.QuickQuery(search, pageNumber, 0, customOrderBySelector).AsSimpleINO();
        }

        public static IQueryable<SimpleIDNameObject> QuickSimpleINOQuery<TEntity>(this IQueryable<TEntity> set, string search = null, Expression<Func<TEntity, object>> customOrderBySelector = null)
            where TEntity : class
        {
            return set.QuickQuery(search, 0, 0, customOrderBySelector).AsSimpleINO();
        }

        public static IQueryable<SimpleIDNameObject> QuickSimpleINOQuery<TEntity, TKey>(this IQueryable<TEntity> set, IList<TKey> ids, Expression<Func<TEntity, object>> customOrderBySelector = null)
            where TEntity : class
        {
            return set.QuickQuery(ids, customOrderBySelector).AsSimpleINO();
        }


        public static IList<TEntity> QuickList<TEntity>(this IQueryable<TEntity> set, string search, int pageNumber, int pageSize, Expression<Func<TEntity, object>> customOrderBySelector = null)
            where TEntity : class
        {
            return set.QuickQuery(search, pageNumber, pageSize, customOrderBySelector).ToList();
        }

        public static IList<TEntity> QuickList<TEntity>(this IQueryable<TEntity> set, string search, int pageNumber, Expression<Func<TEntity, object>> customOrderBySelector = null)
            where TEntity : class
        {
            return set.QuickQuery(search, pageNumber, 0, customOrderBySelector).ToList();
        }

        public static IList<TEntity> QuickList<TEntity>(this IQueryable<TEntity> set, string search = null, Expression<Func<TEntity, object>> customOrderBySelector = null)
            where TEntity : class
        {
            return set.QuickQuery(search, 0, 0, customOrderBySelector).ToList();
        }

        public static IList<TEntity> QuickList<TEntity, TKey>(this IQueryable<TEntity> set, IList<TKey> ids, Expression<Func<TEntity, object>> customOrderBySelector = null)
            where TEntity : class
        {
            return set.QuickQuery(ids, customOrderBySelector).ToList();
        }


        public static async Task<IList<TEntity>> QuickListAsync<TEntity>(this IQueryable<TEntity> set, string search, int pageNumber, int pageSize, Expression<Func<TEntity, object>> customOrderBySelector = null)
            where TEntity : class
        {
            return await set.QuickQuery(search, pageNumber, pageSize, customOrderBySelector).ToListAsync();
        }

        public static async Task<IList<TEntity>> QuickListAsync<TEntity>(this IQueryable<TEntity> set, string search, int pageNumber, Expression<Func<TEntity, object>> customOrderBySelector = null)
            where TEntity : class
        {
            return await set.QuickQuery(search, pageNumber, 0, customOrderBySelector).ToListAsync();
        }

        public static async Task<IList<TEntity>> QuickListAsync<TEntity>(this IQueryable<TEntity> set, string search = null, Expression<Func<TEntity, object>> customOrderBySelector = null)
            where TEntity : class
        {
            return await set.QuickQuery(search, 0, 0, customOrderBySelector).ToListAsync();
        }

        public static async Task<IList<TEntity>> QuickListAsync<TEntity, TKey>(this IQueryable<TEntity> set, IList<TKey> ids, Expression<Func<TEntity, object>> customOrderBySelector = null)
            where TEntity : class
        {
            return await set.QuickQuery(ids, customOrderBySelector).ToListAsync();
        }


        public static IList<SimpleIDNameObject> QuickSimpleINOList<TEntity>(this IQueryable<TEntity> set, string search, int pageNumber, int pageSize, Expression<Func<TEntity, object>> customOrderBySelector = null)
            where TEntity : class
        {
            return set.QuickSimpleINOQuery(search, pageNumber, pageSize, customOrderBySelector).ToList();
        }

        public static IList<SimpleIDNameObject> QuickSimpleINOList<TEntity>(this IQueryable<TEntity> set, string search, int pageNumber, Expression<Func<TEntity, object>> customOrderBySelector = null)
            where TEntity : class
        {
            return set.QuickSimpleINOQuery(search, pageNumber, 0, customOrderBySelector).ToList();
        }

        public static IList<SimpleIDNameObject> QuickSimpleINOList<TEntity>(this IQueryable<TEntity> set, string search = null, Expression<Func<TEntity, object>> customOrderBySelector = null)
            where TEntity : class
        {
            return set.QuickSimpleINOQuery(search, 0, 0, customOrderBySelector).ToList();
        }

        public static IList<SimpleIDNameObject> QuickSimpleINOList<TEntity, TKey>(this IQueryable<TEntity> set, IList<TKey> ids, Expression<Func<TEntity, object>> customOrderBySelector = null)
            where TEntity : class
        {
            return set.QuickSimpleINOQuery(ids, customOrderBySelector).ToList();
        }

        public static async Task<IList<SimpleIDNameObject>> QuickSimpleINOListAsync<TEntity>(this IQueryable<TEntity> set, string search, int pageNumber, int pageSize, Expression<Func<TEntity, object>> customOrderBySelector = null)
            where TEntity : class
        {
            return await set.QuickSimpleINOQuery(search, pageNumber, pageSize, customOrderBySelector).ToListAsync();
        }

        public static async Task<IList<SimpleIDNameObject>> QuickSimpleINOListAsync<TEntity>(this IQueryable<TEntity> set, string search, int pageNumber, Expression<Func<TEntity, object>> customOrderBySelector = null)
            where TEntity : class
        {
            return await set.QuickSimpleINOQuery(search, pageNumber, 0, customOrderBySelector).ToListAsync();
        }

        public static async Task<IList<SimpleIDNameObject>> QuickSimpleINOListAsync<TEntity>(this IQueryable<TEntity> set, string search = null, Expression<Func<TEntity, object>> customOrderBySelector = null)
            where TEntity : class
        {
            return await set.QuickSimpleINOQuery(search, 0, 0, customOrderBySelector).ToListAsync();
        }

        public static async Task<IList<SimpleIDNameObject>> QuickSimpleINOListAsync<TEntity, TKey>(this IQueryable<TEntity> set, IList<TKey> ids, Expression<Func<TEntity, object>> customOrderBySelector = null)
            where TEntity : class
        {
            return await set.QuickSimpleINOQuery(ids, customOrderBySelector).ToListAsync();
        }


        public static async Task<IList<TEntity>> ToEntityListAsync<TEntity>(this IQueryable<TEntity> set)
            where TEntity : class
        {
            return await set.ToListAsync();
        }

        public static IList<SimpleIDNameObject> ToSimpleINOList<TEntity>(this IQueryable<TEntity> set)
            where TEntity : class
        {
            return set.AsSimpleINO().ToList();
        }

        public static async Task<IList<SimpleIDNameObject>> ToSimpleINOListAsync<TEntity>(this IQueryable<TEntity> set)
            where TEntity : class
        {
            return await set.AsSimpleINO().ToListAsync();
        }
    }
}
