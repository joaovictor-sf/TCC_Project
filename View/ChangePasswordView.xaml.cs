using System.Windows;
using System.Windows.Input;
using TCC_MVVM.ViewModel;

namespace TCC_MVVM.View
{
    public partial class ChangePasswordView : Window
    {
        public ChangePasswordView()
        {
            InitializeComponent();
        }

        private void Window_MouseDown(object sender, MouseButtonEventArgs e) {
            if (e.ChangedButton == MouseButton.Left) {
                DragMove();
            }
        }
    }
}
