using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Input;
using TCC_MVVM.Infra;
using TCC_MVVM.Model;
using TCC_MVVM.Util;
using TCC_MVVM.View;

namespace TCC_MVVM.ViewModel
{
    class UserListViewModel : ViewModelBase {
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
        public ICommand AddCommand { get; }
        public ICommand EditCommand { get; }

        public Action? MinimizeWindow { get; set; }
        public Action? CloseWindow { get; set; }

        public UserListViewModel() {
            Users = new ObservableCollection<UserModel>(GetAllUsers());

            MinimizeCommand = new RelayCommand(_ => MinimizeWindow?.Invoke());
            CloseCommand = new RelayCommand(_ => CloseWindow?.Invoke());
            AddCommand = new RelayCommand(_ => OpenCadastroModal());
            EditCommand = new RelayCommand(_ => OpenEditModal(), _ => SelectedUser != null);
        }

        private void OpenCadastroModal() {
            var cadastroView = new CadastroView
            {
                Owner = Application.Current.Windows.OfType<Window>().FirstOrDefault(w => w.IsActive)
            };
            cadastroView.ShowDialog();
        }

        private void OpenEditModal() {
            if (SelectedUser == null) return;

            var viewModel = new EditViewModel(SelectedUser)
            {
                CloseWindow = () => { }, // Evita null temporariamente
                MinimizeWindow = () => { }
            };

            var editView = new EditView
            {
                DataContext = new EditViewModel(SelectedUser),
                Owner = Application.Current.Windows.OfType<Window>().FirstOrDefault(w => w.IsActive)
            };

            viewModel.CloseWindow = () => editView.Close();      // Associa corretamente após o editView estar instanciado
            viewModel.MinimizeWindow = () => editView.WindowState = WindowState.Minimized;

            editView.ShowDialog();
            RefreshUserList();
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
