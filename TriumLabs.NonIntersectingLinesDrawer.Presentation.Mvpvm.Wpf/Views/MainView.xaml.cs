using System.Windows;
using System.Windows.Input;
using TriumLabs.NonIntersectingLinesDrawer.Presentation.Mvpvm.Views;

namespace TriumLabs.NonIntersectingLinesDrawer.Presentation.Mvpvm.Wpf.Views
{
    /// <summary>
    /// Interaction logic for MainView.xaml
    /// </summary>
    public partial class MainView : IMainView
    {
        /// <summary>
        /// 
        /// </summary>
        public MainView()
        {
            InitializeComponent();
        }

        private void HandleEventClearButtonClick(object sender, RoutedEventArgs e)
        {
            Presenter.HandleCommandClearBoard();
        }

        private void HandleEventDrawingBoardMouseUp(object sender, MouseButtonEventArgs e)
        {
            var position = e.GetPosition(drawingBoard);
            Presenter.HandleCommandPinPoint(position.X, position.Y);
        }
    }
}
