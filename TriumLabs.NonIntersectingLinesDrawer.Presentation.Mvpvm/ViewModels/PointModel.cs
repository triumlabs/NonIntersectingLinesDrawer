using System.Diagnostics;
using TriumLabs.Core.ComponentModel;

namespace TriumLabs.NonIntersectingLinesDrawer.Presentation.Mvpvm.ViewModels
{
    /// <summary>
    /// Represents a point.
    /// </summary>
    [DebuggerDisplay("({X}:{Y})")]
    public class PointModel : ObservableObject
    {
        /// <summary>
        /// Gets or sets the X-coordinate value of the point.
        /// </summary>
        public double X { get; set; }

        /// <summary>
        /// Gets or sets the Y-coordinate value of the point.
        /// </summary>
        public double Y { get; set; }

        /// <summary>
        /// Gets or sets the radius of the point.
        /// </summary>
        public double Radius { get; set; }
    }
}
