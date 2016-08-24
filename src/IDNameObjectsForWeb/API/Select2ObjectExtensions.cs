using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
using IDNameObjects.Engine;

namespace IDNameObjects.Mvc
{
    public static class Select2ObjectExtensions
    {
        public static Select2Object ToSelect2Object<TEntity>(this TEntity entity)
            where TEntity : class
        {
            return Select2ObjectManager<TEntity>.ToSelect2Object(entity);
        }

        public static Select2PageResult ToSelect2PageResult<TEntity>(this IList<TEntity> list, int pageNumber, int pageSize, int total)
            where TEntity : class
        {
            return Select2ObjectManager<TEntity>.ToSelect2PageResult(list, pageNumber, pageSize, total);
        }


        public static Select2PageResult ToSelect2PageResult<TEntity>(this IQueryable<TEntity> set, string search, int pageNumber, int pageSize, Expression<Func<TEntity, object>> customOrderBySelector = null)
            where TEntity : class
        {
            var total = set.DefaultCount(search);
            if (IDNameObjectType<TEntity>.IsSimplifiable)
            {
                var list = Select2ObjectManager<TEntity>.SimpleINOQuery(set, search, pageNumber, pageSize, customOrderBySelector).ToList();
                return list.ToSelect2PageResult(pageNumber, pageSize, total);
            }
            else
            {
                var list = Select2ObjectManager<TEntity>.Query(set, search, pageNumber, pageSize, customOrderBySelector).ToList();
                return list.ToSelect2PageResult(pageNumber, pageSize, total);
            }
        }

        public static Select2PageResult ToSelect2PageResult<TEntity>(this IQueryable<TEntity> set, string search, int pageNumber, Expression<Func<TEntity, object>> customOrderBySelector = null)
            where TEntity : class
        {
            return ToSelect2PageResult(set, search, pageNumber, 0, customOrderBySelector);
        }

        public static Select2PageResult ToSelect2PageResult<TEntity>(this IQueryable<TEntity> set, string search = null, Expression<Func<TEntity, object>> customOrderBySelector = null)
            where TEntity : class
        {
            return ToSelect2PageResult(set, search, 0, 0, customOrderBySelector);
        }

        public static async Task<Select2PageResult> ToSelect2PageResultAsync<TEntity>(this IQueryable<TEntity> set, string search, int pageNumber, int pageSize, Expression<Func<TEntity, object>> customOrderBySelector = null)
            where TEntity : class
        {
            var total = await set.DefaultCountAsync(search);
            if (IDNameObjectType<TEntity>.IsSimplifiable)
            {
                var list = await Select2ObjectManager<TEntity>.SimpleINOQuery(set, search, pageNumber, pageSize, customOrderBySelector).ToEntityListAsync();
                return list.ToSelect2PageResult(pageNumber, pageSize, total);
            }
            else
            {
                var list = await Select2ObjectManager<TEntity>.Query(set, search, pageNumber, pageSize, customOrderBySelector).ToEntityListAsync();
                return list.ToSelect2PageResult(pageNumber, pageSize, total);
            }
        }

        public static async Task<Select2PageResult> ToSelect2PageResultAsync<TEntity>(this IQueryable<TEntity> set, string search, int pageNumber, Expression<Func<TEntity, object>> customOrderBySelector = null)
            where TEntity : class
        {
            return await ToSelect2PageResultAsync(set, search, pageNumber, 0, customOrderBySelector);
        }

        public static async Task<Select2PageResult> ToSelect2PageResultAsync<TEntity>(this IQueryable<TEntity> set, string search = null, Expression<Func<TEntity, object>> customOrderBySelector = null)
            where TEntity : class
        {
            return await ToSelect2PageResultAsync(set, search, 0, 0, customOrderBySelector);
        }

    }
}