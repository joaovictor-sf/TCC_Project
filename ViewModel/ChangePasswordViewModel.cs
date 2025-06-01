using System.Windows;
using System.Windows.Input;
using TCC_MVVM.Infra;
using TCC_MVVM.Model;
using TCC_MVVM.Util;

namespace TCC_MVVM.ViewModel
{
    class ChangePasswordViewModel : ViewModelBase {
        private string _currentPassword;
        private string _newPassword;
        private string _confirmPassword;
        private string _message;

        private readonly UserModel _user;

        public string CurrentPassword {
            get => _currentPassword;
            set { _currentPassword = value; OnPropertyChanged(nameof(CurrentPassword)); }
        }

        public string NewPassword {
            get => _newPassword;
            set { _newPassword = value; OnPropertyChanged(nameof(NewPassword)); }
        }

        public string ConfirmPassword {
            get => _confirmPassword;
            set { _confirmPassword = value; OnPropertyChanged(nameof(ConfirmPassword)); }
        }

        public string Message {
            get => _message;
            set { _message = value; OnPropertyChanged(nameof(Message)); }
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
            if (string.IsNullOrWhiteSpace(CurrentPassword) ||
                string.IsNullOrWhiteSpace(NewPassword) ||
                string.IsNullOrWhiteSpace(ConfirmPassword)) {
                MessageBox.Show("Preencha todos os campos.", "Erro", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (!BCrypt.Net.BCrypt.Verify(CurrentPassword, _user.PasswordHash)) {
                MessageBox.Show("Senha atual incorreta.", "Erro", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (NewPassword != ConfirmPassword) {
                MessageBox.Show("A nova senha e a confirmação não coincidem.", "Erro", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

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
    }
}
