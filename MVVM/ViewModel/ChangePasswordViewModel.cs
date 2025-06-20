using System.Windows;
using System.Windows.Input;
using TCC_MVVM.Infra;
using TCC_MVVM.Model;
using TCC_MVVM.MVVM.Base;
using TCC_MVVM.MVVM.Commands;

namespace TCC_MVVM.MVVM.ViewModel
{
    /// <summary>
    /// ViewModel responsável por gerenciar a lógica de alteração de senha do usuário.
    /// </summary>
    class ChangePasswordViewModel : ViewModelBase {
        private string _currentPassword;
        private string _newPassword;
        private string _confirmPassword;
        private string _message;

        private readonly UserModel _user;

        /// <summary>
        /// Senha atual inserida pelo usuário.
        /// </summary>
        public string CurrentPassword {
            get => _currentPassword;
            set { SetField(ref _currentPassword, value, nameof(CurrentPassword)); }
        }
        /// <summary>
        /// Nova senha desejada pelo usuário.
        /// </summary>
        public string NewPassword {
            get => _newPassword;
            set { SetField(ref _newPassword, value, nameof(NewPassword)); }
        }
        /// <summary>
        /// Confirmação da nova senha.
        /// </summary>
        public string ConfirmPassword {
            get => _confirmPassword;
            set { SetField(ref _confirmPassword, value, nameof(ConfirmPassword)); }
        }
        /// <summary>
        /// Mensagem de erro exibida na interface.
        /// </summary>
        public string Message {
            get => _message;
            set { SetField(ref _message, value, nameof(Message)); }
        }

        /// <summary>
        /// Comando executado para alterar a senha.
        /// </summary>
        public ICommand ChangePasswordCommand { get; }
        /// <summary>
        /// Comando para minimizar a janela.
        /// </summary>
        public ICommand MinimizeCommand { get; }
        /// <summary>
        /// Comando para fechar a janela.
        /// </summary>
        public ICommand CloseCommand { get; }
        /// <summary>
        /// Ação executada para minimizar a janela (vinculada pela view).
        /// </summary>
        public Action? MinimizeWindow { get; set; }
        /// <summary>
        /// Ação executada para fechar a janela (vinculada pela view).
        /// </summary>
        public Action? CloseWindow { get; set; }

        /// <summary>
        /// Construtor da ViewModel responsável pela alteração de senha.
        /// </summary>
        /// <param name="user">Usuário logado que deseja alterar a senha.</param>
        public ChangePasswordViewModel(UserModel user) {
            _user = user;

            ChangePasswordCommand = new RelayCommand(_ => ExecuteChangePassword());
            MinimizeCommand = new RelayCommand(_ => MinimizeWindow?.Invoke());
            CloseCommand = new RelayCommand(_ => CloseWindow?.Invoke());
        }

        /// <summary>
        /// Executa o fluxo de alteração de senha, incluindo validações e atualização no banco.
        /// </summary>
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

        /// <summary>
        /// Realiza todas as validações de entrada antes da alteração de senha.
        /// </summary>
        /// <returns>True se todas as entradas forem válidas; false caso contrário.</returns>
        private bool ValidarEntradas() {
            if (CamposInvalidos()) return MostrarErro("Preencha todos os campos corretamente.");
            if (!BCrypt.Net.BCrypt.Verify(CurrentPassword, _user.PasswordHash)) return MostrarErro("Senha atual incorreta.");
            if (NewPassword != ConfirmPassword) return MostrarErro("A nova senha e a confirmação não coincidem.");
            return true;
        }

        /// <summary>
        /// Define a mensagem de erro na propriedade Message e retorna false.
        /// </summary>
        /// <param name="msg">Mensagem a ser exibida.</param>
        /// <returns>False, para uso em expressões condicionais.</returns>
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
