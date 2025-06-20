using System.Windows;
using System.Windows.Input;
using TCC_MVVM.Infra;
using TCC_MVVM.Model;
using TCC_MVVM.MVVM.Base;
using TCC_MVVM.MVVM.Commands;

namespace TCC_MVVM.MVVM.ViewModel
{
    class ChangePasswordViewModel : ViewModelBase {
        private string _currentPassword;
        private string _newPassword;
        private string _confirmPassword;
        private string _message;

        private readonly UserModel _user;

        public string CurrentPassword {
            get => _currentPassword;
            set { SetField(ref _currentPassword, value, nameof(CurrentPassword)); }
        }

        public string NewPassword {
            get => _newPassword;
            set { SetField(ref _newPassword, value, nameof(NewPassword)); }
        }

        public string ConfirmPassword {
            get => _confirmPassword;
            set { SetField(ref _confirmPassword, value, nameof(ConfirmPassword)); }
        }

        public string Message {
            get => _message;
            set { SetField(ref _message, value, nameof(Message)); }
        }

        public ICommand ChangePasswordCommand { get; }
        public ICommand MinimizeCommand { get; }
        public ICommand CloseCommand { get; }
        public Action? MinimizeWindow { get; set; }
        public Action? CloseWindow { get; set; }

        public ChangePasswordViewModel(UserModel user) {
            _user = user;

            ChangePasswordCommand = new RelayCommand(_ => ExecuteChangePassword());
            MinimizeCommand = new RelayCommand(_ => MinimizeWindow?.Invoke());
            CloseCommand = new RelayCommand(_ => CloseWindow?.Invoke());
        }

        private void ExecuteChangePassword() {
            if (!ValidarEntradas()) return;

            using var db = new AppDbContext();
            var userInDb = db.Users.FirstOrDefault(u => u.Id == _user.Id);
            if (userInDb == null) {
                MessageBox.Show("Usuário não encontrado.", "Erro", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            userInDb.PasswordHash = BCrypt.Net.BCrypt.HashPassword(NewPassword);
            userInDb.MustChangePassword = false;
            db.SaveChanges();

            MessageBox.Show("Senha alterada com sucesso!", "Sucesso", MessageBoxButton.OK, MessageBoxImage.Information);
            CloseWindow?.Invoke();
        }

        private bool ValidarEntradas() {
            if (CamposInvalidos()) return MostrarErro("Preencha todos os campos corretamente.");
            if (!BCrypt.Net.BCrypt.Verify(CurrentPassword, _user.PasswordHash)) return MostrarErro("Senha atual incorreta.");
            if (NewPassword != ConfirmPassword) return MostrarErro("A nova senha e a confirmação não coincidem.");
            return true;
        }

        private bool MostrarErro(string msg) {
            Message = msg;
            return false;
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
        /// Verifica se algum dos campos obrigatórios está inválido ou vazio.
        /// </summary>
        private bool CamposInvalidos() =>
            string.IsNullOrWhiteSpace(CurrentPassword) ||
            string.IsNullOrWhiteSpace(NewPassword) ||
            string.IsNullOrWhiteSpace(ConfirmPassword);
    }
}
