using System.Windows.Input;
using System.Windows;
using TCC_MVVM.Infra;
using TCC_MVVM.Model.Enum;
using TCC_MVVM.Model;
using TCC_MVVM.MVVM.Commands;
using TCC_MVVM.MVVM.Base;

namespace TCC_MVVM.MVVM.ViewModel
{
    class CadastroViewModel : ViewModelBase {
        private readonly UserModel _usuarioLogado;

        private string _nome;
        private string _sobrenome;
        private string _email;
        private string _username;
        private UserRole? _role;
        private WorkHours _workHours;
        private string _mensagem;

        public string Nome {
            get => _nome;
            set => SetField(ref _nome, value, nameof(Nome));
        }

        public string Sobrenome {
            get => _sobrenome;
            set => SetField(ref _sobrenome, value, nameof(Sobrenome));
        }

        public string Email {
            get => _email;
            set => SetField(ref _email, value, nameof(Email));
        }

        public string Username {
            get => _username;
            set => SetField(ref _username, value, nameof(Username));
        }

        public UserRole? Role {
            get => _role;
            set => SetField(ref _role, value, nameof(Role));
        }
        public IEnumerable<UserRole> AvailableRoles {
            get {
                if (_usuarioLogado.Role == UserRole.RH)
                    return new[] { UserRole.DEV, UserRole.RH };

                return Enum.GetValues(typeof(UserRole)).Cast<UserRole>();
            }
        }
        public WorkHours WorkHours {
            get => _workHours;
            set => SetField(ref _workHours, value, nameof(WorkHours));

        }
        public IEnumerable<WorkHours> AvailableWorkHours {
            get {
                if (_usuarioLogado.Role == UserRole.RH)
                    return new[] { WorkHours.QUATRO_HORAS, WorkHours.SEIS_HORAS, WorkHours.OITO_HORAS };

                return Enum.GetValues(typeof(WorkHours)).Cast<WorkHours>();
            }
        }

        public string Mensagem {
            get => _mensagem;
            set { _mensagem = value; OnPropertyChanged(nameof(Mensagem)); }
        }

        public ICommand CadastrarCommand { get; }
        public ICommand MinimizeCommand { get; }
        public ICommand CloseCommand { get; }
        public Action? MinimizeWindow { get; set; }
        public Action? CloseWindow { get; set; }

        public CadastroViewModel(UserModel user) {
            _usuarioLogado = user;

            CadastrarCommand = new RelayCommand(ExecuteCadastrar);
            MinimizeCommand = new RelayCommand(_ => MinimizeWindow?.Invoke());
            CloseCommand = new RelayCommand(_ => CloseWindow?.Invoke());
        }

        private async void ExecuteCadastrar(object? parameter) {
            try {
                using var db = new AppDbContext();

                if (CamposInvalidos()) {
                    Mensagem = "Preencha todos os campos corretamente.";
                    return;
                }

                if (UsernameEmUso(db)) {
                    Mensagem = "Esse username já está em uso.";
                    return;
                }

                if (EmailInvalido(Email)) {
                    Mensagem = "E-mail inválido.";
                    return;
                }

                string senhaGerada = GerarSenhaAleatoria();
                string senhaCriptografada = Criptografar(senhaGerada);

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

                LimparCampos();

                Mensagem = $"Usuário cadastrado com sucesso!\nSenha gerada: {senhaGerada}";
            } catch (Exception ex) {
                MessageBox.Show($"Erro ao cadastrar: {ex}", "Erro", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void SetField<T>(ref T field, T value, string propertyName) {
            field = value;
            OnPropertyChanged(propertyName);
            CommandManager.InvalidateRequerySuggested();
        }

        private bool UsernameEmUso(AppDbContext db) => db.Users.Any(u => u.Username == Username.Trim());

        private bool EmailInvalido(string email) => !System.Text.RegularExpressions.Regex.IsMatch(email, @"^[^@\s]+@[^@\s]+\.[^@\s]+$");

        private string GerarSenhaAleatoria() => Guid.NewGuid().ToString("N")[..8];

        private string Criptografar(string senha) => BCrypt.Net.BCrypt.HashPassword(senha);

        private void LimparCampos() {
            Nome = string.Empty;
            Sobrenome = string.Empty;
            Email = string.Empty;
            Username = string.Empty;
        }

        private bool CamposInvalidos() =>
            string.IsNullOrWhiteSpace(Nome) ||
            string.IsNullOrWhiteSpace(Sobrenome) ||
            string.IsNullOrWhiteSpace(Email) ||
            string.IsNullOrWhiteSpace(Username) ||
            Role == null ||
            WorkHours == default;
    }
}
