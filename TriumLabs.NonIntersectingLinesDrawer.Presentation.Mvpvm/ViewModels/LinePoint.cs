using System.Diagnostics;
using TriumLabs.Core.ComponentModel;

namespace TriumLabs.NonIntersectingLinesDrawer.Presentation.Mvpvm.ViewModels
{
    /// <summary>
    /// Represents a line point.
    /// </summary>
    [DebuggerDisplay("({X}:{Y})={Size}")]
    public class LinePoint : ObservableObject
    {
        /// <summary>
        /// Gets or sets the X-coordinate vaue of the point.
        /// </summary>
        public double X { get; set; }

        /// <summary>
        /// Gets or sets the Y-coordinate vaue of the point.
        /// </summary>
        public double Y { get; set; }
    }
}
