using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace FlutterSharp.Extensions
{
    internal static class TypeExtension
    {
        public static T GetCustomAttribute<T>(this Type type, bool inherit = true) where T : Attribute
        {
            return type.GetCustomAttributes<T>(inherit).FirstOrDefault();
        }

        public static IEnumerable<T> GetCustomAttributes<T>(this Type type, bool inherit = true) where T : Attribute
        {
            return type.GetCustomAttributes(typeof(T), inherit).Select(a => a as T);
        }

        public static T GetCustomAttribute<T>(this MethodInfo method, bool inherit = true) where T : Attribute
        {
            return method.GetCustomAttributes<T>(inherit).FirstOrDefault();
        }

        public static IEnumerable<T> GetCustomAttributes<T>(this MethodInfo method, bool inherit = true) where T : Attribute
        {
            return method.GetCustomAttributes(typeof(T), inherit).Select(a => a as T);
        }
    }
}