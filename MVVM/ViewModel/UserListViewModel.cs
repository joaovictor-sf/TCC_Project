using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Input;
using TCC_MVVM.Infra;
using TCC_MVVM.Model;
using TCC_MVVM.MVVM.Base;
using TCC_MVVM.MVVM.Commands;
using TCC_MVVM.View;

namespace TCC_MVVM.MVVM.ViewModel
{
    /// <summary>
    /// ViewModel responsável por gerenciar a listagem de usuários ativos no sistema,
    /// permitindo cadastro, edição, demissão e logout.
    /// </summary>
    class UserListViewModel : ViewModelBase {
        private readonly UserModel _usuarioLogado;

        /// <summary>
        /// Lista observável de usuários ativos exibida na interface.
        /// </summary>
        public ObservableCollection<UserModel> Users { get; set; }

        private UserModel _selectedUser;

        /// <summary>
        /// Usuário selecionado atualmente na interface.
        /// </summary>
        public UserModel SelectedUser {
            get => _selectedUser;
            set {
                _selectedUser = value;
                OnPropertyChanged(nameof(SelectedUser));
            }
        }

        /// <summary>
        /// Comando para minimizar a janela.
        /// </summary>
        public ICommand MinimizeCommand { get; }
        /// <summary>
        /// Comando para fechar a janela.
        /// </summary>
        public ICommand CloseCommand { get; }

        /// <summary>
        /// Comando para realizar logout do sistema.
        /// </summary>
        public ICommand LogoutCommand { get; }

        /// <summary>
        /// Comando para abrir a tela de cadastro de usuários.
        /// </summary>
        public ICommand AddCommand { get; }
        /// <summary>
        /// Comando para abrir a tela de edição do usuário selecionado.
        /// </summary>
        public ICommand EditCommand { get; }
        /// <summary>
        /// Comando para demitir o usuário selecionado.
        /// </summary>
        public ICommand DeleteCommand { get; }

        /// <summary>
        /// Ação a ser executada para minimizar a janela.
        /// </summary>
        public Action? MinimizeWindow { get; set; }
        /// <summary>
        /// Ação a ser executada para fechar a janela.
        /// </summary>
        public Action? CloseWindow { get; set; }

        /// <summary>
        /// Construtor que recebe o usuário logado e inicializa os comandos e dados da tela.
        /// </summary>
        /// <param name="usuarioLogado">Usuário atualmente autenticado.</param>
        public UserListViewModel(UserModel usuarioLogado) {
            _usuarioLogado = usuarioLogado;

            Users = new ObservableCollection<UserModel>(GetAllUsers());

            MinimizeCommand = new RelayCommand(_ => MinimizeWindow?.Invoke());
            CloseCommand = new RelayCommand(_ => CloseWindow?.Invoke());

            AddCommand = new RelayCommand(_ => OpenCadastroModal());
            EditCommand = new RelayCommand(_ => OpenEditModal(), _ => SelectedUser != null);
            DeleteCommand = new RelayCommand(_ => DemitirUsuario(), _ => SelectedUser != null);

            LogoutCommand = new RelayCommand(ExecuteLogout);
        }

        /// <summary>
        /// Construtor padrão necessário para evitar exceções em tempo de execução.
        /// </summary>
        [Obsolete("Use o construtor com parâmetro UserModel.")]
        public UserListViewModel() {
            Users = new ObservableCollection<UserModel>(GetAllUsers());

            MinimizeCommand = new RelayCommand(_ => MinimizeWindow?.Invoke());
            CloseCommand = new RelayCommand(_ => CloseWindow?.Invoke());

            AddCommand = new RelayCommand(_ => OpenCadastroModal());
            EditCommand = new RelayCommand(_ => OpenEditModal(), _ => SelectedUser != null);
            DeleteCommand = new RelayCommand(_ => DemitirUsuario(), _ => SelectedUser != null);

            LogoutCommand = new RelayCommand(ExecuteLogout);
        }

        /// <summary>
        /// Fecha a janela atual e redireciona o usuário para a tela de login.
        /// </summary>
        private void ExecuteLogout(object? parameter) {
            Application.Current.Dispatcher.Invoke(() =>
            {
                var loginView = new LoginView();
                if (loginView.DataContext is LoginViewModel loginVM) {
                    loginVM.Reset();
                }
                loginView.Show();

                foreach (Window window in Application.Current.Windows) {
                    if (window.DataContext == this) {
                        window.Close();
                        break;
                    }
                }
            });
        }

        /// <summary>
        /// Abre a janela de cadastro de um novo usuário.
        /// </summary>
        private void OpenCadastroModal() {
            var cadastroView = new CadastroView(_usuarioLogado)
            {
                Owner = Application.Current.Windows.OfType<Window>().FirstOrDefault(w => w.IsActive)
            };
            cadastroView.ShowDialog();
            RefreshUserList();
        }

        /// <summary>
        /// Abre a janela de edição para o usuário selecionado.
        /// </summary>
        private void OpenEditModal() {
            if (SelectedUser == null) return;

            var viewModel = new EditViewModel(SelectedUser, _usuarioLogado)
            {
                CloseWindow = () => { },
                MinimizeWindow = () => { }
            };

            var editView = new EditView
            {
                DataContext = new EditViewModel(SelectedUser, _usuarioLogado),
                Owner = Application.Current.Windows.OfType<Window>().FirstOrDefault(w => w.IsActive)
            };

            viewModel.CloseWindow = () => editView.Close();
            viewModel.MinimizeWindow = () => editView.WindowState = WindowState.Minimized;

            editView.ShowDialog();
            RefreshUserList();
        }

        /// <summary>
        /// Marca o usuário selecionado como inativo (demitido).
        /// </summary>
        private void DemitirUsuario() {
            if (SelectedUser == null) return;

            var confirm = MessageBox.Show(
                $"Deseja realmente demitir o usuário {SelectedUser.Name} {SelectedUser.LastName}?",
                "Confirmar demissão",
                MessageBoxButton.YesNo,
                MessageBoxImage.Warning);

            if (confirm != MessageBoxResult.Yes) return;

            try {
                using var db = new AppDbContext();
                var usuario = db.Users.FirstOrDefault(u => u.Id == SelectedUser.Id);
                if (usuario != null) {
                    usuario.IsActive = false;
                    db.SaveChanges();
                    RefreshUserList();
                }
            } catch (Exception ex) {
                MessageBox.Show($"Erro ao demitir o usuário: {ex.Message}", "Erro", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        /// <summary>
        /// Obtém todos os usuários ativos do banco de dados.
        /// </summary>
        private List<UserModel> GetAllUsers() {
            using var db = new AppDbContext();
            return db.Users.Where(u => u.IsActive).ToList(); // Exibe apenas ativos
        }

        /// <summary>
        /// Recarrega a lista de usuários ativos na interface.
        /// </summary>
        public void RefreshUserList() {
            using var db = new AppDbContext();
            var users = db.Users.Where(u => u.IsActive).ToList();

            Users.Clear();
            foreach (var user in users)
                Users.Add(user);
        }

    }
}
