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

        public ICommand MinimizeCommand { get; }
        public ICommand CloseCommand { get; }
        public ICommand AddCommand { get; }

        public Action? MinimizeWindow { get; set; }
        public Action? CloseWindow { get; set; }

        public UserListViewModel() {
            Users = new ObservableCollection<UserModel>(GetAllUsers());

            MinimizeCommand = new RelayCommand(_ => MinimizeWindow?.Invoke());
            CloseCommand = new RelayCommand(_ => CloseWindow?.Invoke());
            AddCommand = new RelayCommand(_ => OpenCadastroModal());
        }

        private void OpenCadastroModal() {
            var cadastroView = new CadastroView
            {
                Owner = Application.Current.Windows.OfType<Window>().FirstOrDefault(w => w.IsActive)
            };
            cadastroView.ShowDialog();
        }

        private List<UserModel> GetAllUsers() {
            using var db = new AppDbContext();
            return db.Users.Where(u => u.IsActive).ToList(); // Exibe apenas ativos
        }
    }
}
