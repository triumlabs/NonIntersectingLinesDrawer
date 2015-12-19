using System.ComponentModel;
using System.Windows;

namespace TriumLabs.Core.Presentation.Mvpvm.Wpf
{
    /// <summary>
    /// Represents a base class for Window View.
    /// </summary>
    public abstract class WindowView : Window, IView
    {
        #region Constructors

        /// <summary>
        /// 
        /// </summary>
        protected WindowView()
        {
            //DefaultStyleKey = typeof(Window);
            //Background = Brushes.White;
        }

        #endregion

        #region IView Members

        /// <summary>
        /// Gets the Presenter.
        /// </summary>
        public IPresenter Presenter { get; protected set; }

        #endregion
    }

    /// <summary>
    /// Represents a base class for Window View.
    /// </summary>
    /// <typeparam name="TPresenter">The type of Presenter.</typeparam>
    /// <typeparam name="TViewModel">The type of View Model.</typeparam>
    public abstract class WindowView<TPresenter, TViewModel> : WindowView, IView<TPresenter>
        where TPresenter : class, IPresenter, new()
        where TViewModel : class, IViewModel
    {
        #region Properties

        /// <summary>
        /// Gets the View Model.
        /// </summary>
        public TViewModel ViewModel
        {
            get { return Presenter != null ? Presenter.ViewModel as TViewModel : null; }
        }

        /// <summary>
        /// Determines whether View is in Design mode.
        /// </summary>
        protected bool DesignTime
        {
            get
            {
                return DesignerProperties.GetIsInDesignMode(this);
            }
        }

        #endregion

        #region Constructors

        /// <summary>
        /// Initializes a new instance of the FormView class.
        /// </summary>
        protected WindowView()
        {
            if (!DesignTime)
            {
                SetPresenter();
                ViewModel.PropertyChanged += HandleEventViewModelPropertyChanged;
                DataContext = this;

                Loaded += OnViewLoaded;
                Unloaded += OnViewUnloaded;
            }
        }

        #endregion

        #region Methods

        private void SetPresenter()
        {
            Presenter = new TPresenter();
            Presenter.View = this;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnViewLoaded(object sender, RoutedEventArgs e)
        {
            Presenter.OnViewLoaded();

            Loaded -= OnViewLoaded;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnViewUnloaded(object sender, RoutedEventArgs e)
        {
            Presenter.OnViewDisposed();

            ViewModel.PropertyChanged -= HandleEventViewModelPropertyChanged;
            Unloaded -= OnViewUnloaded;
        }

        /// <summary>
        /// Handles ViewModel.PropertyChanged event.
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The <see cref="PropertyChangedEventArgs"/> instance containing the event data.</param>
        protected virtual void HandleEventViewModelPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
        }

        #endregion

        #region IView Members

        /// <summary>
        /// Gets the Presenter.
        /// </summary>
        IPresenter IView.Presenter
        {
            get { return base.Presenter; }
        }

        /// <summary>
        /// Gets the Presenter.
        /// </summary>
        public new TPresenter Presenter
        {
            get { return base.Presenter as TPresenter; }
            protected set { base.Presenter = value; }
        }

        #endregion
    }
}
