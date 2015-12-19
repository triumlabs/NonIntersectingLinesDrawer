using TriumLabs.Core.ComponentModel;

namespace TriumLabs.Core.Presentation.Mvpvm
{
    /// <summary>
    /// Represents a base class for View Model.
    /// </summary>
    public abstract class ViewModelBase : ObservableObject, IViewModel
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ViewModelBase"/> class.
        /// </summary>
        protected ViewModelBase()
        {
        }

    }
}
