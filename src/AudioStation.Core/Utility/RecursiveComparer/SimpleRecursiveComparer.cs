using System.Collections;
using System.Reflection;
using System.Security.Cryptography.X509Certificates;
using System.Windows.Forms;

using AudioStation.Core.Utility.RecursiveComparer.Attribute;
using AudioStation.Core.Utility.RecursiveComparer.Interface;
using AudioStation.Model;

using AutoMapper.Internal;

using Microsoft.Extensions.Logging;

using NAudio.MediaFoundation;

using Newtonsoft.Json.Linq;

using SimpleWpf.Extensions;

namespace AudioStation.Core.Utility.RecursiveComparer
{
    public class SimpleRecursiveComparer : IRecursiveComparer
    {
        public SimpleRecursiveComparer()
        {
        }

        public bool Compare<T>(T object1, T object2, out string message)
        {
            message = string.Empty;
            var result = true;

            // Check Recursion:  This may be a recursion leaf. So, check to see if we've finished.
            //
            var bothNulls = false;

            if (!VerifyReferences(object1, object2, out bothNulls))
                return false;

            if (bothNulls)
                return true;

            // Get all (shallow) properties for this object
            var properties = ReflectionStore.GetAll<T>();

            // CHECK FOR PRIMITIVES!
            if (!properties.Any())
            {
                // This could be cleaned up a bit.. when we find all possibilities for these value types
                //
                if (!IsPrimitive(typeof(T)) && (typeof(T) != typeof(string)))
                    throw new Exception("Improper use of primitives:  SimpleRecursiveComparer.cs");

                return object1.Equals(object2);
            }

            foreach (var property in properties)
            {
                // IGNORED PROPERTIES
                if (property.CustomAttributes.Any(x => x.AttributeType == typeof(RecursiveCompareIgnoreAttribute)))
                    continue;

                result &= CompareRecurse(object1, object2, property, out message);

                if (!result)
                    break;
            }

            return result;
        }

        public bool CompareRecurse<T>(T object1, T object2, PropertyInfo property, out string message)
        {
            // Best to enumerate these types. MSFT's Type (System.Runtime.Type) is not easy to pick apart; and
            // there are properties that don't make sense. IsPrimitive, for example, will miss some of these 
            // "blue" fields.
            //

            // Nullable
            if (IsNullable(property.PropertyType))
                return CompareNullable(object1, object2, property, out message);

            // Primitives
            else if (IsPrimitive(property.PropertyType))
                return ComparePrimitive(object1, object2, property, out message);

            // Any IComparable
            else if (property.PropertyType.HasInterface<IComparable>())
                return CompareIComparable<T>(object1, object2, property, out message);

            // Collections
            else if (property.PropertyType.HasInterface<IEnumerable>())
                return CompareCollection(object1, object2, property, out message);

            else if (property.PropertyType.HasInterface<IDictionary>())
                return CompareCollection(object1, object2, property, out message);

            // Complex Types
            else
                return CompareReference(object1, object2, property, out message);
        }

        private bool IsNullable(Type type)
        {
            return type.IsGenericType(typeof(Nullable<>));
        }

        private bool IsPrimitive(Type type)
        {
            if (type == typeof(bool))
                return true;

            else if (type == typeof(byte))
                return true;

            else if (type == typeof(short))
                return true;

            else if (type == typeof(char))
                return true;

            else if (type == typeof(int))
                return true;

            else if (type == typeof(long))
                return true;

            else if (type == typeof(double))
                return true;

            else if (type == typeof(float))
                return true;

            else if (type == typeof(uint))
                return true;

            else if (type == typeof(ushort))
                return true;

            else if (type == typeof(ulong))
                return true;

            else if (type.IsEnum)
                return true;

            return false;
        }

