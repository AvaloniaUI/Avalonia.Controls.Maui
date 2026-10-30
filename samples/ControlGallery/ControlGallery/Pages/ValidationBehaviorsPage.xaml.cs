using System.ComponentModel;
using System.Diagnostics.CodeAnalysis;
using CommunityToolkit.Maui.Behaviors;

namespace ControlGallery.Pages;

// The validation behaviors bind to the Text property of their entry by name, which TrimmerRoots.xml keeps
[UnconditionalSuppressMessage("Trimming", "IL2026", Justification = "The property the validation behaviors bind to is kept by TrimmerRoots.xml")]
public partial class ValidationBehaviorsPage : ContentPage
{
    public ValidationBehaviorsPage()
    {
        InitializeComponent();
        PasswordRules.PropertyChanged += OnPasswordRulesPropertyChanged;
    }

    private void OnPasswordTextChanged(object? sender, TextChangedEventArgs e)
    {
        ConfirmPasswordValidator.RequiredString = e.NewTextValue;
    }

    private void OnPasswordRulesPropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        if (e.PropertyName != nameof(MultiValidationBehavior.Errors))
            return;

        var errors = PasswordRules.Errors;
        PasswordRulesLabel.Text = errors is { Count: > 0 }
            ? string.Join(Environment.NewLine, errors)
            : "All rules are met.";
    }
}
