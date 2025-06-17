using System.Windows;
using System.Windows.Input;
using TCC_MVVM.MVVM.ViewModel;

namespace TCC_MVVM.View
{
    
    public partial class LoginView : Window
    {
        public LoginView()
        {
            InitializeComponent();
            var vm = new LoginViewModel
            {
                MinimizeWindow = () => WindowState = WindowState.Minimized,
                CloseWindow = () => Application.Current.Shutdown()
            };

            DataContext = vm;
        }

        private void Window_MouseDown(object sender, MouseButtonEventArgs e) {
            if (e.ChangedButton == MouseButton.Left) {
                DragMove();
            }
        }
    }
}
