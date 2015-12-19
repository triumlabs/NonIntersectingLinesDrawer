using System;
using TriumLabs.Core.Threading;

namespace TriumLabs.Core.Presentation.Mvpvm
{
    /// <summary>
    /// Represents a base class for Presenter.
    /// </summary>
    public abstract class PresenterBase
    {
        #region Properties

        /// <summary>
        /// Gets the dispatcher to invoke actions on the view's thread.
        /// </summary>
        protected Dispatcher Dispatcher { get; private set; }

        /// <summary>
        /// Gets the View.
        /// </summary>
        public IView View { get; protected set; }

        /// <summary>
        /// Gets the View Model.
        /// </summary>
        public IViewModel ViewModel { get; protected set; }

        #endregion

        #region Constructors

        /// <summary>
        /// Initializes a new instance of the <see cref="T:PresenterBase"/> class.
        /// </summary>
        protected PresenterBase()
        {
            Dispatcher = new Dispatcher();
        }

        #endregion

        #region Methods

        /// <summary>
        /// Sets the View.
        /// </summary>
        /// <param name="view">The view.</param>
        protected virtual void SetView(IView view)
        {
            if (view == null) throw new ArgumentNullException("view");

            View = view;
            ViewModel = CreateViewModel();
        }

        /// <summary>
        /// Creates the View Model.
        /// </summary>
        protected abstract IViewModel CreateViewModel();

        /// <summary>
        /// Handles the View's Loaded event.
        /// </summary>
        public virtual void OnViewLoaded()
        {
        }

        /// <summary>
        /// Handles the view's Disposed event.
        /// </summary>
        public virtual void OnViewDisposed()
        {
        }

        #endregion
    }
    /// <summary>
    /// Represents a base class for Presenter.
    /// </summary>
    /// <typeparam name="TView">The type of View.</typeparam>
    /// <typeparam name="TViewModel">The type of View Model.</typeparam>
    public abstract class PresenterBase<TView, TViewModel> : PresenterBase, IPresenter<TView, TViewModel>
        where TView : class, IView
        where TViewModel : class, IViewModel, new()
    {
        #region PresenterBase Members

        /// <summary>
        /// Sets the View.
        /// </summary>
        /// <param name="view">The view.</param>
        protected override void SetView(IView view)
        {
            if (!(view is TView))
                throw new ViewTypeMismatchException(typeof(TView), view.GetType());

            base.SetView(view);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        protected override IViewModel CreateViewModel()
        {
            return new TViewModel();
        }

        #endregion

        #region IPresenter Members

        /// <summary>
        /// Gets or sets the View.
        /// </summary>
        public new TView View
        {
            get { return (TView)base.View; }
            set { SetView(value); }
        }

        /// <summary>
        /// Gets or sets the View.
        /// </summary>
        IView IPresenter.View
        {
            get { return base.View; }
            set { SetView(value); }
        }


        /// <summary>
        /// Gets the View Model.
        /// </summary>
        public new TViewModel ViewModel
        {
            get { return (TViewModel)base.ViewModel; }
        }

        /// <summary>
        /// Gets the View Model.
        /// </summary>
        IViewModel IPresenter.ViewModel
        {
            get { return base.ViewModel; }
        }

        #endregion
    }
}
