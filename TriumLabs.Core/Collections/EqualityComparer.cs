using System;
using System.Collections.Generic;

namespace TriumLabs.Core.Collections
{
    /// <summary>
    /// Represents a wrapper class to be able to use delegates as equality comparer.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class EqualityComparer<T> : IEqualityComparer<T>
    {
        private Func<T, T, bool> comparer;

        /// <summary>
        /// Initializes a new instance of the <see cref="T:EqualityComparer`1"/> class.
        /// </summary>
        /// <param name="comparer">The ......</param>
        public EqualityComparer(Func<T, T, bool> comparer)
        {
            this.comparer = comparer;
        }

        /// <summary>
        /// Determines whether the specified objects are equal.
        /// </summary>
        /// <param name="x">The first object of type T to compare.</param>
        /// <param name="y">The second object of type T to compare.</param>
        /// <returns>true if the specified objects are equal; otherwise, false.</returns>
        public bool Equals(T x, T y)
        {
            return comparer(x, y);
        }

        /// <summary>
        /// Returns a hash code for the specified object.
        /// </summary>
        /// <param name="obj">The <see cref="Object"/> for which a hash code is to be returned.</param>
        /// <returns>A hash code for the specified object.</returns>
        public int GetHashCode(T obj)
        {
            return obj != null ? obj.GetHashCode() : 0;
        }
    }
}
