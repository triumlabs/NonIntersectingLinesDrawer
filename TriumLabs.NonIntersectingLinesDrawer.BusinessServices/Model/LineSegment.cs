using System.Diagnostics;
using System.Windows;

namespace TriumLabs.NonIntersectingLinesDrawer.BusinessServices.Model
{
    /// <summary>
    /// Represents a line segment.
    /// </summary>
    [DebuggerDisplay("({StartVector} -> {EndVector})")]
    public class LineSegment
    {
        /// <summary>
        /// Gets or sets the base vector of segment's start point.
        /// </summary>
        public Vector StartVector { get; set; }

        /// <summary>
        /// Gets or sets the base vector of segment's end point.
        /// </summary>
        public Vector EndVector { get; set; }
    }
}
