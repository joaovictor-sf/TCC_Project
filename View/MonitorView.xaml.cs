using System.Windows;
using System.Windows.Input;
using TCC_MVVM.MVVM.ViewModel;

namespace TCC_MVVM.View
{
    public partial class MonitorView : Window
    {
        public MonitorView()
        {
            InitializeComponent();
            DataContextChanged += (s, e) =>
            {
                if (e.NewValue is MonitorViewModel vm) {
                    vm.MinimizeWindow = () => WindowState = WindowState.Minimized;
                    vm.CloseWindow = () => Close();
                }
            };

            Closing += MonitorView_Closing;
        }

        private void MonitorView_Closing(object? sender, System.ComponentModel.CancelEventArgs e) {
            if (DataContext is MonitorViewModel vm && vm.IsMonitoring) {
                MessageBox.Show("Você não pode fechar a aplicação enquanto o monitoramento estiver ativo.\nUse o botão 'Stop' ou 'Logout'.",
                    "Aviso", MessageBoxButton.OK, MessageBoxImage.Warning);

                e.Cancel = true;
            }
        }

        private void Window_MouseDown(object sender, MouseButtonEventArgs e) {
            if (e.ChangedButton == MouseButton.Left) {
                DragMove();
            }
        }
    }
}
