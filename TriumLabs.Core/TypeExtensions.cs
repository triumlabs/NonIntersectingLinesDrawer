using System;
using System.Reflection;

namespace TriumLabs.Core
{
    /// <summary>
    /// Provides extensions for class Type.
    /// </summary>
    public static class TypeExtensions
    {
        /// <summary>
        /// Gets whether type has a default constructor.
        /// </summary>
        /// <param name="type">The type.</param>
        /// <returns>true if type has a default constructor; otherwise, false.</returns>
        public static bool HasDefaultConstructor(this Type type)
        {
            if (type == null) throw new ArgumentNullException("type");
            
            if (type.IsPrimitive) return true;

            var ci = type.GetConstructor(
                    BindingFlags.Instance | BindingFlags.Public | BindingFlags.CreateInstance,
                    null,
                    new Type[0],
                    null);
            return ci != null;
        }

        /// <summary>
        /// Gets whether type is nullable.
        /// </summary>
        /// <param name="type">The type.</param>
        /// <returns>true if type is nullable; otherwise, false.</returns>
        public static bool IsNullable(this Type type)
        {
            if (type == null) throw new ArgumentNullException("type");

            return type.IsGenericType && type.GetGenericTypeDefinition() == typeof(Nullable<>);
        }
    }
}
