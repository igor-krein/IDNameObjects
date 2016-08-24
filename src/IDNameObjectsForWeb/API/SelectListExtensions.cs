using System;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
using System.Web.Mvc;
using IDNameObjects.Engine;

namespace IDNameObjects.Mvc
{
    public static class SelectListExtensions
    {
        public static SelectList ToSelectList<TEntity>(this TEntity selectedItem)
            where TEntity : class
        {
            return SelectListManager<TEntity>.FromItem(selectedItem);
        }

        // selectedOnly == true - list of one only (selected) item
        // otherwise - list of all items from the set
        public static SelectList ToSelectList<TEntity>(this IQueryable<TEntity> set, object selectedID, bool selectedOnly, Expression<Func<TEntity, object>> customOrderBySelector = null)
            where TEntity : class
        {
            if (IDNameObjectType<TEntity>.IsSimplifiable)
            {
                var q = (selectedOnly)
                    ? SelectListManager<TEntity>.SimpleINOQuery(set, selectedID)
                    : SelectListManager<TEntity>.SimpleINOQuery(set, customOrderBySelector);
                return SelectListManager<SimpleIDNameObject>.FromList(q.ToList(), selectedID);
            }
            else
            {
                var q = (selectedOnly)
                    ? SelectListManager<TEntity>.Query(set, selectedID)
                    : SelectListManager<TEntity>.Query(set, customOrderBySelector);
                return SelectListManager<TEntity>.FromList(q.ToList(), selectedID);
            }
        }

        // list of all items from the set
        public static SelectList ToSelectList<TEntity>(this IQueryable<TEntity> set, Expression<Func<TEntity, object>> customOrderBySelector = null)
            where TEntity : class
        {
            if (IDNameObjectType<TEntity>.IsSimplifiable)
            {
                var list = SelectListManager<TEntity>.SimpleINOQuery(set, customOrderBySelector).ToList();
                return SelectListManager<SimpleIDNameObject>.FromList(list);
            }
            else
            {
                var list = SelectListManager<TEntity>.Query(set, customOrderBySelector).ToList();
                return SelectListManager<TEntity>.FromList(list);
            }
        }

        // list of all items from the set (otherwise there would be no sense to order it)
        public static SelectList ToSelectList<TEntity>(this IQueryable<TEntity> set, object selectedID, Expression<Func<TEntity, object>> customOrderBySelector)
            where TEntity : class
        {
            return ToSelectList(set, selectedID, false, customOrderBySelector);
        }

        // list of one only (selected) item
        public static SelectList ToSelectList<TEntity>(this IQueryable<TEntity> set, object selectedID)
            where TEntity : class
        {
            return ToSelectList(set, selectedID, true, null);
        }


        // selectedOnly == true - list of one only (selected) item
        // otherwise - list of all items from the set
        public static async Task<SelectList> ToSelectListAsync<TEntity>(this IQueryable<TEntity> set, object selectedID, bool selectedOnly, Expression<Func<TEntity, object>> customOrderBySelector = null)
            where TEntity : class
        {
            if (IDNameObjectType<TEntity>.IsSimplifiable)
            {
                var q = (selectedOnly)
                    ? SelectListManager<TEntity>.SimpleINOQuery(set, selectedID)
                    : SelectListManager<TEntity>.SimpleINOQuery(set, customOrderBySelector);
                return SelectListManager<SimpleIDNameObject>.FromList(await q.ToEntityListAsync(), selectedID);
            }
            else
            {
                var q = (selectedOnly)
                    ? SelectListManager<TEntity>.Query(set, selectedID)
                    : SelectListManager<TEntity>.Query(set, customOrderBySelector);
                return SelectListManager<TEntity>.FromList(await q.ToEntityListAsync(), selectedID);
            }
        }

        // list of all items from the set
        public static async Task<SelectList> ToSelectListAsync<TEntity>(this IQueryable<TEntity> set, Expression<Func<TEntity, object>> customOrderBySelector = null)
            where TEntity : class
        {
            if (IDNameObjectType<TEntity>.IsSimplifiable)
            {
                var list = await SelectListManager<TEntity>.SimpleINOQuery(set, customOrderBySelector).ToEntityListAsync();
                return SelectListManager<SimpleIDNameObject>.FromList(list);
            }
            else
            {
                var list = await SelectListManager<TEntity>.Query(set, customOrderBySelector).ToEntityListAsync();
                return SelectListManager<TEntity>.FromList(list);
            }
        }

        // list of all items from the set (otherwise there would be no sense to order it)
        public static async Task<SelectList> ToSelectListAsync<TEntity>(this IQueryable<TEntity> set, object selectedID, Expression<Func<TEntity, object>> customOrderBySelector)
            where TEntity : class
        {
            return await ToSelectListAsync(set, selectedID, false, customOrderBySelector);
        }

        // list of one only (selected) item
        public static async Task<SelectList> ToSelectListAsync<TEntity>(this IQueryable<TEntity> set, object selectedID)
            where TEntity : class
        {
            return await ToSelectListAsync(set, selectedID, true, null);
        }

    }
}
