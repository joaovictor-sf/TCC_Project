using System.Windows;
using System.Windows.Controls;

namespace TCC_MVVM.UserControls
{
    public partial class BindablePasswordBox : UserControl
    {
        public static readonly DependencyProperty PasswordProperty = DependencyProperty.Register("Password", typeof(string), typeof(BindablePasswordBox));

        public string Password {
            get {
                return (string)GetValue(PasswordProperty);
            }
            set {
                SetValue(PasswordProperty, value);
            }
        }

        public BindablePasswordBox() {
            InitializeComponent();
            txtPassword.PasswordChanged += OnPasswordChanged;
        }

        private void OnPasswordChanged(object sender, RoutedEventArgs e) {
            Password = txtPassword.Password;
        }
    }
}
