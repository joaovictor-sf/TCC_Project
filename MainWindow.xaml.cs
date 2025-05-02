using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
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
        viewModel.SaveLogsToDatabase();

        btnSTOP.IsEnabled = false;
        btnStart.IsEnabled = true;
    }

    private void btnStart_Click(object sender, RoutedEventArgs e) {
        btnSTOP.IsEnabled = true;
        btnStart.IsEnabled = false;

        timer = new DispatcherTimer();
        timer.Interval = TimeSpan.FromSeconds(10); // ajustável
        timer.Tick += (s, args) => viewModel.CollectActiveProcesses();
        timer.Start();
    }
}