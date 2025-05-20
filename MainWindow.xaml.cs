using System.Windows;
using TCC_MVVM.ViewModel;

namespace TCC_MVVM;

public partial class MainWindow : Window
{
    public MainWindow()
    {
        InitializeComponent();
        MainWindowViewModel viewModel = new MainWindowViewModel();
        DataContext = viewModel;
    }
}