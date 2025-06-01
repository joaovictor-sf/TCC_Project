using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using TCC_MVVM.ViewModel;

namespace TCC_MVVM.View
{
    
    public partial class LoginView : Window
    {
        public LoginView()
        {
            InitializeComponent();
            DataContextChanged += (s, e) =>
            {
                if (e.NewValue is LoginViewModel vm) {
                    vm.MinimizeWindow = () => WindowState = WindowState.Minimized;
                    vm.CloseWindow = Close;
                }
            };
        }

        private void Window_MouseDown(object sender, MouseButtonEventArgs e) {
            if (e.ChangedButton == MouseButton.Left) {
                DragMove();
            }
        }

        private void btnMinimize_Click(object sender, RoutedEventArgs e) {
            WindowState = WindowState.Minimized;
        }

        private void btnClose_Click(object sender, RoutedEventArgs e) {
            Application.Current.Shutdown();
        }
    }
}
