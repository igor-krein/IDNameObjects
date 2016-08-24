using System;
using System.Collections.Generic;

namespace ObjectExtensions
{
    public static class ObjectExtensions
    {
        public static object GetPropertyValue<T>(this T obj, string propertyName)
        {
            return obj?.GetType().GetProperty(propertyName)?.GetValue(obj, null);
        }

        public static bool IsGenericList(this object obj, Type t = null)
        {
            var oType = obj.GetType();

            // if this is generic list
            if (oType.IsGenericType && (oType.GetGenericTypeDefinition() == typeof(List<>)))
            {
                // if this is the list of a given type (or the type doesn't matter)
                if ((t == null) || (t == oType.GetProperty("Item").PropertyType))
                    return true;
            }

            return false;
        }

        /*
                public static T GetDefaultValue<T>()
                {
                    // We want an Func<T> which returns the default.
                    // Create that expression here.
                    Expression<Func<T>> e = Expression.Lambda<Func<T>>(
                        // The default value, always get what the *code* tells us.
                        Expression.Default(typeof(T))
                    );

                    // Compile and return the value.
                    return e.Compile()();
                }

                public static object GetDefaultValue(this Type type)
                {
                    // Validate parameters.
                    if (type == null) throw new ArgumentNullException("type");

                    // We want an Func<object> which returns the default.
                    // Create that expression here.
                    Expression<Func<object>> e = Expression.Lambda<Func<object>>(
                        // Have to convert to object.
                        Expression.Convert(
                            // The default value, always get what the *code* tells us.
                            Expression.Default(type), typeof(object)
                        )
                    );

                    // Compile and return the value.
                    return e.Compile()();
                }
        */
    }

}
