using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace TCC_MVVM.MVVM.Base
{
    /// <summary>
    /// Classe base para todos os ViewModels, implementa INotifyPropertyChanged.
    /// </summary>
    public abstract class ViewModelBase : INotifyPropertyChanged {
        /// <inheritdoc/>
        public event PropertyChangedEventHandler? PropertyChanged;

        /// <summary>
        /// Notifica que uma propriedade foi alterada.
        /// </summary>
        /// <param name="propertyName">Nome da propriedade. Preenchido automaticamente pelo compilador.</param>
        public void OnPropertyChanged([CallerMemberName] string? propertyName = null) {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        /// <summary>
        /// Define o valor de uma propriedade e notifica a alteração se necessário.
        /// </summary>
        /// <typeparam name="T">Tipo do valor.</typeparam>
        /// <param name="field">Campo de backing da propriedade.</param>
        /// <param name="value">Novo valor.</param>
        /// <param name="propertyName">Nome da propriedade. Preenchido automaticamente pelo compilador.</param>
        protected bool SetProperty<T>(ref T field, T value, [CallerMemberName] string? propertyName = null) {
            if (EqualityComparer<T>.Default.Equals(field, value)) return false;
            field = value;
            OnPropertyChanged(propertyName);
            return true;
        }
    }
}
