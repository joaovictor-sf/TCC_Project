using System.Windows;
using System.Windows.Threading;
using TCC_WPF.ViewModel;

namespace TCC_WPF;

public partial class MainWindow : Window
{
    private MainWindowViewModel viewModel;
    private DispatcherTimer timer;

    public MainWindow()
    {
        viewModel = new MainWindowViewModel();
        InitializeComponent();
    }

    private void btnSTOP_Click(object sender, RoutedEventArgs e) {
        timer.Stop();
        viewModel.StopMonitoring();

        viewModel.SaveLogsToDatabase();
        viewModel.SaveInactivityLogToDatabase();

        btnSTOP.IsEnabled = false;
        btnStart.IsEnabled = true;
    }

    private void btnStart_Click(object sender, RoutedEventArgs e) {
        btnSTOP.IsEnabled = true;
        btnStart.IsEnabled = false;

        viewModel.StartMonitoring();

        timer = new DispatcherTimer();
        timer.Interval = TimeSpan.FromSeconds(10); // ajustável
        timer.Tick += (s, args) => viewModel.CollectActiveProcesses();
        timer.Start();
    }
}