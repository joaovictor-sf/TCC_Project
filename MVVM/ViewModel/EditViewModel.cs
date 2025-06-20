using System.Windows.Input;
using TCC_MVVM.Model.Enum;
using TCC_MVVM.Model;
using TCC_MVVM.MVVM.Commands;
using System.Windows;
using TCC_MVVM.Infra;
using TCC_MVVM.MVVM.Base;

namespace TCC_MVVM.MVVM.ViewModel
{
    public class EditViewModel : ViewModelBase {
        private readonly UserModel _usuarioLogado;
        private readonly UserModel _originalUser;

        private string _nome;
        private string _sobrenome;
        private string _email;
        private string _username;
        private UserRole _role;
        private WorkHours _workHours;
        private string _mensagem;

        /// <summary>
        /// Nome do usuário.
        /// </summary>
        public string Nome {
            get => _nome;
            set => SetField(ref _nome, value, nameof(Nome));
        }

        /// <summary>
        /// Sobrenome do usuário.
        /// </summary>
        public string Sobrenome {
            get => _sobrenome;
            set => SetField(ref _sobrenome, value, nameof(Sobrenome));
        }

        /// <summary>
        /// E-mail do usuário.
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
        /// Papel (Role) do usuário no sistema.
        /// </summary>
        public UserRole Role {
            get => _role;
            set => SetField(ref _role, value, nameof(Role));
        }
        public WorkHours WorkHours {
            get => _workHours;
            set => SetField(ref _workHours, value, nameof(WorkHours));

        }
        public string Mensagem {
            get => _mensagem;
            set { _mensagem = value; OnPropertyChanged(nameof(Mensagem)); }
        }

        public IEnumerable<UserRole> AvailableRoles {
            get {
                if (_usuarioLogado.Role == UserRole.RH)
                    return new[] { UserRole.DEV, UserRole.RH };
                return Enum.GetValues(typeof(UserRole)).Cast<UserRole>();
            }
        }
        public IEnumerable<WorkHours> AvailableWorkHours {
            get {
                if (_usuarioLogado.Role == UserRole.RH)
                    return new[] { WorkHours.QUATRO_HORAS, WorkHours.SEIS_HORAS, WorkHours.OITO_HORAS };

                return Enum.GetValues(typeof(WorkHours)).Cast<WorkHours>();
            }
        }

        public ICommand EditCommand { get; }

        public ICommand MinimizeCommand { get; }
        public ICommand CloseCommand { get; }
        public Action? MinimizeWindow { get; set; }
        public Action? CloseWindow { get; set; }
        public EditViewModel(UserModel userEditado, UserModel usuarioLogado) {
            _originalUser = userEditado;
            _usuarioLogado = usuarioLogado;

            Nome = userEditado.Name;
            Sobrenome = userEditado.LastName;
            Email = userEditado.Email;
            Username = userEditado.Username;
            Role = userEditado.Role;
            WorkHours = userEditado.WorkHours;

            EditCommand = new RelayCommand(_ => SalvarEdicao());
            MinimizeCommand = new RelayCommand(_ => MinimizeWindow?.Invoke());
            CloseCommand = new RelayCommand(_ => CloseWindow?.Invoke());
        }

        private void SalvarEdicao() {
            try {
                using var db = new AppDbContext();
                var user = db.Users.FirstOrDefault(u => u.Id == _originalUser.Id);

                //Realisticamente, user não pode ser nulo, pois o ViewModel é inicializado com um usuário existente.
                if (user == null) {
                    Mensagem = "Usuário não encontrado.";
                    return;
                }

                if (CamposInvalidos()) {
                    Mensagem = "Preencha todos os campos corretamente.";
                    return;
                }

                if (Username != user.Username && UsernameEmUso(db)) {
                    Mensagem = "Esse username já está em uso.";
                    return;
                }

                if (EmailInvalido(Email)) {
                    Mensagem = "E-mail inválido.";
                    return;
                }

                user.Name = Nome;
                user.LastName = Sobrenome;
                user.Email = Email;
                user.Username = Username;
                user.Role = Role;
                user.WorkHours = WorkHours;

                db.SaveChanges();
                MessageBox.Show("Usuário atualizado com sucesso!", "Sucesso", MessageBoxButton.OK, MessageBoxImage.Information);

                CloseWindow?.Invoke();
            } catch (Exception ex) {
                MessageBox.Show($"Erro ao salvar alterações: {ex.Message}", "Erro", MessageBoxButton.OK, MessageBoxImage.Error);
            }
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
        /// Verifica se algum dos campos obrigatórios está inválido ou vazio.
        /// </summary>
        private bool CamposInvalidos() =>
            string.IsNullOrWhiteSpace(Nome) ||
            string.IsNullOrWhiteSpace(Sobrenome) ||
            string.IsNullOrWhiteSpace(Email) ||
            string.IsNullOrWhiteSpace(Username) ||
            Role == null ||
            WorkHours == default;

        /// <summary>
        /// Método auxiliar para setar valores de propriedades e notificar alterações.
        /// </summary>
        private void SetField<T>(ref T field, T value, string propertyName) {
            field = value;
            OnPropertyChanged(propertyName);
            CommandManager.InvalidateRequerySuggested();
        }
    }
}
