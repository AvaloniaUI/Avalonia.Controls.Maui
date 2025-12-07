using System.Windows.Input;

namespace Avalonia.Controls.Maui.Tests.TestUtilities;

/// <summary>
/// A simple ICommand implementation for testing purposes.
/// </summary>
public class TestCommand : ICommand
{
    private readonly Action _execute;
    private readonly Func<bool>? _canExecute;

    public TestCommand(Action execute, Func<bool>? canExecute = null)
    {
        _execute = execute;
        _canExecute = canExecute;
    }

    public event EventHandler? CanExecuteChanged;

    public bool CanExecute(object? parameter) => _canExecute?.Invoke() ?? true;

    public void Execute(object? parameter) => _execute();

    public void RaiseCanExecuteChanged() => CanExecuteChanged?.Invoke(this, EventArgs.Empty);
}

/// <summary>
/// A generic ICommand implementation for testing with typed parameters.
/// </summary>
public class TestCommand<T> : ICommand
{
    private readonly Action<T?> _execute;
    private readonly Func<T?, bool>? _canExecute;

    public TestCommand(Action<T?> execute, Func<T?, bool>? canExecute = null)
    {
        _execute = execute;
        _canExecute = canExecute;
    }

    public event EventHandler? CanExecuteChanged;

    public bool CanExecute(object? parameter)
    {
        if (_canExecute == null)
            return true;

        if (parameter is T typedParam)
            return _canExecute(typedParam);

        if (parameter == null && default(T) == null)
            return _canExecute(default);

        return true;
    }

    public void Execute(object? parameter)
    {
        if (parameter is T typedParam)
            _execute(typedParam);
        else if (parameter == null && default(T) == null)
            _execute(default);
    }

    public void RaiseCanExecuteChanged() => CanExecuteChanged?.Invoke(this, EventArgs.Empty);
}
