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
    class UserListViewModel : ViewModelBase {
        private readonly UserModel _usuarioLogado;

        public ObservableCollection<UserModel> Users { get; set; }

        private UserModel _selectedUser;
        public UserModel SelectedUser {
            get => _selectedUser;
            set {
                _selectedUser = value;
                OnPropertyChanged(nameof(SelectedUser));
            }
        }

        public ICommand MinimizeCommand { get; }
        public ICommand CloseCommand { get; }

        public ICommand LogoutCommand { get; }

        public ICommand AddCommand { get; }
        public ICommand EditCommand { get; }
        public ICommand DeleteCommand { get; }

        public Action? MinimizeWindow { get; set; }
        public Action? CloseWindow { get; set; }

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

        public UserListViewModel() {
            Users = new ObservableCollection<UserModel>(GetAllUsers());

            MinimizeCommand = new RelayCommand(_ => MinimizeWindow?.Invoke());
            CloseCommand = new RelayCommand(_ => CloseWindow?.Invoke());

            AddCommand = new RelayCommand(_ => OpenCadastroModal());
            EditCommand = new RelayCommand(_ => OpenEditModal(), _ => SelectedUser != null);
            DeleteCommand = new RelayCommand(_ => DemitirUsuario(), _ => SelectedUser != null);

            LogoutCommand = new RelayCommand(ExecuteLogout);
        }

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

        private void OpenCadastroModal() {
            var cadastroView = new CadastroView(_usuarioLogado)
            {
                Owner = Application.Current.Windows.OfType<Window>().FirstOrDefault(w => w.IsActive)
            };
            cadastroView.ShowDialog();
            RefreshUserList();
        }

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

        private List<UserModel> GetAllUsers() {
            using var db = new AppDbContext();
            return db.Users.Where(u => u.IsActive).ToList(); // Exibe apenas ativos
        }

        public void RefreshUserList() {
            using var db = new AppDbContext();
            var users = db.Users.Where(u => u.IsActive).ToList();

            Users.Clear();
            foreach (var user in users)
                Users.Add(user);
        }

    }
}
