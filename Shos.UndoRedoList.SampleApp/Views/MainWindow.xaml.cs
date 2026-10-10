using System.Windows;

namespace Shos.UndoRedoList.SampleApp.Views;

using ViewModels;

public partial class MainWindow : Window
{
    readonly MainViewModel viewModel = new();

    public MainWindow()
    {
        InitializeComponent();
        DataContext = viewModel;
        //Closing += (_, _) => viewModel.Dispose();
    }
}
