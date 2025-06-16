using System.Windows.Input;
using System.Windows;
using TCC_MVVM.Infra;
using TCC_MVVM.Model.Enum;
using TCC_MVVM.Model;
using TCC_MVVM.Util;

namespace TCC_MVVM.ViewModel
{
    class CadastroViewModel : ViewModelBase {
        private string _nome;
        private string _sobrenome;
        private string _email;
        private string _username;
        private UserRole? _role;
        private WorkHours _workHours;
        private string _mensagem;

        public string Nome {
            get => _nome;
            set {
                _nome = value;
                OnPropertyChanged(nameof(Nome));
                CommandManager.InvalidateRequerySuggested();
            }
        }

        public string Sobrenome {
            get => _sobrenome;
            set {
                _sobrenome = value;
                OnPropertyChanged(nameof(Sobrenome));
                CommandManager.InvalidateRequerySuggested();
            }
        }

        public string Email {
            get => _email;
            set {
                _email = value;
                OnPropertyChanged(nameof(Email));
                CommandManager.InvalidateRequerySuggested();
            }
        }

        public string Username {
            get => _username;
            set {
                _username = value;
                OnPropertyChanged(nameof(Username));
                CommandManager.InvalidateRequerySuggested();
            }
        }

        public UserRole? Role {
            get => _role;
            set {
                _role = value;
                OnPropertyChanged(nameof(Role));
                CommandManager.InvalidateRequerySuggested();
            }
        }

        public WorkHours WorkHours {
            get => _workHours;
            set {
                _workHours = value;
                OnPropertyChanged(nameof(WorkHours));
                CommandManager.InvalidateRequerySuggested();
            }
        }

        public string Mensagem {
            get => _mensagem;
            set { _mensagem = value; OnPropertyChanged(nameof(Mensagem)); }
        }

        public IEnumerable<UserRole> Roles => Enum.GetValues(typeof(UserRole)).Cast<UserRole>();
        public IEnumerable<WorkHours> WorkHourOptions => Enum.GetValues(typeof(WorkHours)).Cast<WorkHours>();

        public ICommand CadastrarCommand { get; }
        public ICommand MinimizeCommand { get; }
        public ICommand CloseCommand { get; }
        public Action? MinimizeWindow { get; set; }
        public Action? CloseWindow { get; set; }

        public CadastroViewModel() {
            CadastrarCommand = new RelayCommand(ExecuteCadastrar, CanExecuteCadastrar);
            MinimizeCommand = new RelayCommand(_ => MinimizeWindow?.Invoke());
            CloseCommand = new RelayCommand(_ => CloseWindow?.Invoke());
        }

        private bool CanExecuteCadastrar(object? parameter) {
            return !string.IsNullOrEmpty(Nome) &&
                   !string.IsNullOrEmpty(Sobrenome) &&
                   !string.IsNullOrEmpty(Email) &&
                   EmailValido(Email) &&
                   !string.IsNullOrEmpty(Username) &&
                   Role != default &&
                   WorkHours != default;
        }

        private async void ExecuteCadastrar(object? parameter) {
            try {
                using var db = new AppDbContext();

                //Essa parte não está funcionando
                if (db.Users.Any(u => u.Username == Username)) {
                    Mensagem = "Esse username já está em uso.";
                    return;
                }

                if (!EmailValido(Email)) {
                    Mensagem = "E-mail inválido.";
                    return;
                }//Até aqui

                string senhaGerada = Guid.NewGuid().ToString("N")[..8];
                string senhaCriptografada = BCrypt.Net.BCrypt.HashPassword(senhaGerada);

                var novoUsuario = new UserModel
                {
                    Username = Username,
                    PasswordHash = senhaCriptografada,
                    Name = Nome,
                    LastName = Sobrenome,
                    Email = Email,
                    Role = Role.Value,
                    WorkHours = WorkHours,
                    IsActive = true,
                    MustChangePassword = true
                };

                db.Users.Add(novoUsuario);
                db.SaveChanges();

                Nome = string.Empty;
                Sobrenome = string.Empty;
                Email = string.Empty;
                Username = string.Empty;
            } catch (Exception ex) {
                MessageBox.Show($"Erro ao cadastrar: {ex}", "Erro", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private bool EmailValido(string email) {
            return System.Text.RegularExpressions.Regex.IsMatch(email,
                @"^[^@\s]+@[^@\s]+\.[^@\s]+$");
        }
    }
}
