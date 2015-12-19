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
        private const double DefaultPinnedPointSize = 30.0;

        /// <summary>
        /// Handles the View's Loaded event.
        /// </summary>
        public override void OnViewLoaded()
        {
            base.OnViewLoaded();

            ViewModel.ViewTitle = "Non-intersecting Lines Drawer v0.1";
            ViewModel.UsageText = "To draw a non-intersecting, continuous line, select the start and end points";
            ViewModel.PinnedPointSize = DefaultPinnedPointSize;
        }

        /// <summary>
        /// Handles the command to pin a point.
        /// </summary>
        /// <param name="x">The X-coordinate to pint the point.</param>
        /// <param name="y">The Y-coordinate to pint the point.</param>
        public void HandleCommandPinPoint(double x, double y)
        {
            ViewModel.PinnedPoints.Add(new LinePoint 
            {
                X = x - ViewModel.PinnedPointSize / 2,
                Y = y - ViewModel.PinnedPointSize / 2
            });
        }
    }
}
