namespace TriumLabs.Core.Presentation.Mvpvm
{
    /// <summary>
    /// Defines a Presenter.
    /// </summary>
    public interface IPresenter
    {
        /// <summary>
        /// Gets or sets the View.
        /// </summary>
        IView View { get; set; }

        /// <summary>
        /// Gets the View Model.
        /// </summary>
        IViewModel ViewModel { get; }

        /// <summary>
        /// Handles the View's Loaded event.
        /// </summary>
        void OnViewLoaded();

        /// <summary>
        /// Handles the view's Disposed event.
        /// </summary>
        void OnViewDisposed();
    }

    /// <summary>
    /// Defines a Presenter.
    /// </summary>
    /// <typeparam name="TView">The type of View.</typeparam>
    /// <typeparam name="TViewModel">The type of View Model.</typeparam>
    public interface IPresenter<TView, out TViewModel> : IPresenter
        where TView : class, IView
        where TViewModel : class, IViewModel, new()
    {
        /// <summary>
        /// Gets or sets the View.
        /// </summary>
        new TView View { get; set; }

        /// <summary>
        /// Gets the View Model.
        /// </summary>
        new TViewModel ViewModel { get; }
    }
}
