using System.Windows;
using System.Windows.Input;
using TCC_MVVM.ViewModel;

namespace TCC_MVVM.View
{
    /// <summary>
    /// Interaction logic for CadastroView.xaml
    /// </summary>
    public partial class CadastroView : Window
    {
        public CadastroView()
        {
            InitializeComponent();
            if (DataContext is CadastroViewModel vm) {
                vm.MinimizeWindow = () => WindowState = WindowState.Minimized;
                vm.CloseWindow = () => Close();
            }
        }

        private void Window_MouseDown(object sender, MouseButtonEventArgs e) {
            if (e.ChangedButton == MouseButton.Left) {
                DragMove();
            }
        }
    }
}
