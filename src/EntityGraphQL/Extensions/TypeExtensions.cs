using System;
using System.Reflection;
using System.Linq;
using System.Collections.Generic;

namespace EntityGraphQL.Extensions
{
    public static class TypeExtensions
    {
        /// <summary>
        /// Return the Type unwrapped from any Nullable<>
        /// </summary>
        /// <param name="source"></param>
        /// <returns></returns>
        public static Type GetNonNullableType(this Type source)
        {
            if (source.IsNullableType())
            {
                return source.GetGenericArguments()[0];
            }
            return source;
        }

        public static Type GetNonNullableOrEnumerableType(this Type source)
        {
            return source.GetNonNullableType().GetEnumerableOrArrayType() ?? source.GetNonNullableType();
        }

        /// <summary>
        /// Returns true if this type is an Enumerable<> or an array
        /// </summary>
        /// <param name="source"></param>
        /// <returns></returns>
        public static bool IsEnumerableOrArray(this Type source)
        {
            if (source == typeof(string) || source == typeof(byte[]))
                return false;

            if (source.GetTypeInfo().IsArray)
            {
                return true;
            }
            var isEnumerable = false;
            if (source.GetTypeInfo().IsGenericType && !source.IsNullableType())
            {
                isEnumerable = IsGenericTypeEnumerable(source);
            }
            return isEnumerable;
        }

        private static bool IsGenericTypeEnumerable(Type source)
        {
            bool isEnumerable = (source.GetTypeInfo().IsGenericType && source.GetGenericTypeDefinition() == typeof(IEnumerable<>) || source.GetTypeInfo().IsGenericType && source.GetGenericTypeDefinition() == typeof(IQueryable<>));
            if (!isEnumerable)
            {
                foreach (var intType in source.GetInterfaces())
                {
                    isEnumerable = IsGenericTypeEnumerable(intType);
                    if (isEnumerable)
                        break;
                }
            }

            return isEnumerable;
        }

        /// <summary>
        /// Return the array element type or the generic type for a IEnumerable<T>
        /// Specifically does not treat string as IEnumerable<char> and will not return byte for byte[]
        /// </summary>
        /// <param name="type"></param>
        /// <returns></returns>
        public static Type GetEnumerableOrArrayType(this Type type)
        {
            if (type == typeof(string) || type == typeof(byte[]) || type == typeof(byte))
            {
                return null;
            }
            if (type.IsArray)
                return type.GetElementType();
            if (type.GenericTypeArguments.Count() == 1)
                return type.GetGenericArguments()[0];
            return null;
        }

        public static bool IsNullableType(this Type t)
        {
            return t.GetTypeInfo().IsGenericType && t.GetGenericTypeDefinition() == typeof(Nullable<>);
        }
    }
}
