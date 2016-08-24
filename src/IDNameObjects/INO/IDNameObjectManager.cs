using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Collections;
using IDNameObjects.Attributes;
using IDNameObjects.Classes;
using ObjectExtensions;

namespace IDNameObjects.Engine
{
    public static class IDNameObjectManager<TEntity>
        where TEntity : class
    {
        public const string MethodName_NameContains = "NameContainsPredicate";
        public const string MethodName_NameStartsWith = "NameStartsWithPredicate";
        public const string MethodName_IDEquals = "IDEqualsPredicate";
        public const string MethodName_IDsInList = "IDsInListPredicate";
        public const string MethodName_IDsNotInList = "IDsNotInListPredicate";
        public const string MethodName_OrderBySelector = "OrderBySelector";
        public const string MethodName_Order = "Order";

        public const string PropertyName_ID = "ID";
        public const string PropertyName_Name = "Name";
        public const string PropertyName_NameDB = "NameDB";

        // To add enum with filter/order options ???
        // All
        // NameContains
        // NameStartWith
        // IDEquals
        // IDsInList
        // IDsNotInList
        // OrderAsc
        // OrderDesc
        private static MethodInfo miDefaultIDEqualsExpression;
        private static MethodInfo miDefaultIDsInListExpression;
        //private static MethodInfo miOrderBy;

        public static Type ClassType { get; private set; }
        private static ParameterExpression entityParameter;
        internal static MemberExpression idMemberExpression, nameMemberExpression, nameDBMemberExpression;

        private static Expression TrueExpression = Expression.Constant(true, typeof(bool));
        private static Expression FalseExpression = Expression.Constant(false, typeof(bool));

        static IDNameObjectManager()
        {
            ClassType = typeof(TEntity);
//            if (ClassType.IsGenericType && (ClassType.GetGenericTypeDefinition() == typeof(IDNameObject<>)))
            if (ClassType.GetType() == typeof(SimpleIDNameObject))
            {
                throw new ArgumentException("You cannot make an IDNameObject from IDNameObject");
            }
            entityParameter = Expression.Parameter(ClassType, "entity");

            IDNameObjectType<TEntity>.IDProperty = GetIDProperty();
            IDNameObjectType<TEntity>.NameProperty = GetNameProperty();
            if ((IDNameObjectType<TEntity>.IDProperty == null) || (IDNameObjectType<TEntity>.NameProperty == null))
            {
                throw new ArgumentException($"Failed to determine ID and/or Name property of type {ClassType.Name}");
            }
            IDNameObjectType<TEntity>.NameDBProperty = GetNameDBProperty(IDNameObjectType<TEntity>.NameProperty);

            idMemberExpression = Expression.Property(entityParameter, IDNameObjectType<TEntity>.IDProperty.Name);
            nameMemberExpression = Expression.Property(entityParameter, IDNameObjectType<TEntity>.NameProperty.Name);
            nameDBMemberExpression = Expression.Property(entityParameter, IDNameObjectType<TEntity>.NameDBProperty.Name);

            var entityAttribute = AttributeManager<IDNameObjectAttribute>.GetClassAttribute(ClassType);
            IDNameObjectType<TEntity>.NameSearchType = entityAttribute?.NameSearchType ?? "Contains";
            IDNameObjectType<TEntity>.DefaultPageSize = entityAttribute?.PageSize ?? 10;

            IDNameObjectType<TEntity>.SimpleINOType = typeof(SimpleIDNameObject);
            IDNameObjectType<TEntity>.IsSimplifiable = (IDNameObjectType<TEntity>.NameProperty == IDNameObjectType<TEntity>.NameDBProperty);

            miDefaultIDEqualsExpression =
                ClassManager.FindStaticGenericMethod(typeof(IDNameObjectManager<TEntity>), "DefaultIDEqualsExpression", 1, 1);

            miDefaultIDsInListExpression =
                ClassManager.FindStaticGenericMethod(typeof(IDNameObjectManager<TEntity>), "DefaultIDsInListExpression", 1, 1);
/*
            miOrderBy =
                ClassManager.FindStaticGenericMethod(typeof(IDNameObjectManager<TEntity>), "_orderby", 1, 2);
*/
            try
            {
                IDNameObjectType<TEntity>.NameContainsPredicateMethod = GetNameContainsPredicateMethod();
                IDNameObjectType<TEntity>.NameStartsWithPredicateMethod = GetNameStartsWithPredicateMethod();
                IDNameObjectType<TEntity>.IDEqualsPredicateMethod = GetIDEqualsPredicateMethod();
                IDNameObjectType<TEntity>.IDsInListPredicateMethod = GetIDsInListPredicateMethod();
                IDNameObjectType<TEntity>.IDsNotInListPredicateMethod = GetIDsNotInListPredicateMethod();
                IDNameObjectType<TEntity>.OrderBySelectorMethod = GetOrderBySelectorMethod();
                IDNameObjectType<TEntity>.OrderMethod = GetOrderMethod();

                IDNameObjectType<TEntity>.IDSelectorExpression = GetPropertySelectorExpression(IDNameObjectType<TEntity>.IDProperty);
                IDNameObjectType<TEntity>.NameSelectorExpression = GetPropertySelectorExpression(IDNameObjectType<TEntity>.NameProperty);
                IDNameObjectType<TEntity>.NameDBSelectorExpression = GetPropertySelectorExpression(IDNameObjectType<TEntity>.NameDBProperty);
                IDNameObjectType<TEntity>.OrderBySelectorExpression = GetOrderBySelector();

                IDNameObjectType<TEntity>.IDSelectorFunc = GetPropertySelectorFunc(IDNameObjectType<TEntity>.IDSelectorExpression);
                IDNameObjectType<TEntity>.NameSelectorFunc = GetPropertySelectorFunc(IDNameObjectType<TEntity>.NameSelectorExpression);
                IDNameObjectType<TEntity>.NameDBSelectorFunc = GetPropertySelectorFunc(IDNameObjectType<TEntity>.NameDBSelectorExpression);
                IDNameObjectType<TEntity>.OrderBySelectorFunc = GetPropertySelectorFunc(IDNameObjectType<TEntity>.OrderBySelectorExpression);

                IDNameObjectType<TEntity>.SimpleINOSelectorExpression = GetSimpleINOSelectorExpression();
            }
            catch
            {
                throw new ArgumentException($"{ClassType.Name} contains method(s) with IDNameObjectMethod attribute that is (are) of wrong type.");
            }
        }

        private static PropertyInfo GetIDProperty()
        {
            PropertyInfo pi;
            pi = AttributeManager<IDNameObjectPropertyAttribute>.GetPropertyAttributeNameProp(ClassType, new string[] { PropertyName_ID, ClassType.Name + PropertyName_ID }, "Name");
            if (pi == null) pi = AttributeManager<IDNameObjectPropertyAttribute>.GetPropertyAttributeNameProp(ClassType, "");
            return pi;
        }

        private static PropertyInfo GetNameProperty()
        {
            return AttributeManager<IDNameObjectPropertyAttribute>.GetPropertyAttributeNameProp(ClassType, new string[] { PropertyName_Name }, "Name");
        }

        private static PropertyInfo GetNameDBProperty(PropertyInfo defaultProperty = null)
        {
            var p = AttributeManager<IDNameObjectPropertyAttribute>.GetPropertyAttributeNameProp(ClassType, new string[] { PropertyName_NameDB }, "Name");
            return p ?? defaultProperty ?? GetNameProperty();
        }

        private static MethodInfo GetIDNameObjectMethod(string name)
        {
            MethodInfo mi = AttributeManager<IDNameObjectMethodAttribute>.GetStaticMethodByAttributeNameProp(ClassType, name, "Name");
            if (mi?.IsGenericMethod ?? false)
            {
                mi = mi.MakeGenericMethod(ClassType);
            }
            return mi;
        }

        private static MethodInfo GetNameContainsPredicateMethod()
        {
            return GetIDNameObjectMethod(MethodName_NameContains);
        }

        private static MethodInfo GetNameStartsWithPredicateMethod()
        {
            return GetIDNameObjectMethod(MethodName_NameStartsWith);
        }
        
        private static MethodInfo GetIDEqualsPredicateMethod()
        {
            return GetIDNameObjectMethod(MethodName_IDEquals);
        }

        private static MethodInfo GetIDsInListPredicateMethod()
        {
            return GetIDNameObjectMethod(MethodName_IDsInList);
        }

        private static MethodInfo GetIDsNotInListPredicateMethod()
        {
            return GetIDNameObjectMethod(MethodName_IDsNotInList);
        }

        private static MethodInfo GetOrderBySelectorMethod()
        {
            return GetIDNameObjectMethod(MethodName_OrderBySelector);
        }

        private static MethodInfo GetOrderMethod()
        {
            return GetIDNameObjectMethod(MethodName_Order);
        }

        private static Expression<Func<TEntity, bool>> NameMethodPredicate(string filterString, string methodName)
        {
            Type stringType = typeof(string);

//            ConstantExpression filterStringParameter = Expression.Constant(filterString, stringType);
            Expression<Func<string>> filterStringLambda = () => filterString;
            MethodInfo method = stringType.GetMethod(methodName, new[] { stringType });
            MethodCallExpression containsExpression = Expression.Call(nameDBMemberExpression, method, filterStringLambda.Body);

            return Expression.Lambda<Func<TEntity, bool>>(containsExpression, entityParameter);
        }

        private static Expression<Func<TEntity, bool>> DefaultNameContainsPredicate(string filterString)
        {
            return NameMethodPredicate(filterString, "Contains");
        }

        private static Expression<Func<TEntity, bool>> DefaultNameStartsWithPredicate(string filterString)
        {
            return NameMethodPredicate(filterString, "StartsWith");
        }

        private static Expression DefaultIDEqualsExpression<TID>(TID id)
        {
            Type idType = typeof(TID);

            //            ConstantExpression idParameter = Expression.Constant(id, idType);
            Expression<Func<TID>> idLambda = () => id;
            BinaryExpression equalExpression = Expression.Equal(idMemberExpression, idLambda.Body);

            return equalExpression;
        }

        private static Expression DefaultIDsInListExpression<TID>(List<TID> ids)
        {
            int cnt = ids?.Count() ?? 0;
            if (cnt == 0) return FalseExpression;
            if (cnt == 1) return DefaultIDEqualsExpression(ids[0]);

            Type listType = typeof(List<TID>),
                idType = typeof(TID);
            // Converting IList to List because it is difficult to get Contains method for arrays
            //List<TID> list = (ids is List<TID>) ? ids as List<TID> : ids.ToList();

            //            ConstantExpression idsParameter = Expression.Constant(list, listType);
            Expression<Func<List<TID>>> idsLambda = () => ids;
            Expression convertExpression = Expression.Convert(idMemberExpression, idType);
            MethodInfo method = listType.GetMethod("Contains", new[] { idType });
            MethodCallExpression containsExpression = Expression.Call(idsLambda.Body, method, convertExpression);

            return containsExpression;
        }

        private static Expression DefaultIDEqualsExpression(object id)
        {
            Type idType = IDNameObjectType<TEntity>.IDProperty.PropertyType;
            if (id.GetType() != idType) return null;

            Expression resultExpression = (Expression)ClassManager.InvokeGenericMethod(miDefaultIDEqualsExpression, idType, id);

            return resultExpression;
        }

        private static Expression DefaultIDsInListExpression(IList idList)
        {
            if (idList == null) return FalseExpression;

            Type idType = IDNameObjectType<TEntity>.IDProperty.PropertyType;
            object ids;
            Type genericClass = typeof(List<>);
            Type listType = genericClass.MakeGenericType(idType);

            if (idList.IsGenericList(idType))
            {
                ids = idList;
            }
            else
            {
                // Converting IList to List because it is difficult to get Contains method for arrays
                // check for idType
                ids = Activator.CreateInstance(listType);
                foreach (var elm in idList)
                {
                    ((IList)ids).Add(elm);
                }
            }

            Expression resultExpression = (Expression)ClassManager.InvokeGenericMethod(miDefaultIDsInListExpression, idType, ids);
            return resultExpression;
        }

        private static Expression<Func<TEntity, bool>> DefaultIDEqualsPredicate(object id)
        {
            var idEqualsExpression = DefaultIDEqualsExpression(id);
            return Expression.Lambda<Func<TEntity, bool>>(idEqualsExpression, entityParameter);
        }

        private static Expression<Func<TEntity, bool>> DefaultIDsInListPredicate(IList ids)
        {
            var idsInListExpression = DefaultIDsInListExpression(ids);
            return Expression.Lambda<Func<TEntity, bool>>(idsInListExpression, entityParameter);
        }

        private static Expression<Func<TEntity, bool>> DefaultIDsNotInListPredicate(IList ids)
        {
            var idsInListExpression = DefaultIDsInListExpression(ids);
            var notExpression = Expression.Not(idsInListExpression);
            return Expression.Lambda<Func<TEntity, bool>>(notExpression, entityParameter);
        }

        private static Expression<Func<TEntity, object>> GetPropertySelectorExpression(PropertyInfo property)
        {
            if (property == null) return null;

            MemberExpression memberExpression = Expression.Property(entityParameter, property.Name);

            Expression convertExpression = (property.PropertyType.IsValueType)
                ? (Expression)Expression.Convert(memberExpression, typeof(object))
                : memberExpression;

            var delegateType = typeof(Func<,>).MakeGenericType(ClassType, typeof(object));
            return (Expression<Func<TEntity, object>>)Expression.Lambda(delegateType, convertExpression, entityParameter);
        }

        private static Expression<Func<TEntity, SimpleIDNameObject>> GetSimpleINOSelectorExpression()
        {
            Type simpleINOType = typeof(SimpleIDNameObject);
            NewExpression ctorExpression = Expression.New(simpleINOType);
            PropertyInfo simpleINOIDProperty = simpleINOType.GetProperty(PropertyName_ID);
            PropertyInfo simpleINONameProperty = simpleINOType.GetProperty(PropertyName_Name);
            MemberAssignment idAssignment = Expression.Bind(simpleINOIDProperty, idMemberExpression);
            MemberAssignment nameAssignment = Expression.Bind(simpleINONameProperty, nameDBMemberExpression);
            MemberInitExpression newSimpleINOExpression = Expression.MemberInit(ctorExpression, idAssignment, nameAssignment);
            var lambda = Expression.Lambda<Func<TEntity, SimpleIDNameObject>>(newSimpleINOExpression, entityParameter);
            return lambda;
        }

        private static Func<TEntity, object> GetPropertySelectorFunc(Expression<Func<TEntity, object>> propertySelectorExpression)
        {
            return propertySelectorExpression.Compile();
        }

        private static Expression<Func<TEntity, object>> DefaultOrderBySelector()
        {
            return IDNameObjectType<TEntity>.NameDBSelectorExpression;
        }

        internal static Expression<Func<TEntity, bool>> GetNameContainsPredicate(string filterString)
        {
            MethodInfo mi = IDNameObjectType<TEntity>.NameContainsPredicateMethod;
            if (mi != null)
                return (Expression<Func<TEntity, bool>>)mi.Invoke(null, new object[] { filterString });
            else
                return DefaultNameContainsPredicate(filterString);
        }

        internal static Expression<Func<TEntity, bool>> GetNameStartsWithPredicate(string filterString)
        {
            MethodInfo mi = IDNameObjectType<TEntity>.NameStartsWithPredicateMethod;
            if (mi != null)
                return (Expression<Func<TEntity, bool>>)mi.Invoke(null, new object[] { filterString });
            else
                return DefaultNameStartsWithPredicate(filterString);
        }

        internal static Expression<Func<TEntity, bool>> GetNameSearchPredicate(string filterString)
        {
            return (IDNameObjectType<TEntity>.NameSearchType == "Contains") ? GetNameContainsPredicate(filterString) : GetNameStartsWithPredicate(filterString);
        }

        internal static Expression<Func<TEntity, bool>> GetIDEqualsPredicate(object id)
        {
            MethodInfo mi = IDNameObjectType<TEntity>.IDEqualsPredicateMethod;
            if (mi != null)
                return (Expression<Func<TEntity, bool>>)mi.Invoke(null, new object[] { id });
            else
                return DefaultIDEqualsPredicate(id);
        }

        internal static Expression<Func<TEntity, bool>> GetIDsInListPredicate(IList ids)
        {
            MethodInfo mi = IDNameObjectType<TEntity>.IDsInListPredicateMethod;
            if (mi != null)
                return (Expression<Func<TEntity, bool>>)mi.Invoke(null, new object[] { ids });
            else
                return DefaultIDsInListPredicate(ids);
        }

        internal static Expression<Func<TEntity, bool>> GetIDsNotInListPredicate(IList ids)
        {
            MethodInfo mi = IDNameObjectType<TEntity>.IDsNotInListPredicateMethod;
            if (mi != null)
                return (Expression<Func<TEntity, bool>>)mi.Invoke(null, new object[] { ids });
            else
                return DefaultIDsNotInListPredicate(ids);
        }

        internal static Expression<Func<TEntity, object>> GetOrderBySelector()
        {
            MethodInfo mi = IDNameObjectType<TEntity>.OrderBySelectorMethod;
            if (mi != null)
                return (Expression<Func<TEntity, object>>)mi.Invoke(null, null);
            else
                return DefaultOrderBySelector();
        }

        internal static int GetDefaultPageSize()
        {
            return IDNameObjectType<TEntity>.DefaultPageSize;
        }

        internal static IQueryable<TEntity> ApplyWhere(
            IQueryable<TEntity> set,
            Expression<Func<TEntity, bool>> filterPredicate = null
        )
        {
            var q = set;
            if (filterPredicate != null)
            {
                q = q.Where(filterPredicate);
            }
            return q;
        }
/*
        internal static IQueryable<TEntity> _orderby(
            IQueryable<TEntity> set,
            Expression orderBySelector = null
        )
        {
            var q = set;
            if (orderBySelector != null)
            {
                q = Queryable.OrderBy(set, (Expression<Func<TEntity, object>>)orderBySelector);
                q = (IQueryable<TEntity>)ClassManager.InvokeGenericMethod(miOrderBy, typeof(object), new object[] { set, orderBySelector });
            }
            return q;
        }
*/
        // Many thanks to Ivan Stoev http://stackoverflow.com/users/5202563/ivan-stoev
        // http://stackoverflow.com/a/38994227/6499774
        internal static IQueryable<TEntity> ApplyOrderBy(
            IQueryable<TEntity> set,
            Expression<Func<TEntity, object>> orderBySelector = null
        )
        {
            if (orderBySelector == null) return set;
            var body = orderBySelector.Body;
            // Strip the Convert if any
            if (body.NodeType == ExpressionType.Convert)
                body = ((UnaryExpression)body).Operand;
            // Create new selector
            var keySelector = Expression.Lambda(body, orderBySelector.Parameters[0]);
            // Here we cannot use the typed Queryable.OrderBy method because
            // we don't know the TKey, so we compose a method call instead
            var queryExpression = Expression.Call(
                typeof(Queryable), "OrderBy", new[] { ClassType, body.Type },
                set.Expression, Expression.Quote(keySelector));
            return set.Provider.CreateQuery<TEntity>(queryExpression);
        }

        internal static IQueryable<TEntity> ApplyDefaultOrder(IQueryable<TEntity> set)
        {
            MethodInfo mi = IDNameObjectType<TEntity>.OrderMethod;
            if (mi != null)
                return (IQueryable<TEntity>)mi.Invoke(null, new object[] { set });
            else
                return ApplyOrderBy(set, IDNameObjectType<TEntity>.OrderBySelectorExpression);
        }

        /*
                internal static IOrderedQueryable<TEntity> _orderby<TResult>(
                    IQueryable<TEntity> set,
                    Expression<Func<TEntity, TResult>> orderBySelector
                )
                {
                    var q = Queryable.OrderBy(set, orderBySelector);

                    return q;
                }



                internal static IOrderedQueryable<TEntity> _defaultOrderBy(IQueryable<TEntity> set)
                {
                    //var q = (IOrderedQueryable<TEntity>)ClassManager.InvokeGenericMethod(miOrderBy, typeof(string), new object[] { set, (Expression)IDNameObjectType<TEntity>.OrderBySelectorExpression });
                    var q = _orderby(set, IDNameObjectType<TEntity>.OrderBySelectorExpression);
                    return q;
                }
        */


        internal static IQueryable<TEntity> ApplyWhereAndOrderBy(
            IQueryable<TEntity> set,
            Expression<Func<TEntity, bool>> filterPredicate,
            Expression<Func<TEntity, object>> orderBySelector = null
        )
        {
            var q = set;
            q = ApplyWhere(q, filterPredicate);
//            q = _orderby(q, orderBySelector);
            q = ApplyOrderBy(q, orderBySelector);
            return q;
        }

        internal static IQueryable<SimpleIDNameObject> ApplySelectAsSimpleINOs(IQueryable<TEntity> set)
        {
            /*
            if (!IDNameObjectType<TEntity>.IsSimplifiable)
            {
                throw new InvalidOperationException($"You cannot select just [ID] and [Name] from the database because the [Name] property of {ClassType.Name} is not mapped as a DB table field");
            }
            */
            var q = Queryable.Select(set, IDNameObjectType<TEntity>.SimpleINOSelectorExpression);
            return q;
        }
    }
}
