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
    }
}
