using System.Windows.Input;
using TCC_MVVM.Model.Enum;
using TCC_MVVM.Model;
using TCC_MVVM.Util;
using System.Windows;
using TCC_MVVM.Infra;

namespace TCC_MVVM.ViewModel
{
    public class EditViewModel : ViewModelBase {
        private readonly UserModel _originalUser;

        public string Nome { get; set; }
        public string Sobrenome { get; set; }
        public string Email { get; set; }
        public string Username { get; set; }
        public UserRole Role { get; set; }
        public WorkHours WorkHours { get; set; }
        public string Mensagem { get; set; }

        public IEnumerable<UserRole> Roles => Enum.GetValues(typeof(UserRole)).Cast<UserRole>();
        public IEnumerable<WorkHours> WorkHourOptions => Enum.GetValues(typeof(WorkHours)).Cast<WorkHours>();

        public ICommand EditCommand { get; }

        public ICommand MinimizeCommand { get; }
        public ICommand CloseCommand { get; }
        public Action? MinimizeWindow { get; set; }
        public Action? CloseWindow { get; set; }

        public EditViewModel(UserModel user) {
            _originalUser = user;

            // Preenche os campos com os dados do usuário selecionado
            Nome = user.Name;
            Sobrenome = user.LastName;
            Email = user.Email;
            Username = user.Username;
            Role = user.Role;
            WorkHours = user.WorkHours;

            EditCommand = new RelayCommand(_ => SalvarEdicao(), _ => CanEdit());
            MinimizeCommand = new RelayCommand(_ => MinimizeWindow?.Invoke());
            CloseCommand = new RelayCommand(_ => CloseWindow?.Invoke());
        }

        private void SalvarEdicao() {
            try {
                using var db = new AppDbContext();
                var user = db.Users.FirstOrDefault(u => u.Id == _originalUser.Id);
                if (user == null) {
                    Mensagem = "Usuário não encontrado.";
                    return;
                }

                if (!EmailValido(Email)) {
                    MessageBox.Show("E-mail inválido!", "Erro", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }


                // Atualiza os dados
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
        private bool CanEdit() {
            return !string.IsNullOrWhiteSpace(Nome)
                && !string.IsNullOrWhiteSpace(Sobrenome)
                && !string.IsNullOrWhiteSpace(Email)
                && EmailValido(Email)
                && !string.IsNullOrWhiteSpace(Username);
        }

        private bool EmailValido(string email) {
            return System.Text.RegularExpressions.Regex.IsMatch(email,
                @"^[^@\s]+@[^@\s]+\.[^@\s]+$");
        }

    }
}
