using System.Windows;

namespace Shos.UndoRedoList.SampleApp.Views
{
    using ViewModels;

    public partial class MainWindow : Window
    {
        MainViewModel viewModel = new MainViewModel();

        public MainWindow()
        {
            InitializeComponent();
            DataContext = viewModel;
            Closing += (_, __) => viewModel.Dispose();
        }
    }
}
