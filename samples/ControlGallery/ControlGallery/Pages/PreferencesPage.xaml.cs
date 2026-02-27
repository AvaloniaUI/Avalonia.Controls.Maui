namespace ControlGallery.Pages;

public partial class PreferencesPage : ContentPage
{
    const string StringKey = "demo_string";
    const string CounterKey = "demo_counter";
    const string BoolKey = "demo_notifications";
    const string DateTimeKey = "demo_datetime";
    const string SharedKey = "demo_shared";
    const string SharedContainerName = "my_container";

    public PreferencesPage()
    {
        InitializeComponent();
        LoadInitialState();
    }

    void LoadInitialState()
    {
        // Restore counter
        var count = Preferences.Default.Get(CounterKey, 0);
        CounterLabel.Text = $"Count: {count}";

        // Restore bool
        var notifications = Preferences.Default.Get(BoolKey, false);
        BoolSwitch.IsToggled = notifications;
        BoolResultLabel.Text = $"Stored: {notifications}";

        // Restore string
        if (Preferences.Default.ContainsKey(StringKey))
        {
            var str = Preferences.Default.Get(StringKey, "");
            StringResultLabel.Text = $"Value: {str}";
            StringEntry.Text = str;
        }
    }

    // --- String ---

    void OnSaveStringClicked(object? sender, EventArgs e)
    {
        var value = StringEntry.Text ?? "";
        Preferences.Default.Set(StringKey, value);
        StringResultLabel.Text = $"Value: {value} (saved)";
    }

    void OnLoadStringClicked(object? sender, EventArgs e)
    {
        var value = Preferences.Default.Get(StringKey, "(not set)");
        StringResultLabel.Text = $"Value: {value}";
        StringEntry.Text = value;
    }

    void OnRemoveStringClicked(object? sender, EventArgs e)
    {
        Preferences.Default.Remove(StringKey);
        StringResultLabel.Text = "Value: (removed)";
        StringEntry.Text = "";
    }

    // --- Counter (int) ---

    void OnIncrementClicked(object? sender, EventArgs e)
    {
        var count = Preferences.Default.Get(CounterKey, 0);
        count++;
        Preferences.Default.Set(CounterKey, count);
        CounterLabel.Text = $"Count: {count}";
    }

    void OnResetCounterClicked(object? sender, EventArgs e)
    {
        Preferences.Default.Remove(CounterKey);
        CounterLabel.Text = "Count: 0";
    }

    // --- Bool ---

    void OnBoolSwitchToggled(object? sender, ToggledEventArgs e)
    {
        Preferences.Default.Set(BoolKey, e.Value);
        BoolResultLabel.Text = $"Stored: {e.Value}";
    }

    // --- DateTime ---

    void OnSaveDateTimeClicked(object? sender, EventArgs e)
    {
        var now = DateTime.Now;
        Preferences.Default.Set(DateTimeKey, now);
        DateTimeResultLabel.Text = $"Saved: {now:G}";
    }

    void OnLoadDateTimeClicked(object? sender, EventArgs e)
    {
        if (Preferences.Default.ContainsKey(DateTimeKey))
        {
            var dt = Preferences.Default.Get(DateTimeKey, DateTime.MinValue);
            DateTimeResultLabel.Text = $"Saved: {dt:G}";
        }
        else
        {
            DateTimeResultLabel.Text = "Saved: (none)";
        }
    }

    // --- Shared Containers ---

    void OnSaveSharedClicked(object? sender, EventArgs e)
    {
        Preferences.Default.Set(SharedKey, "default_value");
        Preferences.Default.Set(SharedKey, "named_value", SharedContainerName);

        var defaultVal = Preferences.Default.Get(SharedKey, "");
        var namedVal = Preferences.Default.Get(SharedKey, "", SharedContainerName);

        SharedDefaultLabel.Text = $"Default: {defaultVal}";
        SharedNamedLabel.Text = $"Named ({SharedContainerName}): {namedVal}";
    }

    // --- Clear All ---

    void OnClearAllClicked(object? sender, EventArgs e)
    {
        Preferences.Default.Clear();
        Preferences.Default.Clear(SharedContainerName);

        ClearResultLabel.Text = "All preferences cleared.";
        CounterLabel.Text = "Count: 0";
        StringResultLabel.Text = "Value: --";
        BoolSwitch.IsToggled = false;
        BoolResultLabel.Text = "Stored: --";
        DateTimeResultLabel.Text = "Saved: --";
        SharedDefaultLabel.Text = "Default: --";
        SharedNamedLabel.Text = "Named: --";
        StringEntry.Text = "";
    }
}
