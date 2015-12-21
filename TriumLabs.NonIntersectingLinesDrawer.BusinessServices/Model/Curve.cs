using System.Collections.Generic;
using System.Linq;

namespace TriumLabs.NonIntersectingLinesDrawer.BusinessServices.Model
{
    /// <summary>
    /// Represents a curve.
    /// </summary>
    public class Curve
    {
        /// <summary>
        /// Gets the list of line segments which build up the curve.
        /// </summary>
        public IEnumerable<LineSegment> Segments { get; private set; }

        /// <summary>
        /// Gets the length of the curve.
        /// </summary>
        public double Length { get; private set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="T:Curve"/> class.
        /// </summary>
        /// <param name="segments">The list of line segments which build up the curve.</param>
        public Curve(IEnumerable<LineSegment> segments)
        {
            Segments = segments.ToArray();
            Length = Segments.Sum(segment => (segment.EndVector - segment.StartVector).Length);
        }
    }
}
