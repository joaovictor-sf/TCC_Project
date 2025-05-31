using System.Windows;
using System.Windows.Input;
using TCC_MVVM.ViewModel;

namespace TCC_MVVM.View
{
    /// <summary>
    /// Interaction logic for UserList.xaml
    /// </summary>
    public partial class UserListView : Window
    {
        public UserListView()
        {
            InitializeComponent();
            if (DataContext is UserListViewModel vm) {
                vm.MinimizeWindow = () => WindowState = WindowState.Minimized;
                vm.CloseWindow = () => Application.Current.Shutdown();
            }
        }

        private void Window_MouseDown(object sender, MouseButtonEventArgs e) {
            if (e.ChangedButton == MouseButton.Left) {
                DragMove();
            }
        }
    }
}
