using System.Collections.ObjectModel;
using TriumLabs.Core.ComponentModel;
using TriumLabs.Core.Presentation.Mvpvm;

namespace TriumLabs.NonIntersectingLinesDrawer.Presentation.Mvpvm.ViewModels
{
    /// <summary>
    /// Represents the MainView's view model.
    /// </summary>
    public class MainViewModel : ViewModelBase
    {
        /// <summary>
        /// Gets or sets the view's title.
        /// </summary>
        public string ViewTitle { get; set; }

        /// <summary>
        /// Gets or sets the usage text.
        /// </summary>
        public string UsageText { get; set; }

        /// <summary>
        /// Gets the list of pinned points.
        /// </summary>
        [IgnoreObservation]
        public ObservableCollection<PointModel> PinnedPoints { get; private set; }

        /// <summary>
        /// 
        /// </summary>
        public ObservableCollection<CurveModel> Curves { get; private set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="MainViewModel"/> class.
        /// </summary>
        public MainViewModel()
        {
            PinnedPoints = new ObservableCollection<PointModel>();
            Curves = new ObservableCollection<CurveModel>();
        }
    }
}
