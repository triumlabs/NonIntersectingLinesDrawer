using System;
using System.Windows;

namespace TriumLabs.NonIntersectingLinesDrawer.BusinessServices
{
    /// <summary>
    /// Provides extensions for struct Vector.
    /// </summary>
    public static class VectorExtensions
    {
        /// <summary>
        /// Normalizes the vector to have the specified length.
        /// </summary>
        /// <param name="vector">The vector to normalize.</param>
        /// <param name="length">The length to normalize the vector to.</param>
        /// <returns>The normalized vector.</returns>
        public static Vector NormalizeTo(this Vector vector, double length = 1.0)
        {
            return length * vector / vector.Length;
        }

        /// <summary>
        /// Rotates the vector by the specified angle (in radian).
        /// </summary>
        /// <param name="vector">The vector to rotate.</param>
        /// <param name="angle">The angle in radianused in rotation.</param>
        /// <returns>The rotated vector.</returns>
        /// <remarks>How to rotate a vector, see https://en.wikipedia.org/wiki/Rotation_(mathematics). </remarks>
        public static Vector RotateBy(this Vector vector, double angle)
        {
            var sinAngle = Math.Sin(angle);
            var cosAngle = Math.Cos(angle);
            return new Vector(vector.X * cosAngle - vector.Y * sinAngle, vector.X * sinAngle + vector.Y * cosAngle);
        }
    }
}
