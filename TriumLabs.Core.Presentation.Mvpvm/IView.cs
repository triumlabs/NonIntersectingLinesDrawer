namespace TriumLabs.Core.Presentation.Mvpvm
{
    /// <summary>
    /// Defines a View.
    /// </summary>
    public interface IView
    {
        /// <summary>
        /// Gets the Presenter.
        /// </summary>
        IPresenter Presenter { get; }
    }

    /// <summary>
    /// Defines a View.
    /// </summary>
    /// <typeparam name="TPresenter">The type of Presenter.</typeparam>
    public interface IView<out TPresenter> : IView
        where TPresenter : IPresenter
    {
        /// <summary>
        /// Gets the Presenter.
        /// </summary>
        new TPresenter Presenter { get; }
    }
}
