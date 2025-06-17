using System.Windows;
using System.Windows.Input;
using TCC_MVVM.Model;
using TCC_MVVM.MVVM.ViewModel;

namespace TCC_MVVM.View
{
    public partial class CadastroView : Window
    {
        public CadastroView(UserModel usuarioLogado) {
            var viewModel = new CadastroViewModel(usuarioLogado);
            viewModel.MinimizeWindow = () => WindowState = WindowState.Minimized;
            viewModel.CloseWindow = () => Close();

            DataContext = viewModel;

            InitializeComponent();
        }
        /*public CadastroView()
        {
            InitializeComponent();
            if (DataContext is CadastroViewModel vm) {
                vm.MinimizeWindow = () => WindowState = WindowState.Minimized;
                vm.CloseWindow = () => Close();
            }
        }*/

        private void Window_MouseDown(object sender, MouseButtonEventArgs e) {
            if (e.ChangedButton == MouseButton.Left) {
                DragMove();
            }
        }
    }
}
