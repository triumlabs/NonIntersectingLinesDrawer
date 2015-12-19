using System;
using System.Linq;
using System.Reflection;

namespace TriumLabs.Core.Reflection
{
    /// <summary>
    /// Extensions for <see cref="System.Reflection.ICustomAttributeProvider"/> interface.
    /// </summary>
    public static class CustomAttributeProviderExtensions
    {
        /// <summary>
        /// Gets the first custom attribute of the given type, null is returned if there is no custom attribute of that type.
        /// </summary>
        /// <typeparam name="T">The type of the custom attribute.</typeparam>
        /// <param name="provider">The object which custom attribute is requested.</param>
        /// <returns>The custom attribute or null.</returns>
        public static T GetCustomAttribute<T>(this ICustomAttributeProvider provider)
            where T : Attribute
        {
            var attribute = provider.GetCustomAttributes(typeof(T), false).FirstOrDefault() as T;
            return attribute;
        }

        /// <summary>
        /// Gets the first custom attribute of the given type, null is returned if there is no custom attribute of that type.
        /// </summary>
        /// <typeparam name="T">The type of the custom attribute.</typeparam>
        /// <param name="provider">The object which custom attribute is requested.</param>
        /// <param name="inherit">if true, look up the hierarchy chain for the inherited custom attribute.</param>
        /// <returns>The custom attribute or null.</returns>
        public static T GetCustomAttribute<T>(this ICustomAttributeProvider provider, bool inherit)
            where T : Attribute
        {
            var attribute = provider.GetCustomAttributes(typeof(T), inherit).FirstOrDefault() as T;
            return attribute;
        }

        /// <summary>
        /// Gets the custom attributes of the given type, empty array is returned if there are no custom attributes of that type.
        /// </summary>
        /// <typeparam name="T">The type of the custom attributes.</typeparam>
        /// <param name="provider">The object which custom attributes are requested.</param>
        /// <returns>Array of custom attributes, or an empty array.</returns>
        public static T[] GetCustomAttributes<T>(this ICustomAttributeProvider provider)
            where T : Attribute
        {
            var attributes = (T[])provider.GetCustomAttributes(typeof(T), false);
            return attributes;
        }

        /// <summary>
        /// Gets the custom attributes of the given type, empty array is returned if there are no custom attributes of that type.
        /// </summary>
        /// <typeparam name="T">The type of the custom attributes.</typeparam>
        /// <param name="provider">The object which custom attributes are requested.</param>
        /// <param name="inherit">if true, look up the hierarchy chain for the inherited custom attribute.</param>
        /// <returns>Array of custom attributes, or an empty array.</returns>
        public static T[] GetCustomAttributes<T>(this ICustomAttributeProvider provider, bool inherit)
            where T : Attribute
        {
            var attributes = (T[])provider.GetCustomAttributes(typeof(T), inherit);
            return attributes;
        }
    }
}
