using System;
using System.Linq.Expressions;
using System.Reflection;

namespace IDNameObjects.Engine
{
    public static class IDNameObjectType<TEntity>
        where TEntity : class
    {
        static IDNameObjectType()
        {
            var ct = IDNameObjectManager<TEntity>.ClassType;    // fake action to call IDNameObjectManager constructor
        }

        public static PropertyInfo IDProperty { get; internal set; }
        public static PropertyInfo NameProperty { get; internal set; }
        public static PropertyInfo NameDBProperty { get; internal set; }
        public static MethodInfo NameContainsPredicateMethod { get; internal set; }
        public static MethodInfo NameStartsWithPredicateMethod { get; internal set; }
        public static MethodInfo IDEqualsPredicateMethod { get; internal set; }
        public static MethodInfo IDsInListPredicateMethod { get; internal set; }
        public static MethodInfo IDsNotInListPredicateMethod { get; internal set; }
        public static MethodInfo OrderBySelectorMethod { get; internal set; }
        public static MethodInfo OrderMethod { get; internal set; }

        public static Type SimpleINOType { get; internal set; }
        public static bool IsSimplifiable { get; internal set; }

        internal static string NameSearchType { get; set; }
        internal static int DefaultPageSize { get; set; }

        internal static Expression<Func<TEntity, object>> IDSelectorExpression { get; set; }
        internal static Expression<Func<TEntity, object>> NameSelectorExpression { get; set; }
        internal static Expression<Func<TEntity, object>> NameDBSelectorExpression { get; set; }
        internal static Expression<Func<TEntity, object>> OrderBySelectorExpression { get; set; }

        internal static Func<TEntity, object> IDSelectorFunc { get; set; }
        internal static Func<TEntity, object> NameSelectorFunc { get; set; }
        internal static Func<TEntity, object> NameDBSelectorFunc { get; set; }
        internal static Func<TEntity, object> OrderBySelectorFunc { get; set; }

        internal static Expression<Func<TEntity, SimpleIDNameObject>> SimpleINOSelectorExpression { get; set; }
    }
}
