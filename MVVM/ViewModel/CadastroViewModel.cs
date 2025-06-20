using System.Windows.Input;
using System.Windows;
using TCC_MVVM.Infra;
using TCC_MVVM.Model.Enum;
using TCC_MVVM.Model;
using TCC_MVVM.MVVM.Commands;
using TCC_MVVM.MVVM.Base;

namespace TCC_MVVM.MVVM.ViewModel
{
    /// <summary>
    /// ViewModel responsável por gerenciar a lógica de cadastro de novos usuários.
    /// </summary>
    class CadastroViewModel : ViewModelBase {
        private readonly UserModel _usuarioLogado;

        private string _nome;
        private string _sobrenome;
        private string _email;
        private string _username;
        private UserRole? _role;
        private WorkHours _workHours;
        private string _mensagem;

        /// <summary>
        /// Nome do novo usuário.
        /// </summary>
        public string Nome {
            get => _nome;
            set => SetField(ref _nome, value, nameof(Nome));
        }

        /// <summary>
        /// Sobrenome do novo usuário.
        /// </summary>
        public string Sobrenome {
            get => _sobrenome;
            set => SetField(ref _sobrenome, value, nameof(Sobrenome));
        }

        /// <summary>
        /// E-mail do novo usuário.
        /// </summary>
        public string Email {
            get => _email;
            set => SetField(ref _email, value, nameof(Email));
        }

        /// <summary>
        /// Nome de usuário (username) para login.
        /// </summary>
        public string Username {
            get => _username;
            set => SetField(ref _username, value, nameof(Username));
        }

        /// <summary>
        /// Papel (Role) do novo usuário no sistema.
        /// </summary>
        public UserRole? Role {
            get => _role;
            set => SetField(ref _role, value, nameof(Role));
        }

        /// <summary>
        /// Lista de cargos disponíveis para seleção, com base no cargo do usuário logado.
        /// </summary>
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
        /// <summary>
        /// Carga horária selecionada para o novo usuário.
        /// </summary>
        public IEnumerable<WorkHours> AvailableWorkHours {
            get {
                if (_usuarioLogado.Role == UserRole.RH)
                    return new[] { WorkHours.QUATRO_HORAS, WorkHours.SEIS_HORAS, WorkHours.OITO_HORAS };

                return Enum.GetValues(typeof(WorkHours)).Cast<WorkHours>();
            }
        }

        /// <summary>
        /// Mensagem de feedback exibida na interface (sucesso ou erro).
        /// </summary>
        public string Mensagem {
            get => _mensagem;
            set { _mensagem = value; OnPropertyChanged(nameof(Mensagem)); }
        }

        /// <summary>
        /// Comando executado ao clicar no botão "Cadastrar".
        /// </summary>
        public ICommand CadastrarCommand { get; }
        /// <summary>
        /// Comando para minimizar a janela de cadastro.
        /// </summary>
        public ICommand MinimizeCommand { get; }
        /// <summary>
        /// Comando para fechar a janela de cadastro.
        /// </summary>
        public ICommand CloseCommand { get; }
        /// <summary>
        /// Ação que minimiza a janela (usada para binding externo).
        /// </summary>
        public Action? MinimizeWindow { get; set; }
        /// <summary>
        /// Ação que fecha a janela (usada para binding externo).
        /// </summary>
        public Action? CloseWindow { get; set; }

        /// <summary>
        /// Inicializa o ViewModel de cadastro, definindo comandos e o usuário logado.
        /// </summary>
        /// <param name="user">Usuário atualmente logado, responsável pelo cadastro.</param>
        public CadastroViewModel(UserModel user) {
            _usuarioLogado = user;

            CadastrarCommand = new RelayCommand(ExecuteCadastrar);
            MinimizeCommand = new RelayCommand(_ => MinimizeWindow?.Invoke());
            CloseCommand = new RelayCommand(_ => CloseWindow?.Invoke());
        }

        /// <summary>
        /// Executa o cadastro do novo usuário após validações.
        /// </summary>
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

        /// <summary>
        /// Método auxiliar para setar valores de propriedades e notificar alterações.
        /// </summary>
        private void SetField<T>(ref T field, T value, string propertyName) {
            field = value;
            OnPropertyChanged(propertyName);
            CommandManager.InvalidateRequerySuggested();
        }

        /// <summary>
        /// Verifica se o username já está em uso no banco.
        /// </summary>
        private bool UsernameEmUso(AppDbContext db) => db.Users.Any(u => u.Username == Username.Trim());

        /// <summary>
        /// Verifica se o e-mail informado é inválido.
        /// </summary>
        private bool EmailInvalido(string email) => !System.Text.RegularExpressions.Regex.IsMatch(email, @"^[^@\s]+@[^@\s]+\.[^@\s]+$");

        /// <summary>
        /// Gera uma senha aleatória de 8 caracteres.
        /// </summary>
        private string GerarSenhaAleatoria() => Guid.NewGuid().ToString("N")[..8];

        /// <summary>
        /// Criptografa a senha gerada com BCrypt.
        /// </summary>
        private string Criptografar(string senha) => BCrypt.Net.BCrypt.HashPassword(senha);

        /// <summary>
        /// Limpa os campos do formulário após cadastro.
        /// </summary>
        private void LimparCampos() {
            Nome = string.Empty;
            Sobrenome = string.Empty;
            Email = string.Empty;
            Username = string.Empty;
        }

        /// <summary>
        /// Verifica se algum dos campos obrigatórios está inválido ou vazio.
        /// </summary>
        private bool CamposInvalidos() =>
            string.IsNullOrWhiteSpace(Nome) ||
            string.IsNullOrWhiteSpace(Sobrenome) ||
            string.IsNullOrWhiteSpace(Email) ||
            string.IsNullOrWhiteSpace(Username) ||
            Role == null ||
            WorkHours == default;
    }
}
