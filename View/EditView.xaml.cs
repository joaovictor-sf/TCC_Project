using System.Windows;
using System.Windows.Input;
using TCC_MVVM.ViewModel;

namespace TCC_MVVM.View
{
    public partial class EditView : Window
    {
        public EditView()
        {
            InitializeComponent();
            DataContextChanged += (s, e) =>
            {
                if (e.NewValue is EditViewModel vm) {
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
    }
}
