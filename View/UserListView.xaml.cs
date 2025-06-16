using System.Windows;
using System.Windows.Input;
using TCC_MVVM.ViewModel;

namespace TCC_MVVM.View
{
    public partial class UserListView : Window
    {
        public UserListView()
        {
            InitializeComponent();
            if (DataContext is UserListViewModel vm) {
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
