using System.Collections;
using System.Collections.Generic;
using System.Linq;
using IDNameObjects.Engine;

namespace IDNameObjects
{
    public static class INOCollectionExtensions
    {
        public static IEnumerable<TEntity> DefaultOrder<TEntity>(this ICollection<TEntity> list)
            where TEntity : class
        {
            return Enumerable.OrderBy(list, IDNameObjectType<TEntity>.OrderBySelectorFunc);
        }

        public static IEnumerable<object> AsIDs<TEntity>(this ICollection<TEntity> list)
            where TEntity : class
        {
            return Enumerable.Select(list, IDNameObjectType<TEntity>.IDSelectorFunc);
        }

        public static IList ToIDList<TEntity>(this ICollection<TEntity> list)
            where TEntity : class
        {
            return Enumerable.ToList(list.AsIDs());
        }

    }
}