        private bool CompareNullable<TObject>(TObject object1, TObject object2, PropertyInfo propertyInfo, out string message)
        {
            var nullable1 = ReflectReference<TObject>(object1, propertyInfo, out message);
            var nullable2 = ReflectReference<TObject>(object2, propertyInfo, out message);
            var bothNulls = false;

            if (!VerifyReferences(nullable1, nullable2, out bothNulls))
                return false;

            if (bothNulls)
                return true;

            // PRIMITIVE VALUE (ComparePrimitive)
            if (IsPrimitive(nullable1.GetType()))
                return nullable1.Equals(nullable2);

            // Recurse (will compare Nullable sub-properties)
            return Compare(nullable1, nullable2, out message);
        }
        private bool ComparePrimitive<TObject>(TObject object1, TObject object2, PropertyInfo propertyInfo, out string message)
        {
            var value1 = ReflectPrimitive<TObject>(object1, propertyInfo, out message);
            var value2 = ReflectPrimitive<TObject>(object2, propertyInfo, out message);

            return value1.Equals(value2);
        }

        private bool CompareReference<TObject>(TObject object1, TObject object2, PropertyInfo propertyInfo, out string message)
        {
            var value1 = ReflectReference<TObject>(object1, propertyInfo, out message);
            var value2 = ReflectReference<TObject>(object2, propertyInfo, out message);
            var bothNulls = false;

            if (!VerifyReferences(value1, value2, out bothNulls))
                return false;

            if (bothNulls)
                return true;

            // Recurse
            return Compare(value1, value2, out message);
        }

        private bool CompareIComparable<TObject>(TObject object1, TObject object2, PropertyInfo propertyInfo, out string message)
        {
            var value1 = (IComparable)ReflectReference<TObject>(object1, propertyInfo, out message);
            var value2 = (IComparable)ReflectReference<TObject>(object2, propertyInfo, out message);
            var bothNulls = false;

            if (!VerifyReferences(value1, value2, out bothNulls))
                return false;

            if (bothNulls)
                return true;

            return value1.CompareTo(value2) == 0;
        }

        private bool CompareCollection<TObject>(TObject object1, TObject object2, PropertyInfo propertyInfo, out string message)
        {
            var value1 = (IEnumerable)ReflectReference<TObject>(object1, propertyInfo, out message);
            var value2 = (IEnumerable)ReflectReference<TObject>(object2, propertyInfo, out message);
            var bothNulls = false;

            if (!VerifyReferences(value1, value2, out bothNulls))
                return false;

            if (bothNulls)
                return true;

            var value1Copy = new ArrayList();
            var value2Copy = new ArrayList();
            var value1Count = 0;
            var value2Count = 0;

            // Cut down on iteration. Just get it out of the way...There's a hidden iterator each step of the way.
            // 
            foreach (var item in value1)
            {
                value1Copy.Add(item);
                value1Count++;
            }
            foreach (var item in value2)
            {
                value2Copy.Add(item);
                value2Count++;

                // Use this loop to compare the items, also
                if (value2Count > value1Count)
                    return false;

                var item1 = value1Copy[value2Count - 1];
                var item2 = value2Copy[value2Count - 1];

                // Recurse
                var itemCompare = Compare(item1, item2, out message);

                if (!itemCompare)
                    return false;
            }

            if (value1Count != value2Count)
                return false;

            return true;
        }

        // Check possible null combinations
        private bool VerifyReferences<TObject>(TObject object1, TObject object2, out bool bothNulls)
        {
            bothNulls = false;

            if (object1 == null && object2 == null)
            {
                bothNulls = true;
                return true;
            }
                
            else if (object1 != null && object2 == null)
                return false;

            else if (object1 == null && object2 != null)
                return false;

            return true;
        }

        private object ReflectPrimitive<TObject>(TObject theObject, PropertyInfo propertyInfo, out string message)
        {
            try
            {
                message = string.Empty;

                return propertyInfo.GetValue(theObject);
            }
            catch (Exception ex)
            {
                message = ex.Message;
                ApplicationHelpers.Log("Error reflecting property:  {0}", LogMessageType.General, LogLevel.Error, ex, propertyInfo.Name);
                throw ex;
            }
        }

        private object ReflectReference<TObject>(TObject theObject, PropertyInfo propertyInfo, out string message)
        {
            try
            {
                message = string.Empty;

                if (theObject == null)
                    return default;

                return propertyInfo.GetValue(theObject);
            }
            catch (Exception ex)
            {
                message = ex.Message;
                ApplicationHelpers.Log("Error reflecting property:  {0}", LogMessageType.General, LogLevel.Error, ex, propertyInfo.Name);
                throw ex;
            }
        }
    }
}
