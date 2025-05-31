using System.Windows.Input;
using System.Windows;
using TCC_MVVM.Infra;
using TCC_MVVM.Util;
using TCC_MVVM.View;

namespace TCC_MVVM.ViewModel
{
    class LoginViewModel : ViewModelBase {
        // Fields
        private string _username;
        private string _password;
        private string _errorMessage;
        private bool _isViewVisible = true;

        public string Username {
            get => _username; set {
                _username = value;
                OnPropertyChanged(nameof(Username));
                CommandManager.InvalidateRequerySuggested();
            }
        }
        public string Password {
            get => _password; set {
                _password = value;
                OnPropertyChanged(nameof(Password));
                CommandManager.InvalidateRequerySuggested();
            }
        }
        public string ErrorMessage {
            get => _errorMessage; set {
                _errorMessage = value;
                OnPropertyChanged(nameof(ErrorMessage));
            }
        }
        public bool IsViewVisible {
            get => _isViewVisible; set {
                _isViewVisible = value;
                OnPropertyChanged(nameof(IsViewVisible));
            }
        }

        // Commands
        public RelayCommand LoginCommand { get; }
        public RelayCommand RecoverPasswordCommand { get; }
        public RelayCommand ShowPasswordCommand { get; }
        public RelayCommand RememberPasswordCommand { get; }

        public LoginViewModel() {
            LoginCommand = new RelayCommand(ExecuteLoginCommand, CanExecuteLoginCommand);
            RecoverPasswordCommand = new RelayCommand(ExecuteRecoverPasswordCommand);
        }

        private void ExecuteLoginCommand(object? parameter) {
            bool validData;
            if (string.IsNullOrEmpty(Username) || Username.Length < 3 || string.IsNullOrEmpty(Password) || Password.Length < 3) {
                validData = false;
            } else {
                validData = true;
            }

            using var db = new AppDbContext();

            var user = db.Users.FirstOrDefault(u => u.Username == Username);

            if (user != null && BCrypt.Net.BCrypt.Verify(Password, user.PasswordHash)) {
                // abrir MainView, por exemplo
                MessageBox.Show($"Welcome {user.Name} {user.LastName}");

                Application.Current.Dispatcher.Invoke(() =>
                {
                    //var main = new MainWindow(user);
                    var main = new UserListView();
                    main.Show();
                });

                IsViewVisible = false;
            } else {
                ErrorMessage = "* Invalid username or password";
            }
        }

        private bool CanExecuteLoginCommand(object? parameter) {
            return !string.IsNullOrEmpty(Username) && !string.IsNullOrEmpty(Password);
        }

        private void ExecuteRecoverPasswordCommand(object? parameter) {
            // Simulate a password recovery process
            ErrorMessage = "Password recovery link sent to your email.";
        }
    }
}
