using System.Collections.Generic;
using System.Windows;
using TriumLabs.NonIntersectingLinesDrawer.BusinessServices.Model;

namespace TriumLabs.NonIntersectingLinesDrawer.BusinessServices
{
    /// <summary>
    /// Defines the Curve service.
    /// </summary>
    public interface ICurveService
    {
        /// <summary>
        /// Finds a curve between the given two points which does not intersect any other provided curves.
        /// </summary>
        /// <param name="vectorA">The base vector of point A.</param>
        /// <param name="vectorB">The base vector of point B.</param>
        /// <param name="curves">The list of curves which the new curve should not intersect.</param>
        /// <returns>The non-intersecting curve if it exists; otherwise null.</returns>
        Curve FindNonIntersectingCurve(Vector vectorA, Vector vectorB, IEnumerable<Curve> curves);

        /// <summary>
        /// Detects whether the given two line segments intersect each other.
        /// </summary>
        /// <param name="lineSegmentA">The first line segment to detect intersection.</param>
        /// <param name="lineSegmentB">The second line segment to detect intersection.</param>
        /// <returns><c>true</c> if the given two line segments intersect; otherwise <c>false</c>.</returns>
        /// <remarks>The explanation of detection of line segments intersection can be found at http://stackoverflow.com/questions/563198/how-do-you-detect-where-two-line-segments-intersect. </remarks>
        bool DetectLineSegmentsIntersection(LineSegment lineSegmentA, LineSegment lineSegmentB);
    }
}
