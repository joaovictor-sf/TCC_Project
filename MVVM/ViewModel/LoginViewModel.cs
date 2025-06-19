using System.Windows.Input;
using System.Windows;
using TCC_MVVM.Infra;
using TCC_MVVM.View;
using TCC_MVVM.Model.Enum;
using TCC_MVVM.Model;
using TCC_MVVM.MVVM.Commands;
using TCC_MVVM.MVVM.Base;

namespace TCC_MVVM.MVVM.ViewModel
{
    /// <summary>
    /// ViewModel responsável pela lógica de autenticação do sistema.
    /// Gerencia os dados de entrada do usuário, autenticação, controle de janela
    /// e navegação para as respectivas telas conforme o papel do usuário.
    /// </summary>
    class LoginViewModel : ViewModelBase {
        private string _username;
        private string _password;
        private string _errorMessage;

        /// <summary>
        /// Nome de usuário informado na tela de login.
        /// </summary>
        public string Username {
            get => _username; set {
                _username = value;
                OnPropertyChanged(nameof(Username));
                CommandManager.InvalidateRequerySuggested();
            }
        }
        /// <summary>
        /// Senha informada na tela de login.
        /// </summary>
        public string Password {
            get => _password; set {
                _password = value;
                OnPropertyChanged(nameof(Password));
                CommandManager.InvalidateRequerySuggested();
            }
        }
        /// <summary>
        /// Mensagem de erro exibida ao usuário, caso a autenticação falhe.
        /// </summary>
        public string ErrorMessage {
            get => _errorMessage; set {
                _errorMessage = value;
                OnPropertyChanged(nameof(ErrorMessage));
            }
        }

        /// <summary>
        /// Comando que executa o processo de login.
        /// </summary>
        public RelayCommand LoginCommand { get; }
        /// <summary>
        /// Comando que simula envio de link de recuperação de senha.
        /// </summary>
        public RelayCommand RecoverPasswordCommand { get; }

        /// <summary>
        /// Comando para minimizar a janela.
        /// </summary>
        public ICommand MinimizeCommand { get; }
        /// <summary>
        /// Comando para fechar a janela.
        /// </summary>
        public ICommand CloseCommand { get; }
        /// <summary>
        /// Ação a ser executada para minimizar a janela associada.
        /// </summary>
        public Action? MinimizeWindow { get; set; }
        /// <summary>
        /// Ação a ser executada para fechar a janela associada.
        /// </summary>
        public Action? CloseWindow { get; set; }

        /// <summary>
        /// Inicializa os comandos do ViewModel.
        /// </summary>
        public LoginViewModel() {
            LoginCommand = new RelayCommand(ExecuteLoginCommand, CanExecuteLoginCommand);
            RecoverPasswordCommand = new RelayCommand(ExecuteRecoverPasswordCommand);

            MinimizeCommand = new RelayCommand(_ => MinimizeWindow?.Invoke());
            CloseCommand = new RelayCommand(_ => CloseWindow?.Invoke());
        }

        /// <summary>
        /// Executa o processo de login com validação e redirecionamento de acordo com o papel do usuário.
        /// </summary>
        private void ExecuteLoginCommand(object? parameter) {
            using var db = new AppDbContext();

            var user = db.Users.FirstOrDefault(u => u.Username == Username);

            if (user != null && BCrypt.Net.BCrypt.Verify(Password, user.PasswordHash)) {

                if (!user.IsActive) {
                    ErrorMessage = "* Usuário desativado. Contate o administrador.";
                    return;
                }

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
                            UserRole.RH or UserRole.ADMIN => CriarUserListView(user),
                            _ => null
                        };

                        if (nextWindow != null) {
                            nextWindow.Show();
                        }
                        Application.Current.Windows
                        .OfType<Window>()
                        .FirstOrDefault(w => w.DataContext == this)?
                        .Close();
                    }
                });
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

        /// <summary>
        /// Cria e configura a janela de lista de usuários (usada por RH e Admin).
        /// </summary>
        private Window CriarUserListView(UserModel user) {
            var view = new UserListView();
            var vm = new UserListViewModel(user)
            {
                CloseWindow = () => view.Close(),
                MinimizeWindow = () => view.WindowState = WindowState.Minimized
            };
            view.DataContext = vm;
            return view;
        }

        /// <summary>
        /// Valida se o comando de login pode ser executado.
        /// </summary>
        private bool CanExecuteLoginCommand(object? parameter) {
            return !string.IsNullOrEmpty(Username) && !string.IsNullOrEmpty(Password);
        }

        /// <summary>
        /// Simula o envio de recuperação de senha.
        /// </summary>
        private void ExecuteRecoverPasswordCommand(object? parameter) {
            ErrorMessage = "Password recovery link sent to your email.";
        }

        /// <summary>
        /// Reseta os campos do formulário de login.
        /// </summary>
        public void Reset() {
            Username = string.Empty;
            Password = string.Empty;
            ErrorMessage = string.Empty;
        }
    }
}
