using System.Linq;
using TriumLabs.Core.Presentation.Mvpvm;
using TriumLabs.NonIntersectingLinesDrawer.Presentation.Mvpvm.ViewModels;
using TriumLabs.NonIntersectingLinesDrawer.Presentation.Mvpvm.Views;

namespace TriumLabs.NonIntersectingLinesDrawer.Presentation.Mvpvm.Presenters
{
    /// <summary>
    /// Represents the MainView's presenter.
    /// </summary>
    public class MainPresenter : PresenterBase<IMainView, MainViewModel>
    {
        private const double DefaultPinnedPointRadius = 15.0;

        /// <summary>
        /// Handles the View's Loaded event.
        /// </summary>
        public override void OnViewLoaded()
        {
            base.OnViewLoaded();

            ViewModel.ViewTitle = "Non-intersecting Lines Drawer v0.1";
            ViewModel.UsageText = "To draw a non-intersecting, continuous line, select the start and end points";
        }

        /// <summary>
        /// Handles the command to pin a point.
        /// </summary>
        /// <param name="x">The X-coordinate to pin the point.</param>
        /// <param name="y">The Y-coordinate to pin the point.</param>
        public void HandleCommandPinPoint(double x, double y)
        {
            ViewModel.PinnedPoints.Add(new PointModel { X = x, Y = y, Radius = DefaultPinnedPointRadius });

            if (ViewModel.PinnedPoints.Count > 0 && ViewModel.PinnedPoints.Count % 2 == 0)
            {
                var modelCurve = new CurveModel(ViewModel.PinnedPoints.Skip(ViewModel.PinnedPoints.Count - 2));
                ViewModel.Curves.Add(modelCurve);
            }
        }
    }
}
