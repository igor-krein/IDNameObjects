using System;
using System.Reflection;

namespace IDNameObjects.Classes
{
    internal static class ClassManager
    {
        internal static MethodInfo FindStaticGenericMethod(Type classType, string methodName, int genericArgsCnt, int paramsCnt)
        {
            return FindGenericMethod(
                BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static,
                classType,
                methodName,
                genericArgsCnt,
                paramsCnt
            );
        }

        internal static MethodInfo FindGenericMethod(BindingFlags bindingAttr, Type classType, string methodName, int genericArgsCnt, int paramsCnt)
        {
            // Get all static methods in the class, and put them
            // in an array of System.Reflection.MemberInfo objects.
            MemberInfo[] MyMemberInfo = classType.GetMethods(bindingAttr);

            MethodInfo mi = null;
            // Loop through all methods found
            for (int i = 0; i < MyMemberInfo.Length; i++)
            {
                // Just in case - check that this is a method
                if (MyMemberInfo[i].MemberType != MemberTypes.Method) continue;

                MethodInfo currentMI = MyMemberInfo[i] as MethodInfo;
                // Find first method with default name
                // and save it in case there is no method with required attribute
                if (currentMI.IsGenericMethod && (currentMI.Name == methodName))
                {
                    // Check if the method has right number of generic arguments
                    var genericArguments = currentMI.GetGenericMethodDefinition().GetGenericArguments();
                    if (genericArguments.Length != genericArgsCnt)
                        continue;

                    // Check if the method has right number of parameters
                    var methodParameters = currentMI.GetParameters();
                    if (methodParameters.Length != paramsCnt)
                        continue;

                    // Do not check parameters' types
                    mi = currentMI;
                    break;
                }
            }

            return mi;
        }

        internal static object InvokeGenericMethod(MethodInfo mi, Type genericType, params object[] paramsList)
        {
            return InvokeGenericMethod(mi, new Type[] { genericType }, paramsList);
        }

        internal static object InvokeGenericMethod(MethodInfo mi, Type[] genericTypes, params object[] paramsList)
        {
            if (mi == null) return null;
            mi = mi.MakeGenericMethod(genericTypes);
            return mi.Invoke(null, paramsList);
        }
}
/*
    internal static class ClassManager<TClass>
    {
        private static Type classType = typeof(TClass);

        internal static object InvokeStaticGenericMethod(string methodName, Type genericType, params object[] paramsList)
        {
            return ClassManager.InvokeStaticGenericMethod(classType, methodName, genericType, paramsList);
        }

        internal static object InvokeStaticGenericMethod(string methodName, Type[] genericTypes, params object[] paramsList)
        {
            return ClassManager.InvokeStaticGenericMethod(classType, methodName, genericTypes, paramsList);
        }

        internal static object InvokeGenericMethod(BindingFlags bindingAttr, string methodName, Type[] genericTypes, params object[] paramsList)
        {
            return ClassManager.InvokeGenericMethod(bindingAttr, classType, methodName, genericTypes, paramsList);
        }
    }
*/
}
