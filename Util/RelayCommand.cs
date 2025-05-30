﻿using System.Windows.Input;

namespace TCC_MVVM.Util
{
    class RelayCommand : ICommand {
        private readonly Action<object?> _execute;
        private readonly Predicate<object?>? _canExecute;

        public RelayCommand(Action execute, Func<bool>? canExecute = null)
            : this(param => execute(), canExecute == null ? null : new Predicate<object?>(_ => canExecute())) { }

        public RelayCommand(Action<object?> execute, Predicate<object?>? canExecute = null) {
            _execute = execute ?? throw new ArgumentNullException(nameof(execute));
            _canExecute = canExecute;
        }

        public event EventHandler? CanExecuteChanged {
            add => CommandManager.RequerySuggested += value;
            remove => CommandManager.RequerySuggested -= value;
        }

        public bool CanExecute(object? parameter) => _canExecute?.Invoke(parameter) ?? true;
        public void Execute(object? parameter) => _execute(parameter);
    }
}
