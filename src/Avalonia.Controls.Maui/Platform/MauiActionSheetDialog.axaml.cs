using Avalonia.Interactivity;
using System.Collections.ObjectModel;
using System.Windows.Input;

namespace Avalonia.Controls.Maui.Platform;

public partial class MauiActionSheetDialog : UserControl
{
    public MauiActionSheetDialog()
    {
        InitializeComponent();
    }

    public string? Result { get; private set; }

    public bool HasTitle => !string.IsNullOrEmpty(Title);

    public string? Title { get; set; }

    public string? CancelButtonText { get; set; }
    public bool HasCancelButton => !string.IsNullOrEmpty(CancelButtonText);
    public ICommand CancelCommand => new RelayCommand<object>(_ => CloseWithResult(CancelButtonText));

    public string? DestructionButtonText { get; set; }
    public string? DestructionButtonFormatted => DestructionButtonText;
    public bool HasDestructionButton => !string.IsNullOrEmpty(DestructionButtonText);
    public ICommand DestructionCommand => new RelayCommand<object>(_ => CloseWithResult(DestructionButtonText));

    public ObservableCollection<ActionSheetButtonViewModel>? Buttons { get; private set; }
    
    public event Action<string?>? OnResult;

    public MauiActionSheetDialog(string title, string cancel, string destruction, string[]? buttons) : this()
    {
        Title = title;
        CancelButtonText = string.IsNullOrEmpty(cancel) ? null : cancel;
        DestructionButtonText = string.IsNullOrEmpty(destruction) ? null : destruction;
        
        Buttons = new ObservableCollection<ActionSheetButtonViewModel>();
        if (buttons != null)
        {
            foreach (var btn in buttons)
            {
                Buttons.Add(new ActionSheetButtonViewModel(btn, new RelayCommand<string>(OnButtonClicked)));
            }
        }

        DataContext = null;
        DataContext = this;
    }

    private void OnButtonClicked(string text)
    {
        CloseWithResult(text);
    }

    private void OnButtonClicked(object? sender, RoutedEventArgs e)
    {
        if (sender is Button button)
        {
            var text = button.CommandParameter as string ?? button.Content?.ToString();
            CloseWithResult(text);
        }
    }

    private void CloseWithResult(string? result)
    {
        Result = result;
        OnResult?.Invoke(result);
    }
}

public class ActionSheetButtonViewModel
{
    public string Text { get; }
    public ICommand Command { get; }

    public ActionSheetButtonViewModel(string text, ICommand command)
    {
        Text = text;
        Command = command;
    }
}

// Simple RelayCommand implementation to avoid dependencies if not available
internal class RelayCommand<T> : ICommand
{
    private readonly Action<T> _execute;
    private readonly Predicate<T>? _canExecute;

    public RelayCommand(Action<T> execute, Predicate<T>? canExecute = null)
    {
        _execute = execute ?? throw new ArgumentNullException(nameof(execute));
        _canExecute = canExecute;
    }

#pragma warning disable CS0067
    public event EventHandler? CanExecuteChanged;
#pragma warning restore CS0067

    public bool CanExecute(object? parameter)
    {
        return _canExecute == null || _canExecute((T)parameter!);
    }

    public void Execute(object? parameter)
    {
        _execute((T)parameter!);
    }
}
