using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
using System.Web.Mvc;
using IDNameObjects.Engine;

namespace IDNameObjects.Mvc
{
    public static class MultiSelectListExtensions
    {
        // TODO: add support for user-friendly ordering ([IDNameObjectMethodAttribute(Name="Order")])
        // multiselect list from in-memory collection
        public static MultiSelectList ToMultiSelectList<TEntity>(this ICollection<TEntity> selectedList, Func<TEntity, object> customOrderBySelectorFunc = null)
            where TEntity : class
        {
            var orderedList = ((customOrderBySelectorFunc == null) ? selectedList.DefaultOrder() : selectedList.OrderBy(customOrderBySelectorFunc)).ToList();
            var ids = orderedList.ToIDList();
            return MultiSelectListManager<TEntity>.FromList(orderedList, ids);
        }

        // selectedOnly == true - list of selected items only (selectedIDlist)
        // otherwise - list of all items from the set
        public static MultiSelectList ToMultiSelectList<TEntity>(this IQueryable<TEntity> set, IList selectedIDlist, bool selectedOnly, Expression<Func<TEntity, object>> customOrderBySelector = null)
            where TEntity : class
        {
            IList ids = (selectedOnly) ? selectedIDlist : null;
            if (IDNameObjectType<TEntity>.IsSimplifiable)
            {
                var list = MultiSelectListManager<TEntity>.SimpleINOQuery(set, ids, customOrderBySelector).ToList();
                return MultiSelectListManager<SimpleIDNameObject>.FromList(list, selectedIDlist);
            }
            else
            {
                var list = MultiSelectListManager<TEntity>.Query(set, ids, customOrderBySelector).ToList();
                return MultiSelectListManager<TEntity>.FromList(list, selectedIDlist);
            }
        }

        // list of selected items only (selectedIDlist)
        public static MultiSelectList ToMultiSelectList<TEntity>(this IQueryable<TEntity> set, IList selectedIDlist, Expression<Func<TEntity, object>> customOrderBySelector = null)
            where TEntity : class
        {
            return ToMultiSelectList(set, selectedIDlist, true, customOrderBySelector);
        }

        // list of all items from the set
        public static MultiSelectList ToMultiSelectList<TEntity>(this IQueryable<TEntity> set, Expression<Func<TEntity, object>> customOrderBySelector = null)
            where TEntity : class
        {
            return ToMultiSelectList<TEntity>(set, null, false, customOrderBySelector);
        }

        // selectedOnly == true - list of selected items only (selectedIDlist)
        // otherwise - list of all items from the set
        public static async Task<MultiSelectList> ToMultiSelectListAsync<TEntity>(this IQueryable<TEntity> set, IList selectedIDlist, bool selectedOnly, Expression<Func<TEntity, object>> customOrderBySelector = null)
            where TEntity : class
        {
            IList ids = (selectedOnly) ? selectedIDlist : null;
            if (IDNameObjectType<TEntity>.IsSimplifiable)
            {
                var list = await MultiSelectListManager<TEntity>.SimpleINOQuery(set, ids, customOrderBySelector).ToEntityListAsync();
                return MultiSelectListManager<SimpleIDNameObject>.FromList(list, selectedIDlist);
            }
            else
            {
                var list = await MultiSelectListManager<TEntity>.Query(set, ids, customOrderBySelector).ToEntityListAsync();
                return MultiSelectListManager<TEntity>.FromList(list, selectedIDlist);
            }
        }

        // list of selected items only (selectedIDlist)
        public static async Task<MultiSelectList> ToMultiSelectListAsync<TEntity>(this IQueryable<TEntity> set, IList selectedIDlist, Expression<Func<TEntity, object>> customOrderBySelector = null)
            where TEntity : class
        {
            return await ToMultiSelectListAsync(set, selectedIDlist, true, customOrderBySelector);
        }

        // list of all items from the set
        public static async Task<MultiSelectList> ToMultiSelectListAsync<TEntity>(this IQueryable<TEntity> set, Expression<Func<TEntity, object>> customOrderBySelector = null)
            where TEntity : class
        {
            return await ToMultiSelectListAsync<TEntity>(set, null, false, customOrderBySelector);
        }
    }
}
