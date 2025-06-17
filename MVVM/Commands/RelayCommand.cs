using System.Windows.Input;

namespace TCC_MVVM.MVVM.Commands {
    /// <summary>
    /// Implementa ICommand para permitir delegação de lógica de execução e validação de comandos em ViewModels.
    /// </summary>
    class RelayCommand : ICommand {
        private readonly Action<object?> _execute;
        private readonly Predicate<object?>? _canExecute;

        /// <summary>
        /// Cria uma nova instância de RelayCommand com execução sem parâmetros.
        /// </summary>
        /// <param name="execute">A ação a ser executada.</param>
        /// <param name="canExecute">A função que determina se o comando pode ser executado.</param>
        public RelayCommand(Action execute, Func<bool>? canExecute = null)
            : this(param => execute(), canExecute == null ? null : new Predicate<object?>(_ => canExecute())) { }

        /// <summary>
        /// Cria uma nova instância de RelayCommand com execução com parâmetros.
        /// </summary>
        /// <param name="execute">A ação a ser executada.</param>
        /// <param name="canExecute">A função que determina se o comando pode ser executado.</param>
        public RelayCommand(Action<object?> execute, Predicate<object?>? canExecute = null) {
            _execute = execute ?? throw new ArgumentNullException(nameof(execute));
            _canExecute = canExecute;
        }
        /// <inheritdoc/>
        public event EventHandler? CanExecuteChanged {
            add => CommandManager.RequerySuggested += value;
            remove => CommandManager.RequerySuggested -= value;
        }

        /// <inheritdoc/>
        public bool CanExecute(object? parameter) => _canExecute?.Invoke(parameter) ?? true;
        /// <inheritdoc/>
        public void Execute(object? parameter) => _execute(parameter);
    }
}
