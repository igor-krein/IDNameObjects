using ObjectExtensions;
using System;
using System.Linq;
using System.Reflection;

namespace IDNameObjects.Attributes
{
    internal static class AttributeManager<TAttr>
        where TAttr : Attribute
    {
        private static Type attrType = typeof(TAttr);
        private static Type baseType = typeof(System.Object);

        static AttributeManager()
        {

        }

        // Finds a public object's property:
        // ... has some property of some attribute equal to name (case-insensitive)
        // ... or just has some attribute (when its property is not provided)
        // ... or simply has such a name (case-insensitive)
        // If property is not found - tries to look it up in the base class(es)
        public static PropertyInfo GetPropertyAttributeNameProp(Type curClassType, string[] attrPropertyValues, string attrPropertyName = null, PropertyInfo nonattributedProperty = null)
        {
            if (curClassType == baseType) return null;

            string[] lowerNames = new string[attrPropertyValues?.Length ?? 0];
            for (int i = 0; i < lowerNames.Length; i++)
            {
                lowerNames[i] = attrPropertyValues[i]?.ToLower();
            }

            // Get all public non-static properties declared in this class, and put them
            // in an array of System.Reflection.PropertyInfo objects.
            PropertyInfo[] MyPropertyInfo = curClassType.GetProperties(BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly);

            // Loop through all methods found
            for (int i = 0; i < MyPropertyInfo.Length; i++)
            {
                // Just in case - check that this is a method
                if (MyPropertyInfo[i].MemberType != MemberTypes.Property) continue;

                // Find first property with default name
                // and save it in case there is no property with required attribute
                if ((nonattributedProperty == null) && lowerNames.Contains(MyPropertyInfo[i].Name.ToLower())) nonattributedProperty = MyPropertyInfo[i];

                // Check for desired attribute
                TAttr attribute = (TAttr)Attribute.GetCustomAttribute(MyPropertyInfo[i], attrType);
                if (attribute == null) continue;

                // Attribute found - check specified property (if needed)
                if ((attrPropertyName == null) ||
                    lowerNames.Contains(attribute.GetPropertyValue(attrPropertyName)?.ToString().ToLower()))
                {
                    // Method with required attribute is found - return
                    return MyPropertyInfo[i];
                }
            }

            // Return nonattributed method or look up required method in base class(es)
            return nonattributedProperty ?? GetPropertyAttributeNameProp(curClassType.BaseType, attrPropertyValues, attrPropertyName, nonattributedProperty);
        }

        public static PropertyInfo GetPropertyAttributeNameProp(Type curClassType, string attrPropertyValue, string attrPropertyName = null, PropertyInfo nonattributedProperty = null)
        {
            return GetPropertyAttributeNameProp(curClassType, new string[] { attrPropertyValue }, attrPropertyName, nonattributedProperty);
        }

        // Finds a public static class method that:
        // ... has some property of some attribute equal to name (case-insensitive)
        // ... or just has some attribute (when property is not provided)
        // ... or simply has such a name (case-sensitive)
        // If method is not found - tries to look it up in the base class(es)
        public static MethodInfo GetStaticMethodByAttributeNameProp(Type curClassType, string attrPropertyValue, string attrPropertyName = null, MethodInfo nonattributedMethod = null)
        {
            if (curClassType == baseType) return null;

            string lowerName = attrPropertyValue?.ToLower();

            // Get all public static methods in this class, and put them
            // in an array of System.Reflection.MemberInfo objects.
            MemberInfo[] MyMemberInfo = curClassType.GetMethods(BindingFlags.Public | BindingFlags.Static);

            // Loop through all methods found
            for (int i = 0; i < MyMemberInfo.Length; i++)
            {
                // Just in case - check that this is a method
                if (MyMemberInfo[i].MemberType != MemberTypes.Method) continue;

                // Find first method with default name
                // and save it in case there is no method with required attribute
                if ((nonattributedMethod == null) && (MyMemberInfo[i].Name == attrPropertyValue)) nonattributedMethod = MyMemberInfo[i] as MethodInfo;

                // Check for desired attribute
                TAttr attribute = (TAttr)Attribute.GetCustomAttribute(MyMemberInfo[i], attrType);
                if (attribute == null) continue;

                // Attribute found - check specified property (if needed)
                if ((attrPropertyName == null) ||
                    (attribute.GetPropertyValue(attrPropertyName)?.ToString().ToLower() == lowerName))
                {
                    // Method with required attribute is found - return
                    return MyMemberInfo[i] as MethodInfo;
                }
            }

            // Return nonattributed method or look up required method in base class(es)
            return nonattributedMethod ?? GetStaticMethodByAttributeNameProp(curClassType.BaseType, attrPropertyValue, attrPropertyName, nonattributedMethod);
        }

        public static TAttr GetClassAttribute(Type curClassType)
        {
            if (curClassType == baseType) return null;

            TAttr attribute = (TAttr)Attribute.GetCustomAttribute(curClassType, attrType);

            // If attribute not found - look it up in the base class
            return attribute ?? GetClassAttribute(curClassType.BaseType);
        }
    }
}
