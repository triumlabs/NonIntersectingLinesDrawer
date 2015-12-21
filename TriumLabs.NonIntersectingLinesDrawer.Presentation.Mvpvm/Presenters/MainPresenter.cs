using System.Collections.Generic;
using System.Linq;
using System.Windows;
using TriumLabs.Core.Presentation.Mvpvm;
using TriumLabs.NonIntersectingLinesDrawer.BusinessServices;
using TriumLabs.NonIntersectingLinesDrawer.BusinessServices.Model;
using TriumLabs.NonIntersectingLinesDrawer.Presentation.Mvpvm.ViewModels;
using TriumLabs.NonIntersectingLinesDrawer.Presentation.Mvpvm.Views;

namespace TriumLabs.NonIntersectingLinesDrawer.Presentation.Mvpvm.Presenters
{
    /// <summary>
    /// Represents the MainView's presenter.
    /// </summary>
    public class MainPresenter : PresenterBase<IMainView, MainViewModel>
    {
        private const double DefaultPinnedPointRadius = 5.0;
        private ICurveService srvCurve;

        /// <summary>
        /// Handles the View's Loaded event.
        /// </summary>
        public override void OnViewLoaded()
        {
            base.OnViewLoaded();

            srvCurve = new CurveService();

            ViewModel.ViewTitle = "Non-intersecting Lines Drawer v0.1";
            ViewModel.UsageText = "To draw a non-intersecting, continuous line, select the start and end points";
            ViewModel.ClearCommandText = "Clear board!";
        }

        /// <summary>
        /// Handles the command to clear board.
        /// </summary>
        public void HandleCommandClearBoard()
        {
            ViewModel.Curves.Clear();
            ViewModel.PinnedPoints.Clear();
        }

        /// <summary>
        /// Handles the command to pin a point.
        /// </summary>
        /// <param name="x">The X-coordinate to pin the point.</param>
        /// <param name="y">The Y-coordinate to pin the point.</param>
        public void HandleCommandPinPoint(double x, double y)
        {
            ViewModel.PinnedPoints.Add(new PointModel { X = x, Y = y, Radius = DefaultPinnedPointRadius });

            // Calculates a curve when there are 2 new pinned points
            if (ViewModel.PinnedPoints.Count > 0 && ViewModel.PinnedPoints.Count % 2 == 0)
            {
                var modelPointA = ViewModel.PinnedPoints[ViewModel.PinnedPoints.Count - 2];
                var modelPointB = ViewModel.PinnedPoints[ViewModel.PinnedPoints.Count - 1];
                var curveNew = srvCurve.FindNonIntersectingCurve(
                    new Vector(modelPointA.X, modelPointA.Y),
                    new Vector(modelPointB.X, modelPointB.Y),
                    ViewModel.Curves.Select(MapCurveModelToCurve));

                if (curveNew != null && curveNew.Segments.Any())
                {
                    var modelCurve = new CurveModel(
                        curveNew.Segments
                            .Take(1)
                            .Select(segment => segment.StartVector)
                        .Concat(curveNew.Segments
                            .Select(segment => segment.EndVector))
                        .Select(vector => new PointModel { X = vector.X, Y = vector.Y, Radius = DefaultPinnedPointRadius }));
                    ViewModel.Curves.Add(modelCurve);
                }
            }
        }

        /// <summary>
        /// Maps a curve model into a curve business object.
        /// </summary>
        /// <param name="modelCurve">The curve model to map.</param>
        /// <returns>The curve business object.</returns>
        private static Curve MapCurveModelToCurve(CurveModel modelCurve)
        {
            return new Curve(MapCurveModelToLineSegments(modelCurve));
        }

        /// <summary>
        /// Maps the list of curve model's points into a list of line segment business objects.
        /// </summary>
        /// <param name="modelCurve">The curve model to map.</param>
        /// <returns>The list of line segment business objects.</returns>
        private static IEnumerable<LineSegment> MapCurveModelToLineSegments(CurveModel modelCurve)
        {
            return modelCurve.Points
                .Take(modelCurve.Points.Count - 1)
                .Select((point, idx) => new { StartPoint = point, EndPoint = modelCurve.Points[idx + 1] })
                .Select(tuple => new LineSegment
                    {
                        StartVector = new Vector(tuple.StartPoint.X, tuple.StartPoint.Y),
                        EndVector = new Vector(tuple.EndPoint.X, tuple.EndPoint.Y),
                    });
        }

    }
}
