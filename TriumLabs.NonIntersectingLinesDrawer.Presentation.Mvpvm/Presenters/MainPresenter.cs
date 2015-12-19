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
        /// <summary>
        /// Handles the View's Loaded event.
        /// </summary>
        public override void OnViewLoaded()
        {
            base.OnViewLoaded();

            ViewModel.ViewTitle = "Non-intersecting Lines Drawer v0.1";
            ViewModel.UsageText = "To draw a non-intersecting, continuous line, select the start and end points";
        }
    }
}
