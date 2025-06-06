using System.Windows.Input;
using System.Windows;
using TCC_MVVM.Infra;
using TCC_MVVM.Util;
using TCC_MVVM.View;
using TCC_MVVM.Model.Enum;
using TCC_MVVM.Model;

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
        public ICommand MinimizeCommand { get; }
        public ICommand CloseCommand { get; }
        public Action? MinimizeWindow { get; set; }
        public Action? CloseWindow { get; set; }

        public LoginViewModel() {
            LoginCommand = new RelayCommand(ExecuteLoginCommand, CanExecuteLoginCommand);
            RecoverPasswordCommand = new RelayCommand(ExecuteRecoverPasswordCommand);

            MinimizeCommand = new RelayCommand(_ => MinimizeWindow?.Invoke());
            CloseCommand = new RelayCommand(_ => CloseWindow?.Invoke());
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
                MessageBox.Show($"Bem-vindo {user.Name} {user.LastName}!", "Login", MessageBoxButton.OK, MessageBoxImage.Information);

                Application.Current.Dispatcher.Invoke(() =>
                {
                    if (user.MustChangePassword) {
                        var changePwdWindow = new ChangePasswordView();
                        var changePwdVM = new ChangePasswordViewModel(user)
                        {
                            CloseWindow = () => changePwdWindow.Close(),
                            MinimizeWindow = () => changePwdWindow.WindowState = WindowState.Minimized
                        };
                        changePwdWindow.DataContext = changePwdVM;
                        changePwdWindow.Owner = Application.Current.Windows.OfType<Window>().FirstOrDefault(w => w.IsActive);
                        changePwdWindow.ShowDialog();
                    } else {
                        Window nextWindow = user.Role switch
                        {
                            UserRole.DEV => CriarMonitorView(user),
                            UserRole.RH or UserRole.ADMIN => new UserListView(),
                            _ => null
                        };

                        if (nextWindow != null) {
                            nextWindow.Show();
                        }
                        IsViewVisible = false;
                    }
                });

                //IsViewVisible = false;
            } else {
                ErrorMessage = "* Invalid username or password";
            }
        }

        private Window CriarMonitorView(UserModel user) {
            var view = new MonitorView();
            var vm = new MonitorViewModel(user)
            {
                CloseWindow = () => view.Close(),
                MinimizeWindow = () => view.WindowState = WindowState.Minimized
            };
            view.DataContext = vm;
            return view;
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
