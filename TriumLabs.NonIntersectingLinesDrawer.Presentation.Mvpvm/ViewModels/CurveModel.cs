using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using TriumLabs.Core.Collections;
using TriumLabs.Core.ComponentModel;

namespace TriumLabs.NonIntersectingLinesDrawer.Presentation.Mvpvm.ViewModels
{
    /// <summary>
    /// Represents a curve (path).
    /// </summary>
    public sealed class CurveModel : ObservableObject
    {
        /// <summary>
        /// Gets the start point of the curve.
        /// </summary>
        public PointModel StartPoint { get { return Points.FirstOrDefault(); } }

        /// <summary>
        /// Gets the end point of the curve.
        /// </summary>
        public PointModel EndPoint { get { return Points.LastOrDefault(); } }

        /// <summary>
        /// Gets the list of corner points of the curve.
        /// </summary>
        [IgnoreObservation]
        public ObservableCollection<PointModel> Points { get; private set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="CurveModel"/> class.
        /// </summary>
        public CurveModel()
        {
            Points = new ObservableCollection<PointModel>();
            Points.CollectionChanged += (sender, e) =>
                {
                    RaiseEventPropertyChanged("StartPoint");
                    RaiseEventPropertyChanged("EndPoint");
                };
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="CurveModel"/> class.
        /// </summary>
        /// <param name="points">The list of corner points.</param>
        public CurveModel(IEnumerable<PointModel> points) : this()
        {
            points.ForEach(point => Points.Add(point));
        }
    }
}
